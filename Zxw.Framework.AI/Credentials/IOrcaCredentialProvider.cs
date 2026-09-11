using System;
using System.Collections.Generic;
using Zxw.Framework.AI.Models;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// The credential acquisition seam. Both a hand-entered API key and an OAuth 2.0 + PKCE
    /// authorization implement this and produce the same <see cref="OrcaCredential"/>.
    /// <para>
    /// Consumers (chat client, model catalog, embedding selector) depend on this interface only,
    /// so none of them carries its own copy of the authentication logic and none of them can tell
    /// which adapter produced the key.
    /// </para>
    /// </summary>
    public interface IOrcaCredentialProvider
    {
        /// <summary>Stable identifier, e.g. <c>orcarouter</c> or <c>orcarouter-oauth</c>.</summary>
        string Id { get; }

        /// <summary>Source tag written into the credential this provider yields.</summary>
        OrcaCredentialSource Source { get; }

        /// <summary>
        /// Produce a usable credential, or a status explaining why none is available. Must never
        /// throw for an ordinary "no credential configured" condition.
        /// </summary>
        OrcaCredentialResult Acquire();

        /// <summary>Secret-free status for the settings UI.</summary>
        OrcaCredentialStatus GetStatus();

        /// <summary>Forget the stored credential. Never touches a value supplied by the environment.</summary>
        void Clear();
    }

    /// <summary>Outcome of an acquisition attempt.</summary>
    public sealed class OrcaCredentialResult
    {
        private OrcaCredentialResult(OrcaCredential credential, OrcaCredentialStatus status, bool succeeded)
        {
            Credential = credential;
            Status = status;
            Succeeded = succeeded;
        }

        public bool Succeeded { get; }

        /// <summary>The credential, or null when acquisition failed.</summary>
        public OrcaCredential Credential { get; }

        /// <summary>Always populated, secret-free.</summary>
        public OrcaCredentialStatus Status { get; }

        public static OrcaCredentialResult Ok(OrcaCredential credential, string message = null) =>
            new OrcaCredentialResult(
                credential,
                new OrcaCredentialStatus
                {
                    State = OrcaCredentialState.Active,
                    Source = credential.Source,
                    MaskedKey = OrcaSecretRedaction.Mask(credential.ApiKey),
                    AccountId = credential.AccountId,
                    Scope = credential.Scope,
                    Message = message
                },
                true);

        public static OrcaCredentialResult Fail(OrcaCredentialState state, OrcaCredentialSource source, string message) =>
            new OrcaCredentialResult(
                null,
                new OrcaCredentialStatus { State = state, Source = source, Message = message },
                false);
    }
}
