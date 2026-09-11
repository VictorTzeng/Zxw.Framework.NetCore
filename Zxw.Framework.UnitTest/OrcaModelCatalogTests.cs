using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zxw.Framework.AI.Credentials;
using Zxw.Framework.AI.Models;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.UnitTest
{
    /// <summary>
    /// Covers the live catalog, the per-entry-point capability filters, the verified seed fallback,
    /// and the model selector that binds filtered options to a real dropdown.
    /// </summary>
    [TestClass]
    public class OrcaModelCatalogTests
    {
        private const string FakeKey = "sk-orca-test-0000000000000000000000000001";

        private string _ambientApiKey;

        [TestInitialize]
        public void SuspendAmbientCredential() =>
            _ambientApiKey = OrcaTestEnvironment.SuspendAmbientApiKey();

        [TestCleanup]
        public void RestoreAmbientCredential() =>
            OrcaTestEnvironment.RestoreAmbientApiKey(_ambientApiKey);

        /// <summary>
        /// Fixture covering every shape the filters must distinguish: text-only chat, image-input
        /// chat, audio/video-input chat, embedding, image generation, video generation, rerank, and
        /// a record with no capability metadata at all.
        /// </summary>
        private const string FixtureJson = @"{
          ""data"": [
            { ""id"": ""vendor/text-only"", ""context_length"": 128000,
              ""supported_endpoint_types"": [""openai""],
              ""architecture"": { ""input_modalities"": [""text""] } },

            { ""id"": ""vendor/vision-chat"", ""context_length"": 200000,
              ""supported_endpoint_types"": [""openai"", ""anthropic""],
              ""architecture"": { ""input_modalities"": [""text"", ""image""] } },

            { ""id"": ""vendor/omni-chat"",
              ""supported_endpoint_types"": [""gemini""],
              ""architecture"": { ""input_modalities"": [""text"", ""image"", ""audio"", ""video""] } },

            { ""id"": ""vendor/no-modality-metadata"",
              ""supported_endpoint_types"": [""openai""] },

            { ""id"": ""vendor/embedding-model"",
              ""supported_endpoint_types"": [""embeddings""],
              ""architecture"": { ""input_modalities"": [""text""] } },

            { ""id"": ""vendor/image-generator"",
              ""supported_endpoint_types"": [""image-generation""] },

            { ""id"": ""vendor/video-generator"",
              ""supported_endpoint_types"": [""openai-video""] },

            { ""id"": ""vendor/reranker"",
              ""supported_endpoint_types"": [""jina-rerank""] },

            { ""id"": ""vendor/chat-and-image-gen"",
              ""supported_endpoint_types"": [""openai"", ""image-generation""],
              ""architecture"": { ""input_modalities"": [""text""] } },

            { ""id"": """", ""supported_endpoint_types"": [""openai""] },

            { ""id"": ""vendor/no-endpoints"", ""supported_endpoint_types"": [] }
          ]
        }";

        private static IList<OrcaModelInfo> Fixture() => OrcaModelCatalogClient.Parse(FixtureJson);

        private static IList<string> Ids(IEnumerable<OrcaModelInfo> models) =>
            models.Select(m => m.Id).ToList();

        // ---------------------------------------------------------------- parsing

        [TestMethod]
        public void Parse_KeepsVendorNamespaceVerbatim_AndSkipsUnusableRecords()
        {
            var models = Fixture();

            CollectionAssert.Contains(Ids(models).ToList(), "vendor/text-only",
                "the vendor/model namespace is preserved exactly");
            Assert.IsFalse(Ids(models).Any(id => string.IsNullOrWhiteSpace(id)),
                "a record with no usable id is skipped");

            // A record with no endpoint types is still parsed (its id and label are usable) but no
            // capability filter can admit it, so it never reaches a dropdown.
            CollectionAssert.Contains(Ids(models).ToList(), "vendor/no-endpoints");
            CollectionAssert.DoesNotContain(
                Ids(OrcaModelFilter.Select(OrcaModelCapability.Chat, models)).ToList(),
                "vendor/no-endpoints");
        }

        [TestMethod]
        public void Parse_ToleratesMalformedResponses()
        {
            Assert.AreEqual(0, OrcaModelCatalogClient.Parse("{}").Count);
            Assert.AreEqual(0, OrcaModelCatalogClient.Parse("[]").Count);

            // Callers catch JsonException; the concrete reader exception derives from it.
            var threw = false;
            try
            {
                OrcaModelCatalogClient.Parse("not json at all");
            }
            catch (System.Text.Json.JsonException)
            {
                threw = true;
            }

            Assert.IsTrue(threw, "malformed catalog JSON must surface as a JsonException");
        }

        // ---------------------------------------------------------------- capability filters

        [TestMethod]
        public void ChatFilter_KeepsTextEndpoints_AndExcludesSpecialisedModels()
        {
            var ids = Ids(OrcaModelFilter.Select(OrcaModelCapability.Chat, Fixture()));

            CollectionAssert.Contains(ids.ToList(), "vendor/text-only");
            CollectionAssert.Contains(ids.ToList(), "vendor/vision-chat");
            CollectionAssert.Contains(ids.ToList(), "vendor/omni-chat");

            // Models with no declared modality metadata are still valid *text* chat models.
            CollectionAssert.Contains(ids.ToList(), "vendor/no-modality-metadata");

            // Specialised endpoint families are excluded even though the live catalog returns some
            // of them under ?capability=chat.
            Assert.IsFalse(ids.Contains("vendor/embedding-model"));
            Assert.IsFalse(ids.Contains("vendor/image-generator"));
            Assert.IsFalse(ids.Contains("vendor/video-generator"));
            Assert.IsFalse(ids.Contains("vendor/reranker"));
            Assert.IsFalse(ids.Contains("vendor/chat-and-image-gen"),
                "a model advertising image-generation is not offered as a text chat model");
        }

        [TestMethod]
        public void MultimodalFilter_RequiresEveryUploadedModality_AndFailsClosed()
        {
            var imageOnly = Ids(OrcaModelFilter.Select(
                OrcaModelCapability.Multimodal, Fixture(), new[] { "image" }));

            CollectionAssert.Contains(imageOnly.ToList(), "vendor/vision-chat");
            CollectionAssert.Contains(imageOnly.ToList(), "vendor/omni-chat");
            Assert.IsFalse(imageOnly.Contains("vendor/text-only"), "text-only model cannot take an image");
            Assert.IsFalse(imageOnly.Contains("vendor/no-modality-metadata"),
                "a model that declares no modalities must fail closed rather than be assumed capable");

            // Adding a second required modality narrows further: only the omni model declares both.
            var imageAndAudio = Ids(OrcaModelFilter.Select(
                OrcaModelCapability.Multimodal, Fixture(), new[] { "image", "audio" }));

            CollectionAssert.Contains(imageAndAudio.ToList(), "vendor/omni-chat");
            Assert.IsFalse(imageAndAudio.Contains("vendor/vision-chat"),
                "a chat model that declares image but not audio is not offered when audio is attached");
        }

        [TestMethod]
        public void EmbeddingImageVideoAndRerankFilters_AreStrict()
        {
            var fixture = Fixture();

            CollectionAssert.AreEqual(
                new[] { "vendor/embedding-model" },
                Ids(OrcaModelFilter.Select(OrcaModelCapability.Embedding, fixture)).ToArray());

            // The image filter admits every record advertising image-generation. That includes the
            // mixed chat+image-generation record: it is a genuine image generator, and it is
            // simultaneously excluded from the text dropdown above.
            CollectionAssert.AreEquivalent(
                new[] { "vendor/image-generator", "vendor/chat-and-image-gen" },
                Ids(OrcaModelFilter.Select(OrcaModelCapability.ImageGeneration, fixture)).ToArray());

            CollectionAssert.AreEqual(
                new[] { "vendor/video-generator" },
                Ids(OrcaModelFilter.Select(OrcaModelCapability.VideoGeneration, fixture)).ToArray());

            CollectionAssert.AreEqual(
                new[] { "vendor/reranker" },
                Ids(OrcaModelFilter.Select(OrcaModelCapability.Rerank, fixture)).ToArray());
        }

        // ---------------------------------------------------------------- verified seed

        [TestMethod]
        public void VerifiedSeed_CoversTheHardGatesAndKeepsItsMetadata()
        {
            var seed = OrcaVerifiedSeed.All();
            var ids = Ids(seed);

            foreach (var expected in new[]
                     {
                         "openai/gpt-5.5", "anthropic/claude-opus-4.8", "google/gemini-3.5-flash",
                         "deepseek/deepseek-v4-pro", "orcarouter/auto"
                     })
            {
                CollectionAssert.Contains(ids.ToList(), expected);
            }

            // Metadata that must survive the fallback: reasoning ladder and input modalities.
            var gpt = seed.Single(m => m.Id == "openai/gpt-5.5");
            CollectionAssert.AreEqual(
                new[] { "low", "medium", "high", "xhigh" },
                gpt.ReasoningEfforts.ToArray(),
                "GPT-5.5's verified effort ladder must not be reduced by adding live discovery");
            CollectionAssert.Contains(gpt.InputModalities.ToList(), "image");
            Assert.AreEqual(1000000, gpt.ContextLength);

            var gemini = seed.Single(m => m.Id == "google/gemini-3.5-flash");
            CollectionAssert.Contains(gemini.InputModalities.ToList(), "video");
            CollectionAssert.Contains(gemini.InputModalities.ToList(), "audio");

            var deepseek = seed.Single(m => m.Id == "deepseek/deepseek-v4-pro");
            CollectionAssert.AreEqual(new[] { "text" }, deepseek.InputModalities.ToArray(),
                "deepseek is text-only in the live catalog and must stay text-only in the seed");

            Assert.IsTrue(seed.All(m => m.FromSeed), "seed entries are flagged so the UI can label them degraded");
        }

        [TestMethod]
        public void VerifiedSeed_SlicesByCapability()
        {
            var chat = Ids(OrcaVerifiedSeed.For(OrcaModelCapability.Chat));
            Assert.IsTrue(chat.Count >= 5);
            Assert.IsFalse(chat.Contains("openai/gpt-image-1"));

            // orcarouter/auto declares no modalities, so it fails closed out of the multimodal list.
            var multimodal = Ids(OrcaVerifiedSeed.For(OrcaModelCapability.Multimodal, new[] { "image" }));
            CollectionAssert.Contains(multimodal.ToList(), "openai/gpt-5.5");
            Assert.IsFalse(multimodal.Contains("orcarouter/auto"),
                "a seed entry with no declared modalities must not appear in a multimodal dropdown");
            Assert.IsFalse(multimodal.Contains("deepseek/deepseek-v4-pro"),
                "the text-only seed entry must not appear in a multimodal dropdown");

            // No seed entry claims a capability it has no metadata for.
            Assert.AreEqual(0, OrcaVerifiedSeed.For(OrcaModelCapability.Rerank).Count);
            Assert.AreEqual(0, OrcaVerifiedSeed.For(OrcaModelCapability.VideoGeneration).Count);
        }

        // ---------------------------------------------------------------- discovery + selector

        [TestMethod]
        public async Task Discovery_SuccessIsAuthoritative_AndDoesNotMergeTheSeed()
        {
            var options = new OrcaRouterOptions();
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.ApiKey));

            var client = new OrcaModelCatalogClient(
                new HttpClient(new RecorderHandler(FixtureJson, HttpStatusCode.OK)), options);

            var catalog = await client.DiscoverAsync(store.Load(), OrcaModelCapability.Chat);

            Assert.AreEqual(OrcaCatalogStatus.Live, catalog.Status);
            Assert.IsTrue(catalog.IsAuthoritative);
            Assert.IsFalse(catalog.IsDegraded);

            var ids = Ids(catalog.Models);
            Assert.IsFalse(ids.Contains("openai/gpt-5.5"),
                "when live discovery succeeds its result is authoritative and no seed entry is merged in");
        }

        [TestMethod]
        public async Task Discovery_UsesTheCatalogQueryForTheCapability()
        {
            var options = new OrcaRouterOptions();
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.Pkce));

            string query = null;
            var handler = new RecorderHandler(FixtureJson, HttpStatusCode.OK, uri => query = uri.Query);
            var client = new OrcaModelCatalogClient(new HttpClient(handler), options);

            await client.DiscoverAsync(store.Load(), OrcaModelCapability.Chat);
            Assert.AreEqual("?capability=chat", query);

            await client.DiscoverAsync(store.Load(), OrcaModelCapability.Embedding);
            Assert.AreEqual("?capability=embedding", query);

            await client.DiscoverAsync(store.Load(), OrcaModelCapability.ImageGeneration);
            Assert.AreEqual("?capability=image", query);

            // Video and rerank are matched on the endpoint family against the whole catalog, because
            // their capability queries do not surface the models that declare those endpoints.
            await client.DiscoverAsync(store.Load(), OrcaModelCapability.VideoGeneration);
            Assert.AreEqual(string.Empty, query,
                "video discovery must not narrow server-side, or real openai-video models are hidden");

            await client.DiscoverAsync(store.Load(), OrcaModelCapability.Rerank);
            Assert.AreEqual(string.Empty, query);
        }

        [TestMethod]
        public async Task Discovery_VideoAndRerank_FindModelsByEndpointOnTheFullCatalog()
        {
            // Mirrors the live catalog's shape: ?capability=video answers with nothing, while
            // models declaring openai-video do exist. Strict endpoint matching must still find them.
            const string catalog = @"{
              ""data"": [
                { ""id"": ""vendor/text-chat"", ""supported_endpoint_types"": [""openai""],
                  ""architecture"": { ""input_modalities"": [""text""] } },
                { ""id"": ""kling/kling-v3"", ""supported_endpoint_types"": [""openai-video""] },
                { ""id"": ""minimax/minimax-h3"", ""supported_endpoint_types"": [""openai-video""] }
              ]
            }";

            var options = new OrcaRouterOptions();
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.ApiKey));

            var client = new OrcaModelCatalogClient(
                new HttpClient(new RecorderHandler(catalog, HttpStatusCode.OK)), options);

            var video = await client.DiscoverAsync(store.Load(), OrcaModelCapability.VideoGeneration);
            CollectionAssert.AreEqual(
                new[] { "kling/kling-v3", "minimax/minimax-h3" },
                Ids(video.Models).ToArray());
            Assert.IsFalse(Ids(video.Models).Contains("vendor/text-chat"),
                "a text chat model must never appear in the video dropdown");

            var rerank = await client.DiscoverAsync(store.Load(), OrcaModelCapability.Rerank);
            Assert.AreEqual(0, rerank.Models.Count,
                "no model declares jina-rerank, so the rerank dropdown is empty rather than guessed");
            Assert.AreEqual(OrcaCatalogStatus.Live, rerank.Status);
        }

        [TestMethod]
        public async Task Discovery_FallsBackToTheVerifiedSeed_OnAuthRejectionAndNetworkFailure()
        {
            var options = new OrcaRouterOptions();
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());

            // Not authenticated at all.
            var anonymous = new OrcaModelCatalogClient(
                new HttpClient(new RecorderHandler("{}", HttpStatusCode.OK)), options);
            var noCredential = await anonymous.DiscoverAsync(null, OrcaModelCapability.Chat);
            Assert.AreEqual(OrcaCatalogError.AuthRequired, noCredential.Error);
            Assert.AreEqual(OrcaCatalogStatus.SeedFallback, noCredential.Status);
            Assert.IsTrue(noCredential.IsDegraded);
            Assert.IsTrue(noCredential.Models.Count > 0);

            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.Pkce));

            // Rejected credential.
            var rejected = new OrcaModelCatalogClient(
                new HttpClient(new RecorderHandler("{}", HttpStatusCode.Unauthorized)), options);
            var rejectedCatalog = await rejected.DiscoverAsync(store.Load(), OrcaModelCapability.Chat);
            Assert.AreEqual(OrcaCatalogError.AuthRejected, rejectedCatalog.Error);
            Assert.AreEqual(OrcaCatalogStatus.SeedFallback, rejectedCatalog.Status);
            Assert.IsTrue(rejectedCatalog.Models.Count > 0, "an outage must not empty the dropdown");

            // Transport failure.
            var offline = new OrcaModelCatalogClient(
                new HttpClient(new ThrowingHandler()), options);
            var offlineCatalog = await offline.DiscoverAsync(store.Load(), OrcaModelCapability.Chat);
            Assert.AreEqual(OrcaCatalogError.Network, offlineCatalog.Error);
            Assert.IsTrue(offlineCatalog.Models.Count > 0);

            // Rate limited.
            var limited = new OrcaModelCatalogClient(
                new HttpClient(new RecorderHandler("{}", (HttpStatusCode)429)), options);
            var limitedCatalog = await limited.DiscoverAsync(store.Load(), OrcaModelCapability.Chat);
            Assert.AreEqual(OrcaCatalogError.RateLimited, limitedCatalog.Error);
            Assert.IsTrue(limitedCatalog.Models.Count > 0);
        }

        [TestMethod]
        public async Task Discovery_MalformedResponseFallsBack_AndNeverBecomesFreeText()
        {
            var options = new OrcaRouterOptions();
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.ApiKey));

            var client = new OrcaModelCatalogClient(
                new HttpClient(new RecorderHandler("<<<not json>>>", HttpStatusCode.OK)), options);

            var catalog = await client.DiscoverAsync(store.Load(), OrcaModelCapability.Chat);

            Assert.AreEqual(OrcaCatalogError.Network, catalog.Error);
            Assert.AreEqual(OrcaCatalogStatus.SeedFallback, catalog.Status);
            Assert.IsTrue(catalog.Models.Count > 0);
            Assert.IsFalse(catalog.IsAuthoritative);
        }

        [TestMethod]
        public async Task Selector_RebuildsOptionsForEachCapability_AndInvalidatesStaleSelections()
        {
            var options = new OrcaRouterOptions();
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.ApiKey));

            var uriSeen = new List<string>();
            var selector = new OrcaModelSelector(
                new OrcaModelCatalogClient(
                    new HttpClient(new RecorderHandler(FixtureJson, HttpStatusCode.OK, u => uriSeen.Add(u.Query))),
                    options),
                store);

            // Text entry point.
            var text = await selector.GetOptionsAsync(OrcaModelCapability.Chat);
            var textIds = Ids(text.Models);
            Assert.AreEqual(OrcaCatalogStatus.Live, text.Status);
            CollectionAssert.Contains(textIds.ToList(), "vendor/text-only");

            // The selected text model is fine while no attachment is present.
            Assert.IsNull(OrcaModelSelector.ValidateSelection(text, "vendor/text-only", OrcaModelCapability.Chat));

            // Attach an image: the options are recomputed, and the text-only selection is no longer
            // admitted. The caller is told to clear it rather than keep a silently wrong value.
            var multimodal = await selector.GetOptionsAsync(
                OrcaModelCapability.Multimodal, new[] { "image" });
            var multiIds = Ids(multimodal.Models);

            CollectionAssert.Contains(multiIds.ToList(), "vendor/vision-chat");
            Assert.IsFalse(multiIds.Contains("vendor/text-only"),
                "with an image attached the selector must offer only models that declare image input");
            Assert.IsFalse(multiIds.Contains("vendor/no-modality-metadata"),
                "undeclared capability fails closed");

            var reason = OrcaModelSelector.ValidateSelection(
                multimodal, "vendor/text-only", OrcaModelCapability.Multimodal, new[] { "image" });
            Assert.IsNotNull(reason, "the stale selection must be invalidated");
            StringAssert.Contains(reason, "vendor/text-only");

            // Both capability queries were actually issued; nothing is served from a single cache.
            CollectionAssert.Contains(uriSeen.ToList(), "?capability=chat");
            CollectionAssert.Contains(uriSeen.ToList(), "?capability=chat");
        }

        [TestMethod]
        public async Task Selector_LastKnownGoodSurvivesAnOutage_AndIsRelabelled()
        {
            var options = new OrcaRouterOptions();
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.ApiKey));

            var failing = false;
            var handler = new SwitchingHandler(() => failing ? null : FixtureJson);
            var selector = new OrcaModelSelector(new OrcaModelCatalogClient(new HttpClient(handler), options), store);

            var live = await selector.GetOptionsAsync(OrcaModelCapability.Chat);
            Assert.AreEqual(OrcaCatalogStatus.Live, live.Status);
            var liveIds = Ids(live.Models);

            // The catalog endpoint goes away.
            failing = true;
            var degraded = await selector.GetOptionsAsync(OrcaModelCapability.Chat);

            Assert.AreEqual(OrcaCatalogStatus.LastKnownGood, degraded.Status);
            Assert.IsTrue(degraded.IsDegraded);
            Assert.IsNotNull(degraded.Message);

            // The last-known-good list is the one actually fetched from the API, not the seed.
            CollectionAssert.AreEqual(liveIds.ToArray(), Ids(degraded.Models).ToArray());

            // A capability that was never fetched still falls back to the verified seed, clearly labelled.
            var embedding = await selector.GetOptionsAsync(OrcaModelCapability.Embedding);
            Assert.AreEqual(OrcaCatalogStatus.SeedFallback, embedding.Status);
            Assert.AreEqual(0, embedding.Models.Count,
                "the seed makes no embedding claim, so the dropdown is empty rather than inventing entries");
        }

        // ---------------------------------------------------------------- helpers

        private sealed class InMemoryPersistence : IOrcaCredentialPersistence
        {
            private OrcaPersistedCredential _credential;

            public OrcaPersistedCredential Read() => _credential;

            public void Write(OrcaPersistedCredential credential) => _credential = credential;

            public void Delete() => _credential = null;
        }

        private sealed class RecorderHandler : HttpMessageHandler
        {
            private readonly string _body;
            private readonly HttpStatusCode _status;
            private readonly Action<Uri> _observe;

            public RecorderHandler(string body, HttpStatusCode status, Action<Uri> observe = null)
            {
                _body = body;
                _status = status;
                _observe = observe;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _observe?.Invoke(request.RequestUri);
                Assert.AreEqual("Bearer", request.Headers.Authorization?.Scheme,
                    "catalog requests carry a Bearer credential");
                return Task.FromResult(new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_body)
                });
            }
        }

        /// <summary>Returns a body while healthy, and throws a transport failure when not.</summary>
        private sealed class SwitchingHandler : HttpMessageHandler
        {
            private readonly Func<string> _body;

            public SwitchingHandler(Func<string> body) => _body = body;

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var body = _body();
                if (body == null)
                    throw new HttpRequestException("catalog is offline");

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body)
                });
            }
        }

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken) =>
                throw new HttpRequestException("no route to host");
        }
    }
}
