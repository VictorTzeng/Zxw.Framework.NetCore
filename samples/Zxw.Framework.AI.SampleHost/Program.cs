using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zxw.Framework.AI.Abstractions;
using Zxw.Framework.AI.Clients;
using Zxw.Framework.AI.Credentials;
using Zxw.Framework.AI.Extensions;
using Zxw.Framework.AI.Models;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.AI.SampleHost
{
    /// <summary>
    /// A minimal host that exercises the OrcaRouter integration end to end.
    /// <para>
    /// Two entry points into the same library code:
    /// </para>
    /// <list type="bullet">
    /// <item>the web configuration UI, which is what the automated UI test drives;</item>
    /// <item><c>--live-check</c>, which runs a real catalog read and a real completion through the
    /// library's provider path.</item>
    /// </list>
    /// The API key lives only on the server. Every response carries model metadata, never a key.
    /// </summary>
    public static class Program
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static async Task<int> Main(string[] args)
        {
            if (args.Any(a => string.Equals(a, "--live-check", StringComparison.OrdinalIgnoreCase)))
                return await LiveCheckAsync(args).ConfigureAwait(false);

            var builder = WebApplication.CreateBuilder(args);

            // The API key is read from the host's own configuration / environment by the library.
            builder.Services.AddOrcaRouter();
            builder.Services.AddSingleton<OrcaConnectSessionManager>();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            MapConfigurationEndpoints(app);
            MapModelEndpoints(app);

            var url = GetArgument(args, "--urls") ?? "http://127.0.0.1:5080";
            app.Urls.Add(url);
            Console.WriteLine($"OrcaRouter sample host listening on {url}");
            await app.RunAsync().ConfigureAwait(false);
            return 0;
        }

        private static void MapConfigurationEndpoints(WebApplication app)
        {
            // Secret-free status for both authentication choices.
            app.MapGet("/api/orcarouter/status", (OrcaApiKeyCredentialProvider apiKey,
                                                  OrcaPkceCredentialProvider pkce,
                                                  OrcaConnectSessionManager sessions) =>
            {
                return Results.Json(new
                {
                    provider = new
                    {
                        id = "orcarouter",
                        name = "OrcaRouter",
                        baseUrl = OrcaAuthEndpoints.DefaultApiBaseUrl,
                        authBaseUrl = OrcaAuthEndpoints.DefaultAuthBaseUrl,
                        keyConsoleUrl = "https://www.orcarouter.ai/console/authorized-apps"
                    },
                    // The two choices are reported separately and never collapsed into one.
                    apiKey = Describe(apiKey.GetStatus()),
                    pkce = Describe(pkce.GetStatus()),
                    connectBusy = sessions.IsBusy
                }, JsonOptions);
            });

            // Apply a pasted or configured key.
            app.MapPost("/api/orcarouter/apikey", async (HttpContext context,
                                                         OrcaApiKeyCredentialProvider apiKey) =>
            {
                var body = await ReadJsonAsync(context).ConfigureAwait(false);
                var key = body.TryGetProperty("apiKey", out var value) ? value.GetString() : null;
                var status = apiKey.ApplyKey(key);
                return Results.Json(new { apiKey = Describe(status) }, JsonOptions);
            });

            // Clear it again.
            app.MapDelete("/api/orcarouter/apikey", (OrcaApiKeyCredentialProvider apiKey) =>
            {
                apiKey.Clear();
                return Results.Json(new { apiKey = Describe(apiKey.GetStatus()) }, JsonOptions);
            });

            // Start a PKCE authorization. The verifier stays in this process.
            app.MapPost("/api/orcarouter/connect", (OrcaPkceCredentialProvider pkce,
                                                     OrcaConnectSessionManager sessions) =>
            {
                var session = pkce.Start();
                var attempt = sessions.Begin(session, pkce.Id);
                return Results.Json(new
                {
                    attemptId = attempt.AttemptId,
                    generation = attempt.Generation,
                    authorizeUrl = attempt.AuthorizeUrl,
                    instructions = attempt.Instructions,
                    provider = attempt.Provider
                }, JsonOptions);
            });

            // Redeem the code the consent screen showed.
            app.MapPost("/api/orcarouter/connect/complete", async (HttpContext context,
                                                                   OrcaConnectSessionManager sessions) =>
            {
                var body = await ReadJsonAsync(context).ConfigureAwait(false);
                var attemptId = body.TryGetProperty("attemptId", out var a) ? a.GetString() : null;
                var code = body.TryGetProperty("code", out var c) ? c.GetString() : null;

                if (!sessions.TryGetCurrent(attemptId, out var attempt))
                {
                    return Results.Json(new
                    {
                        outcome = "stale",
                        message = "That sign-in is no longer current. Start a new one."
                    }, JsonOptions);
                }

                var result = await attempt.Session.CompleteAsync(code).ConfigureAwait(false);

                // Success, denial, and exchange failure all release the lock.
                sessions.Release(attemptId);

                return Results.Json(new
                {
                    outcome = result.Succeeded ? "connected" : "failed",
                    message = result.Status.Message,
                    generation = attempt.Generation,
                    pkce = Describe(result.Status)
                }, JsonOptions);
            });

            // Explicit cancel, and the cancellation the page sends on pagehide.
            app.MapPost("/api/orcarouter/connect/cancel", async (HttpContext context,
                                                                 OrcaConnectSessionManager sessions) =>
            {
                // Accepts both a JSON body and a sendBeacon blob.
                var attemptId = await ReadAttemptIdAsync(context).ConfigureAwait(false);
                sessions.Release(attemptId);
                return Results.Json(new { released = true, connectBusy = sessions.IsBusy }, JsonOptions);
            });
        }

        private static void MapModelEndpoints(WebApplication app)
        {
            // The model list a selector binds to. The key never reaches the browser.
            app.MapGet("/api/orcarouter/models", async (string capability,
                                                        string modalities,
                                                        OrcaModelSelector selector) =>
            {
                var parsed = ParseCapability(capability);
                if (parsed == null)
                    return Results.Json(new { error = "unknown capability" }, JsonOptions, statusCode: 400);

                var required = string.IsNullOrWhiteSpace(modalities)
                    ? null
                    : modalities.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(m => m.Trim())
                        .Where(m => m.Length > 0)
                        .ToList();

                var catalog = await selector
                    .GetOptionsAsync(parsed.Value, required)
                    .ConfigureAwait(false);

                return Results.Json(new
                {
                    capability = OrcaModelCapabilityMap.ToId(parsed.Value),
                    status = catalog.Status.ToString(),
                    degraded = catalog.IsDegraded,
                    authoritative = catalog.IsAuthoritative,
                    error = catalog.Error.ToString(),
                    message = catalog.Message,
                    requiredModalities = required,
                    models = catalog.Models.Select(m => new
                    {
                        id = m.Id,
                        displayName = m.DisplayName,
                        contextLength = m.ContextLength,
                        maxCompletionTokens = m.MaxCompletionTokens,
                        inputModalities = m.InputModalities,
                        reasoningEfforts = m.ReasoningEfforts,
                        fromSeed = m.FromSeed
                    })
                }, JsonOptions);
            });
        }

        private static OrcaModelCapability? ParseCapability(string value)
        {
            switch ((value ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "chat": return OrcaModelCapability.Chat;
                case "multimodal": return OrcaModelCapability.Multimodal;
                case "embedding": return OrcaModelCapability.Embedding;
                case "image": return OrcaModelCapability.ImageGeneration;
                case "video": return OrcaModelCapability.VideoGeneration;
                case "rerank": return OrcaModelCapability.Rerank;
                default: return null;
            }
        }

        private static object Describe(OrcaCredentialStatus status) => new
        {
            state = status.State.ToString(),
            source = status.Source.ToString(),
            maskedKey = status.MaskedKey,
            accountId = status.AccountId,
            scope = status.Scope,
            message = status.Message,
            canClear = status.CanClear
        };

        private static async Task<JsonElement> ReadJsonAsync(HttpContext context)
        {
            using var document = await JsonDocument
                .ParseAsync(context.Request.Body)
                .ConfigureAwait(false);
            return document.RootElement.Clone();
        }

        private static async Task<string> ReadAttemptIdAsync(HttpContext context)
        {
            try
            {
                if (context.Request.HasFormContentType)
                {
                    var form = await context.Request.ReadFormAsync().ConfigureAwait(false);
                    return form["attemptId"].ToString();
                }

                using var reader = new StreamReader(context.Request.Body);
                var raw = await reader.ReadToEndAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(raw))
                    return null;

                // A sendBeacon may deliver JSON or a form-encoded blob.
                if (raw.TrimStart().StartsWith("{", StringComparison.Ordinal))
                {
                    using var document = JsonDocument.Parse(raw);
                    return document.RootElement.TryGetProperty("attemptId", out var value)
                        ? value.GetString()
                        : null;
                }

                foreach (var pair in raw.Split('&'))
                {
                    var parts = pair.Split(new[] { '=' }, 2);
                    if (parts.Length == 2 && parts[0] == "attemptId")
                        return Uri.UnescapeDataString(parts[1]);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetArgument(string[] args, string name)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    return args[i + 1];
            }

            return null;
        }

        /// <summary>
        /// Real catalog read and a real completion, both through the library's provider path.
        /// Prints ids, counts and text only — never a credential.
        /// </summary>
        private static async Task<int> LiveCheckAsync(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddOrcaRouter();
            using var host = builder.Build();

            var catalog = host.Services.GetRequiredService<OrcaModelCatalogClient>();
            var selector = host.Services.GetRequiredService<OrcaModelSelector>();
            var store = host.Services.GetRequiredService<IOrcaCredentialStore>();
            var chat = host.Services.GetRequiredService<IChatClient>();

            var credential = store.Load();
            Console.WriteLine($"credential present: {credential != null && credential.HasKey}");
            Console.WriteLine($"credential source:  {credential?.Source.ToString() ?? "none"}");
            Console.WriteLine($"masked:             {store.GetStatus().MaskedKey}");

            // 1. Live catalog, per capability.
            foreach (var capability in new[]
                     {
                         OrcaModelCapability.Chat,
                         OrcaModelCapability.Multimodal,
                         OrcaModelCapability.Embedding,
                         OrcaModelCapability.ImageGeneration,
                         OrcaModelCapability.VideoGeneration,
                         OrcaModelCapability.Rerank
                     })
            {
                var modalities = capability == OrcaModelCapability.Multimodal
                    ? new[] { "image" }
                    : null;

                var result = await selector.GetOptionsAsync(capability, modalities).ConfigureAwait(false);
                Console.WriteLine(
                    $"catalog {OrcaModelCapabilityMap.ToId(capability),-16} status={result.Status,-13} " +
                    $"count={result.Models.Count,-4} error={result.Error}");

                if (capability == OrcaModelCapability.Chat && result.Models.Count > 0)
                {
                    Console.WriteLine("  chat sample: " +
                        string.Join(", ", result.Models.Take(5).Select(m => m.Id)));
                    var unnamespaced = result.Models.Where(m => !m.Id.Contains('/')).ToList();
                    Console.WriteLine($"  chat ids without a vendor namespace: {unnamespaced.Count} " +
                        (unnamespaced.Count > 0
                            ? "(" + string.Join(", ", unnamespaced.Take(5).Select(m => m.Id)) + ")"
                            : string.Empty));
                }

                if (capability == OrcaModelCapability.Multimodal && result.Models.Count > 0)
                {
                    var undeclared = result.Models
                        .Where(m => m.InputModalities == null || !m.InputModalities.Contains("image"))
                        .ToList();
                    Console.WriteLine($"  every multimodal option declares image input: {undeclared.Count == 0}");
                }
            }

            // 2. A real completion through the implemented client.
            var response = await chat.CompleteAsync(new ChatCompletionRequest
            {
                Model = "orcarouter/auto",
                Messages = new List<ChatMessage>
                {
                    ChatMessage.User("Reply with exactly: orcarouter-ok")
                },
                MaxTokens = 32
            }).ConfigureAwait(false);

            Console.WriteLine($"inference model: {response.Model}");
            Console.WriteLine($"inference text:  {response.GetContent()}");
            Console.WriteLine($"inference usage: {response.Usage?.TotalTokens} tokens");

            return 0;
        }
    }
}
