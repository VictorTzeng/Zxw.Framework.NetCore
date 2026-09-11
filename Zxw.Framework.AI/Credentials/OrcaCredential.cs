using System;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// Where a credential came from. Downstream consumers (chat client, model catalog, embedding
    /// selector) must never branch on this value — it exists for diagnostics and the settings UI.
    /// </summary>
    public enum OrcaCredentialSource
    {
        None = 0,

        /// <summary>Hand-entered or configured <c>sk-orca-…</c> key.</summary>
        ApiKey = 1,

        /// <summary>Key obtained through an OAuth 2.0 + PKCE authorization.</summary>
        Pkce = 2
    }

    /// <summary>
    /// Lifecycle state of a stored credential.
    /// </summary>
    public enum OrcaCredentialState
    {
        /// <summary>No credential is configured.</summary>
        Missing = 0,

        /// <summary>A credential is present and has not been rejected by the relay.</summary>
        Active = 1,

        /// <summary>
        /// The relay answered <c>401</c> for this exact credential generation. Terminal: the user
        /// must re-authorize. There is no refresh grant to attempt.
        /// </summary>
        NeedsReauth = 2
    }

    /// <summary>
    /// The single shape both authentication adapters produce. A pasted key and a PKCE-issued key
    /// are the same thing downstream: a normal OrcaRouter API key.
    /// </summary>
    public sealed class OrcaCredential
    {
        public OrcaCredential(
            string apiKey,
            OrcaCredentialSource source,
            string accountId = null,
            string scope = null,
            string generation = null)
        {
            ApiKey = apiKey;
            Source = source;
            AccountId = accountId;
            Scope = scope;
            Generation = generation ?? Guid.NewGuid().ToString("N");
        }

        /// <summary>The secret. Never log, print, serialize to a UI payload, or include in an error.</summary>
        public string ApiKey { get; }

        public OrcaCredentialSource Source { get; }

        /// <summary>OrcaRouter user id, when the exchange reported one.</summary>
        public string AccountId { get; }

        /// <summary>The scope actually <em>granted</em> by the exchange, not the one requested.</summary>
        public string Scope { get; }

        /// <summary>
        /// Monotonic identity of this credential value. A late failure raised by a request that
        /// carried an older generation must not mark a newer credential as broken.
        /// </summary>
        public string Generation { get; }

        public bool HasKey => !string.IsNullOrWhiteSpace(ApiKey);
    }

    /// <summary>Serializable, secret-free status for settings screens.</summary>
    public sealed class OrcaCredentialStatus
    {
        public OrcaCredentialState State { get; set; }

        public OrcaCredentialSource Source { get; set; }

        /// <summary>Masked display form, e.g. <c>sk-orca-…Ab12</c>. Never the full key.</summary>
        public string MaskedKey { get; set; }

        public string AccountId { get; set; }

        public string Scope { get; set; }

        /// <summary>Human-readable, actionable, secret-free explanation.</summary>
        public string Message { get; set; }

        public bool CanClear => State != OrcaCredentialState.Missing;
    }
}
