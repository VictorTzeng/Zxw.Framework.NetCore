using System.Collections.Generic;
using Zxw.Framework.AI.Credentials;
using Zxw.Framework.AI.Models;

namespace Zxw.Framework.AI.Models
{
    /// <summary>
    /// The bounded, verified cold-start catalog.
    /// <para>
    /// This is an <em>outage fallback only</em>. When live discovery succeeds its result is
    /// authoritative and none of these entries are merged into it. Every id here was verified
    /// present in the live catalog on 2026-09-11, and every capability flag is copied from the
    /// catalog's own metadata rather than inferred from the model name.
    /// </para>
    /// </summary>
    public static class OrcaVerifiedSeed
    {
        /// <summary>Date the entries below were last verified against the live catalog.</summary>
        public const string VerifiedOn = "2026-09-11";

        /// <summary>Source of the verified entries, recorded so the fallback is auditable.</summary>
        public const string VerifiedSource = "https://api.orcarouter.ai/v1/models";

        public static IList<OrcaModelInfo> All() => new List<OrcaModelInfo>
        {
            new OrcaModelInfo
            {
                Id = "openai/gpt-5.5",
                DisplayName = "OpenAI: GPT-5.5",
                // Catalog record carries max_completion_tokens=128000 and describes a 1M+ token
                // context. The reasoning ladder is the verified one; it is not guessed from the id.
                ContextLength = 1000000,
                MaxCompletionTokens = 128000,
                SupportedEndpointTypes = new List<string> { "openai", "openai-response" },
                InputModalities = new List<string> { "text", "image", "file" },
                OutputModalities = new List<string> { "text" },
                ReasoningEfforts = new List<string> { "low", "medium", "high", "xhigh" },
                FromSeed = true
            },
            new OrcaModelInfo
            {
                Id = "anthropic/claude-opus-4.8",
                DisplayName = "Anthropic: Claude Opus 4.8",
                ContextLength = 1000000,
                MaxCompletionTokens = 128000,
                SupportedEndpointTypes = new List<string> { "openai", "anthropic", "openai-response" },
                InputModalities = new List<string> { "text", "image", "file" },
                OutputModalities = new List<string> { "text" },
                ReasoningEfforts = new List<string> { "low", "medium", "high" },
                FromSeed = true
            },
            new OrcaModelInfo
            {
                Id = "google/gemini-3.5-flash",
                DisplayName = "Gemini 3.5 Flash",
                ContextLength = 1048576,
                MaxCompletionTokens = 65536,
                SupportedEndpointTypes = new List<string> { "openai", "gemini" },
                InputModalities = new List<string> { "text", "image", "video", "file", "audio" },
                OutputModalities = new List<string> { "text" },
                ReasoningEfforts = new List<string> { "low", "medium", "high" },
                FromSeed = true
            },
            new OrcaModelInfo
            {
                Id = "deepseek/deepseek-v4-pro",
                DisplayName = "DeepSeek: DeepSeek V4 Pro",
                ContextLength = 1048576,
                MaxCompletionTokens = 384000,
                SupportedEndpointTypes = new List<string> { "openai", "openai-response" },
                InputModalities = new List<string> { "text" },
                OutputModalities = new List<string> { "text" },
                ReasoningEfforts = new List<string> { "low", "medium", "high" },
                FromSeed = true
            },
            new OrcaModelInfo
            {
                // Catalog record declares no architecture block, so this entry stays text-only and
                // is therefore excluded from every multimodal dropdown. Absent metadata fails closed.
                Id = "orcarouter/auto",
                DisplayName = "OrcaRouter Auto",
                SupportedEndpointTypes = new List<string> { "openai", "openai-response", "anthropic", "gemini" },
                FromSeed = true
            }
        };

        /// <summary>The seed sliced down to the models a given capability may offer.</summary>
        public static IList<OrcaModelInfo> For(
            OrcaModelCapability capability,
            IReadOnlyList<string> requiredInputModalities = null) =>
            OrcaModelFilter.Select(capability, All(), requiredInputModalities);
    }
}
