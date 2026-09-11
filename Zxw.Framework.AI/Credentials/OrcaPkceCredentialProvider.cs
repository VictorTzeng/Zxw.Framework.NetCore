using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// Adapter 2 of 2: OAuth 2.0 + PKCE, out-of-band code delivery (Flow B).
    /// <para>
    /// Flow B is the right fit for this library: <c>Zxw.Framework.AI</c> is hosted inside ASP.NET
    /// Core applications whose listen address is not known at library level and differs on every
    /// deployment, so there is no predictable loopback address to register, and a class library
    /// must not take over the host's socket lifetime. Out-of-band delivery needs neither.
    /// </para>
    /// <para>
    /// No client secret is involved and no redirect URI is pre-registered. S256 is mandatory in
    /// this flow and is the only challenge method sent.
    /// </para>
    /// </summary>
    public sealed class OrcaPkceCredentialProvider : IOrcaCredentialProvider
    {
        /// <summary>Provider id used by the settings UI for the browser/account choice.</summary>
        public const string ProviderId = "orcarouter-oauth";

        /// <summary>Display label used by the settings UI for the browser/account choice.</summary>
        public const string ProviderLabel = "OrcaRouter - Auth";

        /// <summary>Literal callback mode that asks for a displayed code rather than a redirect.</summary>
        public const string OutOfBandCallback = "oob";

        /// <summary>The scope this integration needs. A narrower grant is refused.</summary>
        public const string RequiredScope = "api";

        /// <summary>Scope grants that satisfy <see cref="RequiredScope"/>, widest last.</summary>
        private static readonly Dictionary<string, string[]> ScopeSatisfaction =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { "api", new[] { "api", "connector" } },
                { "connector", new[] { "connector" } }
            };

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly IOrcaCredentialStore _store;
        private readonly OrcaRouterOptions _options;
        private readonly string _authBaseUrl;

        public OrcaPkceCredentialProvider(
            HttpClient httpClient,
            IOrcaCredentialStore store,
            OrcaRouterOptions options = null,
            string authBaseUrl = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _options = options ?? new OrcaRouterOptions();
            _authBaseUrl = authBaseUrl;
        }

        public string Id => ProviderId;

        public OrcaCredentialSource Source => OrcaCredentialSource.Pkce;

        /// <summary>
        /// There is no non-interactive acquisition: the user has to approve in a browser. A stored
        /// key from a previous authorization is reused instead of starting a new one, because
        /// OrcaRouter caps PKCE-issued keys at ten per user per 24 hours.
        /// </summary>
        public OrcaCredentialResult Acquire()
        {
            var stored = _store.Load();
            if (stored != null && stored.HasKey && stored.Source == OrcaCredentialSource.Pkce)
                return OrcaCredentialResult.Ok(stored);

            return OrcaCredentialResult.Fail(
                OrcaCredentialState.Missing,
                OrcaCredentialSource.Pkce,
                "Not connected. Use Connect with OrcaRouter to authorize in a browser.");
        }

        public OrcaCredentialStatus GetStatus()
        {
            var stored = _store.Load();
            if (stored == null || !stored.HasKey || stored.Source != OrcaCredentialSource.Pkce)
            {
                return new OrcaCredentialStatus
                {
                    State = OrcaCredentialState.Missing,
                    Source = OrcaCredentialSource.Pkce,
                    Message = "Not connected with OrcaRouter."
                };
            }

            return _store.GetStatus();
        }

        public void Clear() => _store.Clear();

        /// <summary>
        /// Begin one authorization attempt. A fresh verifier and a fresh state are generated from
        /// the cryptographic RNG here, and the verifier never leaves this process until the code
        /// is exchanged.
        /// </summary>
        public OrcaPkceConnectSession Start(
            string appName = null,
            string scope = RequiredScope,
            string loginHint = null,
            string workspaceHint = null)
        {
            var verifier = OrcaPkce.CreateVerifier();
            var state = OrcaPkce.CreateState();
            var requestedScope = string.IsNullOrWhiteSpace(scope) ? RequiredScope : scope.Trim();

            var authorizeUrl = OrcaAuthEndpoints.BuildAuthorizeUrl(
                OrcaAuthEndpoints.ResolveAuthBaseUrl(_authBaseUrl, _options.SharedBaseUrl),
                OutOfBandCallback,
                OrcaPkce.CreateChallenge(verifier),
                state,
                string.IsNullOrWhiteSpace(appName) ? _options.AppTitle : appName,
                requestedScope,
                loginHint,
                workspaceHint);

            return new OrcaPkceConnectSession(this, verifier, state, authorizeUrl, requestedScope);
        }

        /// <summary>
        /// Exchange an authorization code for a durable API key and persist it. The returned key is
        /// an ordinary OrcaRouter API key, not a refreshable token: there is no refresh endpoint,
        /// and a revoked key requires reauthorization rather than a refresh attempt.
        /// </summary>
        internal async Task<OrcaCredentialResult> ExchangeAsync(
            string verifier,
            string state,
            string returnedState,
            string code,
            string requestedScope,
            CancellationToken cancellationToken)
        {
            // Flow B has no callback, but a caller may still surface a state value (the consent
            // screen echoes it when it is handed one). Compare it when present, so a mismatched
            // state can never be ignored.
            if (!string.IsNullOrEmpty(returnedState) && !OrcaPkce.FixedTimeEquals(state, returnedState))
            {
                return Fail(OrcaCredentialState.Missing,
                    "Authorization state did not match the value this session sent. The code was not used.");
            }

            if (string.IsNullOrWhiteSpace(code))
                return Fail(OrcaCredentialState.Missing, "No authorization code was provided.");

            var exchangeUri = OrcaAuthEndpoints.BuildExchangeUri(
                OrcaAuthEndpoints.ResolveAuthBaseUrl(_authBaseUrl, _options.SharedBaseUrl));

            var payload = new ExchangeRequest
            {
                Code = code.Trim(),
                CodeVerifier = verifier,
                CodeChallengeMethod = OrcaPkce.ChallengeMethod
            };

            HttpResponseMessage response;
            string body;
            try
            {
                using (var content = new StringContent(
                    JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json"))
                using (var request = new HttpRequestMessage(HttpMethod.Post, exchangeUri) { Content = content })
                {
                    response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                }

                body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Fail(OrcaCredentialState.Missing, "Authorization was cancelled.");
            }
            catch (HttpRequestException)
            {
                return Fail(
                    OrcaCredentialState.Missing,
                    $"Could not reach the OrcaRouter authorization service at {exchangeUri.GetLeftPart(UriPartial.Authority)}. " +
                    "Check the network and try again.");
            }
            catch (TaskCanceledException)
            {
                // A timeout, not an explicit cancel: the request may or may not have been seen.
                return Fail(OrcaCredentialState.Missing,
                    "The OrcaRouter authorization service did not respond in time. Start a new authorization.");
            }

            // The response body can echo request fields, so it is scrubbed against the verifier and
            // the code before any part of it is used in a message.
            var safeBody = OrcaSecretRedaction.Scrub(Truncate(body, 400), verifier, code);

            if (!response.IsSuccessStatusCode)
                return Fail(OrcaCredentialState.Missing, DescribeExchangeFailure(response.StatusCode, safeBody));

            ExchangeResponse parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<ExchangeResponse>(body, JsonOptions);
            }
            catch (JsonException)
            {
                return Fail(OrcaCredentialState.Missing,
                    "The OrcaRouter authorization service returned a response that could not be read.");
            }

            if (parsed == null || string.IsNullOrWhiteSpace(parsed.Key))
                return Fail(OrcaCredentialState.Missing,
                    "The OrcaRouter authorization service did not return a key.");

            // Read back what was actually granted, not what was asked for.
            var grantedScope = parsed.Scope;
            if (!ScopeSatisfies(requestedScope, grantedScope))
            {
                // The key is deliberately not persisted: using it would silently operate with fewer
                // permissions than this integration requires.
                return Fail(
                    OrcaCredentialState.Missing,
                    $"Authorization granted scope \"{grantedScope ?? "none"}\", which does not cover the " +
                    $"requested \"{requestedScope}\" scope. Nothing was stored; ask a workspace owner to " +
                    "grant the required access and connect again.");
            }

            var credential = new OrcaCredential(
                parsed.Key,
                OrcaCredentialSource.Pkce,
                parsed.UserId,
                grantedScope);

            _store.Save(credential);

            return OrcaCredentialResult.Ok(
                credential,
                grantedScope != requestedScope
                    ? $"Connected with OrcaRouter. Grant received: \"{grantedScope}\" (requested \"{requestedScope}\")."
                    : "Connected with OrcaRouter. The key is reused until it is revoked.");
        }

        /// <summary>True when <paramref name="granted"/> covers <paramref name="requested"/>.</summary>
        public static bool ScopeSatisfies(string requested, string granted)
        {
            var wanted = string.IsNullOrWhiteSpace(requested) ? RequiredScope : requested.Trim();
            if (string.IsNullOrWhiteSpace(granted))
                return false;

            if (!ScopeSatisfaction.TryGetValue(wanted, out var accepted))
                return string.Equals(wanted, granted.Trim(), StringComparison.Ordinal);

            foreach (var candidate in accepted)
            {
                if (string.Equals(candidate, granted.Trim(), StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static string DescribeExchangeFailure(HttpStatusCode statusCode, string safeBody)
        {
            switch ((int)statusCode)
            {
                case 400:
                    return "OrcaRouter rejected the request (400): the challenge method did not match the one " +
                           "sent when authorization started. Start a new authorization.";
                case 403:
                    return "OrcaRouter rejected the code (403): it is unknown, expired, already used, or the " +
                           "verifier does not match. Authorization codes are single-use with a 10 minute lifetime. " +
                           "Start a new authorization.";
                case 429:
                    return "OrcaRouter refused a new key (429): too many keys have been issued for this account " +
                           "in the last 24 hours (limit 10). Reuse or revoke an existing key in the OrcaRouter " +
                           "console, then try again.";
                default:
                    return $"OrcaRouter authorization failed with HTTP {(int)statusCode}. {safeBody}".Trim();
            }
        }

        private static OrcaCredentialResult Fail(OrcaCredentialState state, string message) =>
            OrcaCredentialResult.Fail(state, OrcaCredentialSource.Pkce, message);

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
                return value;
            return value.Substring(0, max) + "...";
        }

        private sealed class ExchangeRequest
        {
            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("code_verifier")]
            public string CodeVerifier { get; set; }

            [JsonPropertyName("code_challenge_method")]
            public string CodeChallengeMethod { get; set; }
        }

        private sealed class ExchangeResponse
        {
            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("user_id")]
            public string UserId { get; set; }

            [JsonPropertyName("scope")]
            public string Scope { get; set; }
        }
    }

    /// <summary>
    /// One authorization attempt. Holds the verifier privately; it is never exposed, logged, or
    /// placed in a URL. Call <see cref="CompleteAsync"/> with the code the consent screen showed.
    /// </summary>
    public sealed class OrcaPkceConnectSession
    {
        private readonly OrcaPkceCredentialProvider _provider;
        private readonly string _verifier;
        private readonly string _state;
        private readonly string _requestedScope;

        internal OrcaPkceConnectSession(
            OrcaPkceCredentialProvider provider,
            string verifier,
            string state,
            string authorizeUrl,
            string requestedScope)
        {
            _provider = provider;
            _verifier = verifier;
            _state = state;
            _requestedScope = requestedScope;
            AuthorizeUrl = authorizeUrl;
        }

        /// <summary>Open this in a browser. Contains only the challenge, never the verifier.</summary>
        public string AuthorizeUrl { get; }

        /// <summary>Identifies this attempt, for generation-guarding asynchronous results.</summary>
        public string AttemptId { get; } = Guid.NewGuid().ToString("N");

        /// <summary>Human-facing instructions for the out-of-band flow.</summary>
        public string Instructions =>
            "Open the authorization URL in a browser, approve access, then enter the code shown on screen.";

        /// <summary>
        /// Redeem the displayed code. <paramref name="returnedState"/> is optional and only used by
        /// callers that receive an echoed state value.
        /// </summary>
        public Task<OrcaCredentialResult> CompleteAsync(
            string code,
            string returnedState = null,
            CancellationToken cancellationToken = default) =>
            _provider.ExchangeAsync(_verifier, _state, returnedState, code, _requestedScope, cancellationToken);
    }
}
