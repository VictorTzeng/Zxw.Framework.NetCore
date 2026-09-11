using System;
using System.Collections.Generic;
using Zxw.Framework.AI.Models;
namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// Applies the per-entry-point capability rules to a raw catalog page. Every rule is derived
    /// from catalog metadata only — a model is never admitted because its name looks right, and a
    /// model is never admitted when the metadata it would need is absent.
    /// </summary>
    public static class OrcaModelFilter
    {
        /// <summary>Text-chat admission: a text endpoint family, and no non-text-only endpoint family.</summary>
        public static bool IsTextChat(OrcaModelInfo model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id))
                return false;

            var endpoints = model.SupportedEndpointTypes;
            if (endpoints == null || endpoints.Count == 0)
                return false;

            var hasText = false;
            foreach (var endpoint in endpoints)
            {
                if (Contains(OrcaModelCapabilityMap.TextEndpointTypes, endpoint))
                    hasText = true;
                // A model that advertises an image/video/rerank/embedding endpoint is specialised
                // for it. The live catalog still returns some of them under ?capability=chat.
                if (Contains(OrcaModelCapabilityMap.NonTextEndpointTypes, endpoint))
                    return false;
            }

            return hasText;
        }

        /// <summary>
        /// Multimodal understanding admission: text chat first, then an explicit declaration of
        /// every non-text modality the entry point actually uploads. A model with no
        /// <c>architecture.input_modalities</c> at all fails closed.
        /// </summary>
        public static bool IsMultimodal(OrcaModelInfo model, IReadOnlyList<string> requiredInputModalities)
        {
            if (!IsTextChat(model))
                return false;

            if (requiredInputModalities == null || requiredInputModalities.Count == 0)
                return true;

            var declared = model.InputModalities;
            if (declared == null || declared.Count == 0)
                return false;

            foreach (var required in requiredInputModalities)
            {
                if (string.IsNullOrWhiteSpace(required))
                    continue;
                if (string.Equals(required, "text", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!Contains(declared, required))
                    return false;
            }

            return true;
        }

        /// <summary>Embedding admission: the <c>embeddings</c> endpoint family, strictly.</summary>
        public static bool IsEmbedding(OrcaModelInfo model) =>
            HasEndpoint(model, OrcaModelCapabilityMap.ToEndpointType(OrcaModelCapability.Embedding));

        /// <summary>Image generation admission: the <c>image-generation</c> endpoint family, strictly.</summary>
        public static bool IsImageGeneration(OrcaModelInfo model) =>
            HasEndpoint(model, OrcaModelCapabilityMap.ToEndpointType(OrcaModelCapability.ImageGeneration));

        /// <summary>Video generation admission: the <c>openai-video</c> endpoint family, strictly.</summary>
        public static bool IsVideoGeneration(OrcaModelInfo model) =>
            HasEndpoint(model, OrcaModelCapabilityMap.ToEndpointType(OrcaModelCapability.VideoGeneration));

        /// <summary>Rerank admission: the <c>jina-rerank</c> endpoint family, strictly.</summary>
        public static bool IsRerank(OrcaModelInfo model) =>
            HasEndpoint(model, OrcaModelCapabilityMap.ToEndpointType(OrcaModelCapability.Rerank));

        /// <summary>
        /// The one entry point the rest of the project calls. Returns the subset of
        /// <paramref name="models"/> a given selector may offer.
        /// </summary>
        public static IList<OrcaModelInfo> Select(
            OrcaModelCapability capability,
            IEnumerable<OrcaModelInfo> models,
            IReadOnlyList<string> requiredInputModalities = null)
        {
            var result = new List<OrcaModelInfo>();
            if (models == null)
                return result;

            foreach (var model in models)
            {
                bool admitted;
                switch (capability)
                {
                    case OrcaModelCapability.Chat:
                        admitted = IsTextChat(model);
                        break;
                    case OrcaModelCapability.Multimodal:
                        admitted = IsMultimodal(model, requiredInputModalities);
                        break;
                    case OrcaModelCapability.Embedding:
                        admitted = IsEmbedding(model);
                        break;
                    case OrcaModelCapability.ImageGeneration:
                        admitted = IsImageGeneration(model);
                        break;
                    case OrcaModelCapability.VideoGeneration:
                        admitted = IsVideoGeneration(model);
                        break;
                    case OrcaModelCapability.Rerank:
                        admitted = IsRerank(model);
                        break;
                    default:
                        admitted = false;
                        break;
                }

                if (admitted)
                    result.Add(model);
            }

            return result;
        }

        private static bool HasEndpoint(OrcaModelInfo model, string endpoint)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id) || string.IsNullOrWhiteSpace(endpoint))
                return false;
            return Contains(model.SupportedEndpointTypes, endpoint);
        }

        private static bool Contains(IEnumerable<string> values, string value)
        {
            if (values == null)
                return false;

            foreach (var candidate in values)
            {
                if (string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
