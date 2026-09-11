using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zxw.Framework.AI.Credentials;

namespace Zxw.Framework.AI.Models
{
    /// <summary>
    /// The single place a model dropdown gets its contents. Every entry point calls this rather
    /// than a per-page copy of the filtering logic.
    /// <para>
    /// The rules it enforces:
    /// </para>
    /// <list type="bullet">
    /// <item>when live discovery succeeds it is authoritative and the verified seed is not merged in;</item>
    /// <item>when discovery fails the verified seed is offered, marked as degraded, never as free text;</item>
    /// <item>a previously selected model that the current capability filter no longer admits is
    /// reported as invalidated so the caller can clear it instead of silently keeping a wrong value.</item>
    /// </list>
    /// </summary>
    public sealed class OrcaModelSelector
    {
        private readonly OrcaModelCatalogClient _catalogClient;
        private readonly IOrcaCredentialStore _credentialStore;

        private readonly Dictionary<string, OrcaModelCatalog> _lastKnownGood =
            new Dictionary<string, OrcaModelCatalog>(StringComparer.Ordinal);

        private readonly object _gate = new object();

        public OrcaModelSelector(OrcaModelCatalogClient catalogClient, IOrcaCredentialStore credentialStore)
        {
            _catalogClient = catalogClient ?? throw new ArgumentNullException(nameof(catalogClient));
            _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
        }

        /// <summary>
        /// Options for one entry point. <paramref name="requiredInputModalities"/> is how a
        /// multimodal entry point asks for models that declare every modality it uploads; models
        /// with no declared modalities are excluded.
        /// </summary>
        public async Task<OrcaModelCatalog> GetOptionsAsync(
            OrcaModelCapability capability,
            IReadOnlyList<string> requiredInputModalities = null,
            CancellationToken cancellationToken = default)
        {
            var credential = _credentialStore?.Load();
            var catalog = await _catalogClient
                .DiscoverAsync(credential, capability, requiredInputModalities, cancellationToken)
                .ConfigureAwait(false);

            if (catalog.Status == OrcaCatalogStatus.Live)
            {
                Remember(capability, requiredInputModalities, catalog);
                return catalog;
            }

            // Live discovery failed. Prefer a previously fetched catalog over the seed, because it
            // describes the account's real access — but it is re-filtered for the capability being
            // requested now, so a catalog fetched for text chat cannot leak into a video dropdown.
            var cached = Recall(capability, requiredInputModalities);
            if (cached != null && cached.Models.Count > 0)
            {
                return new OrcaModelCatalog
                {
                    Capability = capability,
                    Status = OrcaCatalogStatus.LastKnownGood,
                    Error = catalog.Error,
                    Message = catalog.Message + " Showing the last catalog loaded from the API.",
                    Models = cached.Models
                };
            }

            return catalog;
        }

        /// <summary>
        /// Re-evaluate a previously selected model after the capability or the attachment set
        /// changed. Returns null when the selection is still valid, or a secret-free explanation
        /// when the caller must clear it and ask the user to choose again.
        /// </summary>
        public static string ValidateSelection(
            OrcaModelCatalog catalog,
            string selectedModelId,
            OrcaModelCapability capability,
            IReadOnlyList<string> requiredInputModalities = null)
        {
            if (string.IsNullOrWhiteSpace(selectedModelId))
                return null;

            if (catalog == null)
                return $"The model list is not loaded, so \"{selectedModelId}\" cannot be verified. Choose a model again.";

            if (catalog.ContainsModel(selectedModelId))
                return null;

            return $"\"{selectedModelId}\" is not available for {OrcaModelCapabilityMap.ToId(capability)}" +
                   (requiredInputModalities != null && requiredInputModalities.Count > 0
                       ? " with the current attachment types"
                       : string.Empty) +
                   " and was cleared. Choose one of the listed models.";
        }

        /// <summary>
        /// The same check against the catalog's own filter, for callers that hold model objects
        /// rather than an id.
        /// </summary>
        public static bool IsCompatible(
            OrcaModelInfo model,
            OrcaModelCapability capability,
            IReadOnlyList<string> requiredInputModalities = null) =>
            OrcaModelFilter.Select(capability, new[] { model }, requiredInputModalities).Count == 1;

        private void Remember(
            OrcaModelCapability capability,
            IReadOnlyList<string> modalities,
            OrcaModelCatalog catalog)
        {
            lock (_gate)
            {
                _lastKnownGood[CacheKey(capability, modalities)] = catalog;
            }
        }

        private OrcaModelCatalog Recall(OrcaModelCapability capability, IReadOnlyList<string> modalities)
        {
            lock (_gate)
            {
                return _lastKnownGood.TryGetValue(CacheKey(capability, modalities), out var catalog)
                    ? catalog
                    : null;
            }
        }

        private static string CacheKey(OrcaModelCapability capability, IReadOnlyList<string> modalities)
        {
            var suffix = modalities == null || modalities.Count == 0
                ? string.Empty
                : "|" + string.Join(",", modalities).ToLowerInvariant();

            return OrcaModelCapabilityMap.ToId(capability) + suffix;
        }
    }
}
