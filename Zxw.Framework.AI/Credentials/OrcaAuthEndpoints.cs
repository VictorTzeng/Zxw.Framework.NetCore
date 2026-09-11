using System;
using System.Collections.Generic;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// Raised when the OrcaRouter origins/paths are configured in a way that cannot be safe.
    /// Messages never contain credential material.
    /// </summary>
    public class OrcaConfigurationException : InvalidOperationException
    {
        public OrcaConfigurationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Resolves and validates the two distinct OrcaRouter public origins.
    /// <para>
    /// Authentication lives on <c>https://www.orcarouter.ai</c> (authorize <c>/auth</c>,
    /// exchange <c>/api/v1/auth/keys</c>). Inference and model discovery live on
    /// <c>https://api.orcarouter.ai/v1</c>. The relay is on <c>/v1</c>, the auth endpoints are not,
    /// so one origin is never derived from the other.
    /// </para>
    /// </summary>
    public static class OrcaAuthEndpoints
    {
        public const string DefaultAuthBaseUrl = "https://www.orcarouter.ai";
        public const string DefaultApiBaseUrl = "https://api.orcarouter.ai/v1";

        /// <summary>Absolute path of the consent screen, relative to the auth origin.</summary>
        public const string AuthorizePath = "/auth";

        /// <summary>Absolute path of the code exchange, relative to the auth origin.</summary>
        public const string ExchangePath = "/api/v1/auth/keys";

        /// <summary>Shared self-hosted fallback origin, used for both roles when set.</summary>
        public const string SharedBaseUrlEnvironmentVariable = "ORCA_BASE_URL";

        /// <summary>Explicit auth origin override (wins over the shared fallback).</summary>
        public const string AuthBaseUrlEnvironmentVariable = "ORCA_AUTH_BASE_URL";

        /// <summary>Explicit inference/catalog origin override (wins over the shared fallback).</summary>
        public const string ApiBaseUrlEnvironmentVariable = "ORCA_API_BASE_URL";

        /// <summary>
        /// The relay route that does not exist. <c>POST https://api.orcarouter.ai/v1/auth/keys</c>
        /// is the single most common integration mistake; it is rejected loudly rather than
        /// silently producing a 404 at runtime.
        /// </summary>
        public const string WrongRelayAuthPath = "/v1/auth/keys";

        /// <summary>
        /// Resolve the authentication origin. Precedence: explicit override, then the shared
        /// self-hosted fallback, then the public default. Never derived from the API origin.
        /// </summary>
        public static string ResolveAuthBaseUrl(string explicitAuthBaseUrl = null, string sharedBaseUrl = null)
        {
            var value = FirstNonEmpty(
                explicitAuthBaseUrl,
                sharedBaseUrl,
                Environment.GetEnvironmentVariable(AuthBaseUrlEnvironmentVariable),
                Environment.GetEnvironmentVariable(SharedBaseUrlEnvironmentVariable));
            return NormalizeOrigin(value ?? DefaultAuthBaseUrl, "auth");
        }

        /// <summary>
        /// Resolve the inference + model-catalog origin. Precedence: explicit override, then the
        /// shared self-hosted fallback (with the relay <c>/v1</c> segment appended, because a
        /// self-hosted deployment configures one host and the relay always lives under <c>/v1</c>),
        /// then the public default. Never derived from the auth origin.
        /// </summary>
        public static string ResolveApiBaseUrl(string explicitApiBaseUrl = null, string sharedBaseUrl = null)
        {
            var explicitValue = FirstNonEmpty(
                explicitApiBaseUrl,
                Environment.GetEnvironmentVariable(ApiBaseUrlEnvironmentVariable));
            if (explicitValue != null)
                return NormalizeApiBase(explicitValue);

            var shared = FirstNonEmpty(
                sharedBaseUrl,
                Environment.GetEnvironmentVariable(SharedBaseUrlEnvironmentVariable));
            if (shared != null)
            {
                var normalized = NormalizeOrigin(shared, "shared");
                return normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)
                    ? normalized
                    : normalized + "/v1";
            }

            return DefaultApiBaseUrl;
        }

        /// <summary>Build the consent-screen URL for the given authorize parameters.</summary>
        public static string BuildAuthorizeUrl(
            string authBaseUrl,
            string callbackUrl,
            string codeChallenge,
            string state,
            string appName,
            string scope,
            string loginHint = null,
            string workspaceHint = null,
            string prompt = null)
        {
            var origin = NormalizeOrigin(authBaseUrl, "auth");
            var query = new List<string>
            {
                "callback_url=" + Uri.EscapeDataString(callbackUrl ?? string.Empty),
                "code_challenge=" + Uri.EscapeDataString(codeChallenge ?? string.Empty),
                // S256 is sent unconditionally: on Flow A the user may still choose
                // "Show me a code" on the consent screen, so a human can always end up
                // holding the code. There is no authorize parameter to prevent that.
                "code_challenge_method=S256",
                "state=" + Uri.EscapeDataString(state ?? string.Empty)
            };

            if (!string.IsNullOrWhiteSpace(appName))
                query.Add("app_name=" + Uri.EscapeDataString(appName));
            if (!string.IsNullOrWhiteSpace(scope))
                query.Add("scope=" + Uri.EscapeDataString(scope));
            if (!string.IsNullOrWhiteSpace(loginHint))
                query.Add("login_hint=" + Uri.EscapeDataString(loginHint));
            if (!string.IsNullOrWhiteSpace(workspaceHint))
                query.Add("workspace_hint=" + Uri.EscapeDataString(workspaceHint));
            if (!string.IsNullOrWhiteSpace(prompt))
                query.Add("prompt=" + Uri.EscapeDataString(prompt));

            return origin + AuthorizePath + "?" + string.Join("&", query);
        }

        /// <summary>Build the code-exchange URL. Always on the auth origin, never on the relay.</summary>
        public static Uri BuildExchangeUri(string authBaseUrl)
        {
            var origin = NormalizeOrigin(authBaseUrl, "auth");
            return new Uri(origin + ExchangePath, UriKind.Absolute);
        }

        /// <summary>Build the model-catalog URL on the inference origin.</summary>
        public static Uri BuildModelsUri(string apiBaseUrl, string capability = null)
        {
            var normalized = NormalizeApiBase(apiBaseUrl);
            var url = normalized + "/models";
            if (!string.IsNullOrWhiteSpace(capability))
                url += "?capability=" + Uri.EscapeDataString(capability);
            return new Uri(url, UriKind.Absolute);
        }

        /// <summary>
        /// Reject anything that is not an absolute https origin, or loopback http. Explicit
        /// overrides are trusted to be what the operator meant, but never trusted to be insecure.
        /// </summary>
        public static string NormalizeOrigin(string value, string role)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new OrcaConfigurationException($"OrcaRouter {role} base URL is empty.");

            if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
                throw new OrcaConfigurationException($"OrcaRouter {role} base URL is not an absolute URL.");

            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                throw new OrcaConfigurationException($"OrcaRouter {role} base URL must use http or https.");

            if (uri.Scheme == Uri.UriSchemeHttp && !IsLoopbackHost(uri.Host))
            {
                throw new OrcaConfigurationException(
                    $"OrcaRouter {role} base URL must use https for a remote host. " +
                    "Plain http is only allowed for loopback development.");
            }

            if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                throw new OrcaConfigurationException($"OrcaRouter {role} base URL must not carry a query or fragment.");

            if (!string.IsNullOrEmpty(uri.UserInfo))
                throw new OrcaConfigurationException($"OrcaRouter {role} base URL must not carry userinfo.");

            var normalized = uri.GetLeftPart(UriPartial.Authority) + uri.AbsolutePath.TrimEnd('/');

            // Guard the classic mistake in both directions: the auth origin must not itself be the
            // relay's /v1 route, and the relay /v1 route must never be used as an auth origin.
            if (normalized.EndsWith(WrongRelayAuthPath, StringComparison.OrdinalIgnoreCase) ||
                normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase) && role == "auth")
            {
                throw new OrcaConfigurationException(
                    $"OrcaRouter {role} base URL looks like the inference relay. " +
                    $"Authentication is on {DefaultAuthBaseUrl}; the relay is on {DefaultApiBaseUrl} " +
                    $"and {WrongRelayAuthPath} does not exist.");
            }

            return normalized;
        }

        private static string NormalizeApiBase(string value)
        {
            var normalized = NormalizeOrigin(value, "api");
            if (normalized.EndsWith(WrongRelayAuthPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new OrcaConfigurationException(
                    $"OrcaRouter api base URL must not point at {WrongRelayAuthPath}. " +
                    $"The code exchange lives on the auth origin at {DefaultAuthBaseUrl}{ExchangePath}.");
            }

            return normalized;
        }

        public static bool IsLoopbackHost(string host)
        {
            if (string.IsNullOrEmpty(host))
                return false;
            return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
                || string.Equals(host, "127.0.0.1", StringComparison.Ordinal)
                || string.Equals(host, "[::1]", StringComparison.Ordinal)
                || string.Equals(host, "::1", StringComparison.Ordinal);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return null;
        }
    }
}
