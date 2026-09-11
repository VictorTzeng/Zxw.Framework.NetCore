using System;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// Adapter 1 of 2: a hand-entered or pre-configured <c>sk-orca-…</c> key.
    /// <para>
    /// This keeps the project's existing secret habits — <c>OrcaRouterOptions.ApiKey</c> in host
    /// configuration, or the <c>ORCAROUTER_API_KEY</c> environment variable — and adds update and
    /// clear on top. It never starts an authorization flow, so users with no browser, no account
    /// session, or an existing key are unaffected.
    /// </para>
    /// </summary>
    public sealed class OrcaApiKeyCredentialProvider : IOrcaCredentialProvider
    {
        /// <summary>Provider id used by the settings UI for the paste-a-key choice.</summary>
        public const string ProviderId = "orcarouter";

        /// <summary>Display label used by the settings UI for the paste-a-key choice.</summary>
        public const string ProviderLabel = "OrcaRouter - API";

        private readonly OrcaRouterOptions _options;
        private readonly IOrcaCredentialStore _store;
        private readonly string _sessionKey;

        public OrcaApiKeyCredentialProvider(OrcaRouterOptions options, IOrcaCredentialStore store)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Id => ProviderId;

        public OrcaCredentialSource Source => OrcaCredentialSource.ApiKey;

        /// <summary>
        /// Resolve the credential from the host's configuration/environment layers, or from the
        /// last key the user applied in this session.
        /// </summary>
        public OrcaCredentialResult Acquire()
        {
            if (!string.IsNullOrWhiteSpace(_sessionKey))
                return OrcaCredentialResult.Ok(new OrcaCredential(_sessionKey, OrcaCredentialSource.ApiKey));

            var credential = _store.Load();
            if (credential == null || !credential.HasKey)
            {
                return OrcaCredentialResult.Fail(
                    OrcaCredentialState.Missing,
                    OrcaCredentialSource.ApiKey,
                    "No OrcaRouter API key is configured. Paste an sk-orca-… key, or use Connect with OrcaRouter.");
            }

            return OrcaCredentialResult.Ok(credential);
        }

        public OrcaCredentialStatus GetStatus()
        {
            if (!string.IsNullOrWhiteSpace(_sessionKey))
            {
                return new OrcaCredentialStatus
                {
                    State = OrcaCredentialState.Active,
                    Source = OrcaCredentialSource.ApiKey,
                    MaskedKey = OrcaSecretRedaction.Mask(_sessionKey),
                    Message = "Using the API key entered in this session."
                };
            }

            return _store.GetStatus();
        }

        /// <summary>
        /// Apply a key the user typed. Only a structural check is performed — the <c>sk-orca-</c>
        /// prefix is not proof of validity, and the first real request establishes that.
        /// </summary>
        public OrcaCredentialStatus ApplyKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return Fail("Enter an OrcaRouter API key before saving.");

            var trimmed = apiKey.Trim();
            if (!OrcaSecretRedaction.LooksLikeOrcaKey(trimmed))
                return Fail("That does not look like an OrcaRouter API key; it should start with sk-orca-.");

            var credential = new OrcaCredential(trimmed, OrcaCredentialSource.ApiKey);
            _store.Save(credential);

            return new OrcaCredentialStatus
            {
                State = OrcaCredentialState.Active,
                Source = OrcaCredentialSource.ApiKey,
                MaskedKey = OrcaSecretRedaction.Mask(trimmed),
                Message = "API key saved. Validity is confirmed by the first request."
            };
        }

        public void Clear()
        {
            _store.Clear();
        }

        private static OrcaCredentialStatus Fail(string message) => new OrcaCredentialStatus
        {
            State = OrcaCredentialState.Missing,
            Source = OrcaCredentialSource.ApiKey,
            Message = message
        };
    }
}
