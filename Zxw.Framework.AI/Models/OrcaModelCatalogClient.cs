using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Zxw.Framework.AI.Credentials;
using Zxw.Framework.AI.Models;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.AI.Models
{
    /// <summary>
    /// Reads the model catalog from the configured OrcaRouter inference origin.
    /// <para>
    /// This runs server-side and holds the API key; it returns model metadata only. The key is
    /// never part of any value this type produces.
    /// </para>
    /// </summary>
    public sealed class OrcaModelCatalogClient
    {
        /// <summary>Upper bound on how many catalog entries are read from one response.</summary>
        public const int MaxModels = 400;

        /// <summary>Upper bound on the accepted response size, in bytes.</summary>
        public const int MaxResponseBytes = 8 * 1024 * 1024;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly OrcaRouterOptions _options;

        public OrcaModelCatalogClient(HttpClient httpClient, OrcaRouterOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Discover the models usable for one entry point. The credential is passed in, so this
        /// type has no opinion about whether it came from a pasted key or a PKCE authorization.
        /// </summary>
        public async Task<OrcaModelCatalog> DiscoverAsync(
            OrcaCredential credential,
            OrcaModelCapability capability,
            IReadOnlyList<string> requiredInputModalities = null,
            CancellationToken cancellationToken = default)
        {
            if (credential == null || !credential.HasKey)
            {
                return new OrcaModelCatalog
                {
                    Capability = capability,
                    Status = OrcaCatalogStatus.SeedFallback,
                    Error = OrcaCatalogError.AuthRequired,
                    Message = "No OrcaRouter credential is configured; showing the verified fallback list.",
                    Models = OrcaVerifiedSeed.For(capability, requiredInputModalities)
                };
            }

            var capabilityQuery = OrcaModelCapabilityMap.UsesCapabilityQuery(capability)
                ? OrcaModelCapabilityMap.ToQueryValue(capability)
                : null;

            var uri = OrcaAuthEndpoints.BuildModelsUri(
                _options.ResolveApiBaseUrl(),
                capabilityQuery);

            HttpResponseMessage response;
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credential.ApiKey);
                    response = await _httpClient.SendAsync(
                        request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                return Fallback(capability, requiredInputModalities, OrcaCatalogError.Network,
                    "Model discovery timed out; showing the verified fallback list.");
            }
            catch (HttpRequestException)
            {
                return Fallback(capability, requiredInputModalities, OrcaCatalogError.Network,
                    "Could not reach the OrcaRouter model catalog; showing the verified fallback list.");
            }

            using (response)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return Fallback(capability, requiredInputModalities, OrcaCatalogError.AuthRejected,
                        "OrcaRouter rejected the credential, so the model list could not be loaded. " +
                        "Reconnect the account or replace the API key.");
                }

                if ((int)response.StatusCode == 429)
                {
                    return Fallback(capability, requiredInputModalities, OrcaCatalogError.RateLimited,
                        "OrcaRouter rate limited model discovery; showing the verified fallback list.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    return Fallback(capability, requiredInputModalities, OrcaCatalogError.Network,
                        $"Model discovery failed with HTTP {(int)response.StatusCode}; showing the verified fallback list.");
                }

                string body;
                try
                {
                    body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    return Fallback(capability, requiredInputModalities, OrcaCatalogError.Network,
                        "The OrcaRouter model catalog response could not be read; showing the verified fallback list.");
                }

                if (body == null || body.Length > MaxResponseBytes)
                {
                    return Fallback(capability, requiredInputModalities, OrcaCatalogError.Network,
                        "The OrcaRouter model catalog response was empty or oversized; showing the verified fallback list.");
                }

                IList<OrcaModelInfo> parsed;
                try
                {
                    parsed = Parse(body);
                }
                catch (JsonException)
                {
                    return Fallback(capability, requiredInputModalities, OrcaCatalogError.Network,
                        "The OrcaRouter model catalog response could not be parsed; showing the verified fallback list.");
                }

                var filtered = OrcaModelFilter.Select(capability, parsed, requiredInputModalities);

                // Live discovery is authoritative: the verified seed is not merged into it. A model
                // the API did not return is not offered, even if the seed knows about it.
                return new OrcaModelCatalog
                {
                    Capability = capability,
                    Status = OrcaCatalogStatus.Live,
                    Error = OrcaCatalogError.None,
                    Models = filtered
                };
            }
        }

        /// <summary>Parse the catalog envelope, skipping any record that is not a usable model.</summary>
        public static IList<OrcaModelInfo> Parse(string body)
        {
            var result = new List<OrcaModelInfo>();
            using (var document = JsonDocument.Parse(body))
            {
                JsonElement array;
                if (document.RootElement.ValueKind == JsonValueKind.Array)
                {
                    array = document.RootElement;
                }
                else if (document.RootElement.ValueKind == JsonValueKind.Object &&
                         document.RootElement.TryGetProperty("data", out var data) &&
                         data.ValueKind == JsonValueKind.Array)
                {
                    array = data;
                }
                else
                {
                    return result;
                }

                foreach (var element in array.EnumerateArray())
                {
                    if (result.Count >= MaxModels)
                        break;

                    var model = ReadModel(element);
                    if (model != null)
                        result.Add(model);
                }
            }

            return result;
        }

        private static OrcaModelInfo ReadModel(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return null;

            if (!element.TryGetProperty("id", out var idProperty) || idProperty.ValueKind != JsonValueKind.String)
                return null;

            // The id is preserved verbatim, vendor/model namespace included.
            var id = idProperty.GetString();
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var model = new OrcaModelInfo { Id = id };

            if (element.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
                model.DisplayName = name.GetString();
            if (string.IsNullOrWhiteSpace(model.DisplayName))
                model.DisplayName = id;

            model.ContextLength = ReadInt(element, "context_length") ?? ReadInt(element, "max_input_tokens");
            model.MaxCompletionTokens = ReadInt(element, "max_completion_tokens");

            if (element.TryGetProperty("supported_endpoint_types", out var endpoints) &&
                endpoints.ValueKind == JsonValueKind.Array)
            {
                model.SupportedEndpointTypes = ReadStringArray(endpoints);
            }

            if (element.TryGetProperty("architecture", out var architecture) &&
                architecture.ValueKind == JsonValueKind.Object)
            {
                if (architecture.TryGetProperty("input_modalities", out var input) &&
                    input.ValueKind == JsonValueKind.Array)
                {
                    model.InputModalities = ReadStringArray(input);
                }

                if (architecture.TryGetProperty("output_modalities", out var output) &&
                    output.ValueKind == JsonValueKind.Array)
                {
                    model.OutputModalities = ReadStringArray(output);
                }
            }

            return model;
        }

        private static int? ReadInt(JsonElement element, string property)
        {
            if (!element.TryGetProperty(property, out var value))
                return null;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                return number;
            if (value.ValueKind == JsonValueKind.String &&
                int.TryParse(value.GetString(), out var parsed))
                return parsed;
            return null;
        }

        private static IList<string> ReadStringArray(JsonElement array)
        {
            var values = new List<string>();
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var value = item.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                        values.Add(value);
                }
            }

            return values;
        }

        private static OrcaModelCatalog Fallback(
            OrcaModelCapability capability,
            IReadOnlyList<string> requiredInputModalities,
            OrcaCatalogError error,
            string message) => new OrcaModelCatalog
        {
            Capability = capability,
            Status = OrcaCatalogStatus.SeedFallback,
            Error = error,
            Message = message,
            Models = OrcaVerifiedSeed.For(capability, requiredInputModalities)
        };
    }
}
