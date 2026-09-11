using System.Collections.Generic;
using Zxw.Framework.AI.Credentials;

namespace Zxw.Framework.AI.Models
{
    /// <summary>
    /// One entry of an OrcaRouter model dropdown. Built from the live catalog when discovery
    /// succeeds, or from the verified cold-start seed when it does not.
    /// </summary>
    public sealed class OrcaModelInfo
    {
        /// <summary>
        /// The catalog id, preserved verbatim including the <c>vendor/model</c> namespace.
        /// Never rewritten or normalized.
        /// </summary>
        public string Id { get; set; }

        /// <summary>Human-readable label, falling back to <see cref="Id"/> when the catalog omits one.</summary>
        public string DisplayName { get; set; }

        public int? ContextLength { get; set; }

        public int? MaxCompletionTokens { get; set; }

        /// <summary>Catalog <c>supported_endpoint_types</c>, verbatim.</summary>
        public IList<string> SupportedEndpointTypes { get; set; } = new List<string>();

        /// <summary>Catalog <c>architecture.input_modalities</c>, verbatim. Null means "not declared".</summary>
        public IList<string> InputModalities { get; set; }

        /// <summary>Catalog <c>architecture.output_modalities</c>, verbatim.</summary>
        public IList<string> OutputModalities { get; set; }

        /// <summary>
        /// True when this entry came from the verified cold-start seed rather than live discovery.
        /// The UI labels these so an outage never looks like a complete catalog.
        /// </summary>
        public bool FromSeed { get; set; }

        /// <summary>
        /// Verified reasoning-effort ladder, when the project knows one. Only populated for models
        /// whose metadata is actually verified; never guessed from a model name.
        /// </summary>
        public IList<string> ReasoningEfforts { get; set; }
    }

    /// <summary>Where a dropdown's contents came from, so the UI can show degraded/refresh state.</summary>
    public enum OrcaCatalogStatus
    {
        /// <summary>A live catalog request succeeded. Its result is authoritative; no seed is merged in.</summary>
        Live = 0,

        /// <summary>Live discovery failed and the verified cold-start seed is being shown instead.</summary>
        SeedFallback = 1,

        /// <summary>Live discovery failed and a previously fetched catalog is being reused.</summary>
        LastKnownGood = 2,

        /// <summary>Discovery failed and nothing verified is available for this capability.</summary>
        Empty = 3
    }

    /// <summary>Non-secret catalog error classification for UI messaging.</summary>
    public enum OrcaCatalogError
    {
        None = 0,

        /// <summary>No usable credential. The user must configure a key or connect an account.</summary>
        AuthRequired = 1,

        /// <summary>The credential was rejected. Discovery cannot proceed until the user re-authenticates.</summary>
        AuthRejected = 2,

        /// <summary>Rate limited. Retry later; the previous catalog stays usable.</summary>
        RateLimited = 3,

        /// <summary>Transport failure, timeout, or malformed response.</summary>
        Network = 4
    }

    /// <summary>
    /// The result of one capability discovery call. This is what feeds a model selector; the
    /// selector never receives a free-text field as a fallback.
    /// </summary>
    public sealed class OrcaModelCatalog
    {
        public OrcaModelCapability Capability { get; set; }

        public IList<OrcaModelInfo> Models { get; set; } = new List<OrcaModelInfo>();

        public OrcaCatalogStatus Status { get; set; }

        public OrcaCatalogError Error { get; set; }

        /// <summary>Secret-free, user-actionable explanation when <see cref="Error"/> is set.</summary>
        public string Message { get; set; }

        /// <summary>True when the user should be told to refresh rather than treat this as complete.</summary>
        public bool IsDegraded =>
            Status == OrcaCatalogStatus.SeedFallback ||
            Status == OrcaCatalogStatus.LastKnownGood ||
            Status == OrcaCatalogStatus.Empty;

        /// <summary>True when this list came from the API rather than from the seed.</summary>
        public bool IsAuthoritative =>
            Status == OrcaCatalogStatus.Live || Status == OrcaCatalogStatus.LastKnownGood;

        public bool ContainsModel(string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId) || Models == null)
                return false;

            foreach (var model in Models)
            {
                if (string.Equals(model.Id, modelId, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
