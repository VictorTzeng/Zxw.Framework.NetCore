using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zxw.Framework.AI.Abstractions;
using Zxw.Framework.AI.Credentials;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.AI.Clients
{
    /// <summary>
    /// Raised when OrcaRouter rejects the credential. This is terminal: OrcaRouter keys are
    /// durable and there is no refresh grant, so the user must reconnect or supply a new key.
    /// The message never contains the key.
    /// <para>
    /// Derives from <see cref="InvalidOperationException"/>: an unusable credential is an invalid
    /// operation for the caller, not a transport fault.
    /// </para>
    /// </summary>
    public class OrcaAuthenticationException : InvalidOperationException
    {
        public OrcaAuthenticationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// OrcaRouter OpenAI 兼容 Chat Completions 客户端。
    /// <para>
    /// The credential comes from <see cref="IOrcaCredentialStore"/>, the same seam a pasted API key
    /// and an OAuth 2.0 + PKCE authorization both feed. This client cannot tell which one produced
    /// the key, by design.
    /// </para>
    /// </summary>
    public class OrcaRouterChatClient : IChatClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OrcaRouterOptions _options;
        private readonly IOrcaCredentialStore _credentialStore;

        public OrcaRouterChatClient(IHttpClientFactory httpClientFactory, IOptions<OrcaRouterOptions> options)
            : this(httpClientFactory, options, null)
        {
        }

        public OrcaRouterChatClient(
            IHttpClientFactory httpClientFactory,
            IOptions<OrcaRouterOptions> options,
            IOrcaCredentialStore credentialStore)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _credentialStore = credentialStore;
        }

        public async Task<ChatCompletionResponse> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            Validate(request);

            var credential = ResolveCredential();
            var client = CreateClient(credential);
            using var httpRequest = CreateHttpRequest(request, stream: false);
            using var response = await client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                HandleFailure(response.StatusCode, body, credential);
            }

            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(body, JsonOptions);
            if (result == null)
                throw new InvalidOperationException("OrcaRouter 返回了空的 chat completion 响应。");
            return result;
        }

        public async IAsyncEnumerable<string> CompleteStreamingAsync(
            ChatCompletionRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Validate(request);

            var credential = ResolveCredential();
            var client = CreateClient(credential);
            using var httpRequest = CreateHttpRequest(request, stream: true);
            using var response = await client
                .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                HandleFailure(response.StatusCode, errorBody, credential);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null)
                    yield break;
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (!line.StartsWith("data:", StringComparison.Ordinal))
                    continue;

                var payload = line.Substring("data:".Length).Trim();
                if (payload == "[DONE]")
                    yield break;

                ChatCompletionResponse chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<ChatCompletionResponse>(payload, JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                var delta = chunk?.Choices != null && chunk.Choices.Count > 0
                    ? chunk.Choices[0].Delta?.Content
                    : null;
                if (!string.IsNullOrEmpty(delta))
                    yield return delta;
            }
        }

        private static void Validate(ChatCompletionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.Messages == null || request.Messages.Count == 0)
                throw new ArgumentException("Messages 不能为空。", nameof(request));
        }

        /// <summary>
        /// Resolve the credential through the seam. When no store was supplied, the configured
        /// value is used directly so an existing <c>AddOrcaRouter()</c> host keeps working.
        /// </summary>
        private OrcaCredential ResolveCredential()
        {
            if (_credentialStore != null)
            {
                var credential = _credentialStore.Load();
                if (credential != null && credential.HasKey)
                    return credential;
            }
            else
            {
                var apiKey = _options.ResolveApiKey();
                if (!string.IsNullOrWhiteSpace(apiKey))
                    return new OrcaCredential(apiKey, OrcaCredentialSource.ApiKey);
            }

            throw new InvalidOperationException(
                $"未配置 OrcaRouter API Key。请设置 {nameof(OrcaRouterOptions)}.{nameof(OrcaRouterOptions.ApiKey)} 或环境变量 {OrcaRouterOptions.ApiKeyEnvironmentVariable}，或使用 Connect with OrcaRouter。");
        }

        /// <summary>
        /// Terminal reauthentication. A rejected credential marks exactly the generation that made
        /// the rejected request, so a late failure raised by an older request cannot mark a
        /// credential that has since been replaced. No refresh is attempted: OrcaRouter does not
        /// issue refreshable tokens.
        /// </summary>
        private void HandleFailure(HttpStatusCode statusCode, string body, OrcaCredential credential)
        {
            var safeBody = OrcaSecretRedaction.Scrub(Truncate(body, 500), credential?.ApiKey);

            if ((int)statusCode == 401 || (int)statusCode == 403)
            {
                var marked = _credentialStore?.MarkNeedsReauth(credential?.Generation) ?? false;

                throw new OrcaAuthenticationException(
                    $"OrcaRouter rejected the credential ({(int)statusCode}). " +
                    (marked
                        ? "The account has been marked for reauthentication and will not be used until a new login succeeds. "
                        : "The account is already marked for reauthentication. ") +
                    "Reconnect with OrcaRouter or supply a new API key. OrcaRouter keys are durable and are not refreshed.");
            }

            throw new HttpRequestException(
                $"OrcaRouter chat completion failed: {(int)statusCode} {statusCode}. Body: {safeBody}");
        }

        private HttpClient CreateClient(OrcaCredential credential)
        {
            var client = _httpClientFactory.CreateClient(OrcaRouterOptions.HttpClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credential.ApiKey);
            return client;
        }

        private HttpRequestMessage CreateHttpRequest(ChatCompletionRequest request, bool stream)
        {
            var model = string.IsNullOrWhiteSpace(request.Model) ? _options.DefaultModel : request.Model;
            var fallback = request.FallbackModels != null && request.FallbackModels.Count > 0
                ? request.FallbackModels
                : _options.FallbackModels;

            var payload = new ChatCompletionPayload
            {
                Model = model,
                Messages = request.Messages,
                Stream = stream,
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                Models = fallback != null && fallback.Count > 0 ? fallback : null
            };

            var baseUrl = _options
                .ResolveApiBaseUrl()
                .TrimEnd('/');
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions")
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };

            var referer = _options.ResolveHttpReferer();
            if (!string.IsNullOrWhiteSpace(referer))
                httpRequest.Headers.TryAddWithoutValidation("HTTP-Referer", referer);
            if (!string.IsNullOrWhiteSpace(_options.AppTitle))
                httpRequest.Headers.TryAddWithoutValidation("X-Title", _options.AppTitle);

            return httpRequest;
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
                return value;
            return value.Substring(0, max) + "...";
        }

        private sealed class ChatCompletionPayload
        {
            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("messages")]
            public IList<ChatMessage> Messages { get; set; }

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }

            [JsonPropertyName("temperature")]
            public double? Temperature { get; set; }

            [JsonPropertyName("max_tokens")]
            public int? MaxTokens { get; set; }

            /// <summary>
            /// OrcaRouter fallback 链（对应 SDK extra_body.models）。
            /// </summary>
            [JsonPropertyName("models")]
            public IList<string> Models { get; set; }
        }
    }
}
