using System;
using System.Collections.Generic;
using System.Globalization;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// A model-level capability. The value maps onto the OrcaRouter catalog's
    /// <c>supported_endpoint_types</c> entries and its <c>?capability=</c> query parameter.
    /// </summary>
    public enum OrcaModelCapability
    {
        /// <summary>Text chat / agent completion. Endpoint family: openai, anthropic, gemini, openai-response.</summary>
        Chat = 0,

        /// <summary>Multimodal understanding: chat plus a declared non-text input modality.</summary>
        Multimodal = 1,

        /// <summary>Text embeddings. Endpoint: embeddings.</summary>
        Embedding = 2,

        /// <summary>Image generation. Endpoint: image-generation.</summary>
        ImageGeneration = 3,

        /// <summary>Video generation. Endpoint: openai-video. Strict.</summary>
        VideoGeneration = 4,

        /// <summary>Rerank. Endpoint: jina-rerank. Strict.</summary>
        Rerank = 5
    }

    /// <summary>Maps capabilities onto the catalog query and the endpoint families they accept.</summary>
    public static class OrcaModelCapabilityMap
    {
        /// <summary>Endpoint types that can serve text chat over the OpenAI wire format.</summary>
        public static readonly IReadOnlyList<string> TextEndpointTypes =
            new[] { "openai", "anthropic", "gemini", "openai-response" };

        /// <summary>
        /// Endpoint types that are definitionally not text chat. A model advertising any of these
        /// is excluded from the text dropdown even if it also answers <c>?capability=chat</c>,
        /// which the live catalog does for a handful of records.
        /// </summary>
        public static readonly IReadOnlyList<string> NonTextEndpointTypes =
            new[] { "image-generation", "openai-video", "jina-rerank", "embeddings" };

        /// <summary>The <c>?capability=</c> value to send for a capability, or null for none.</summary>
        public static string ToQueryValue(OrcaModelCapability capability)
        {
            switch (capability)
            {
                case OrcaModelCapability.Chat:
                case OrcaModelCapability.Multimodal:
                    return "chat";
                case OrcaModelCapability.Embedding:
                    return "embedding";
                case OrcaModelCapability.ImageGeneration:
                    return "image";
                case OrcaModelCapability.VideoGeneration:
                    return "video";
                case OrcaModelCapability.Rerank:
                    return "rerank";
                default:
                    return null;
            }
        }

        public static string ToEndpointType(OrcaModelCapability capability)
        {
            switch (capability)
            {
                case OrcaModelCapability.Embedding:
                    return "embeddings";
                case OrcaModelCapability.ImageGeneration:
                    return "image-generation";
                case OrcaModelCapability.VideoGeneration:
                    return "openai-video";
                case OrcaModelCapability.Rerank:
                    return "jina-rerank";
                default:
                    return null;
            }
        }

        /// <summary>Stable identifier used by selector options and tests.</summary>
        public static string ToId(OrcaModelCapability capability) =>
            capability.ToString().ToLowerInvariant();

        /// <summary>
        /// Whether discovery should narrow server-side with <c>?capability=</c>.
        /// <para>
        /// Video and rerank are matched strictly on the endpoint family instead, because their
        /// capability queries do not surface the models that declare those endpoints. The live
        /// catalog returns zero records for <c>?capability=video</c> while eight models declare
        /// <c>openai-video</c>, so narrowing server-side there would hide real models. The
        /// endpoint-family filter is the authority in both cases.
        /// </para>
        /// </summary>
        public static bool UsesCapabilityQuery(OrcaModelCapability capability)
        {
            switch (capability)
            {
                case OrcaModelCapability.VideoGeneration:
                case OrcaModelCapability.Rerank:
                    return false;
                default:
                    return true;
            }
        }
    }
}
