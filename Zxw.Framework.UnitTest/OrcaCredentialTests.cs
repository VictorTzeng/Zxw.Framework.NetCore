using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zxw.Framework.AI.Abstractions;
using Zxw.Framework.AI.Clients;
using Zxw.Framework.AI.Credentials;
using Zxw.Framework.AI.Models;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.UnitTest
{
    /// <summary>
    /// Covers the credential seam: both authentication choices must independently produce the same
    /// <see cref="OrcaCredential"/>, and downstream code must not be able to tell them apart.
    /// All keys and codes here are fakes.
    /// </summary>
    [TestClass]
    public class OrcaCredentialTests
    {
        private const string FakeKey = "sk-orca-test-0000000000000000000000000001";
        private const string OtherFakeKey = "sk-orca-test-0000000000000000000000000002";

        private string _ambientApiKey;

        [TestInitialize]
        public void SuspendAmbientCredential() =>
            _ambientApiKey = OrcaTestEnvironment.SuspendAmbientApiKey();

        [TestCleanup]
        public void RestoreAmbientCredential() =>
            OrcaTestEnvironment.RestoreAmbientApiKey(_ambientApiKey);

        // ---------------------------------------------------------------- API key adapter

        [TestMethod]
        public void ApiKeyAdapter_SaveReadMaskClear_RoundTrips()
        {
            var store = new OrcaCredentialStore(new OrcaRouterOptions(), new InMemoryPersistence());
            var provider = new OrcaApiKeyCredentialProvider(new OrcaRouterOptions(), store);

            // Save
            var saved = provider.ApplyKey(FakeKey);
            Assert.AreEqual(OrcaCredentialState.Active, saved.State);
            Assert.IsTrue(saved.CanClear);

            // Read back through the seam
            var acquired = provider.Acquire();
            Assert.IsTrue(acquired.Succeeded);
            Assert.AreEqual(FakeKey, acquired.Credential.ApiKey);
            Assert.AreEqual(OrcaCredentialSource.ApiKey, acquired.Credential.Source);

            // Masked: the status never carries the whole key
            var status = provider.GetStatus();
            Assert.AreNotEqual(FakeKey, status.MaskedKey);
            StringAssert.StartsWith(status.MaskedKey, "sk-orca-");
            Assert.IsFalse(status.MaskedKey.Contains("0000000000000000000000000001"));

            // Clear
            provider.Clear();
            Assert.IsFalse(provider.Acquire().Succeeded);
            Assert.AreEqual(OrcaCredentialState.Missing, provider.GetStatus().State);
        }

        [TestMethod]
        public void ApiKeyAdapter_RejectsMalformedInput_WithoutStoring()
        {
            var store = new OrcaCredentialStore(new OrcaRouterOptions(), new InMemoryPersistence());
            var provider = new OrcaApiKeyCredentialProvider(new OrcaRouterOptions(), store);

            var status = provider.ApplyKey("not-a-key");
            Assert.AreEqual(OrcaCredentialState.Missing, status.State);
            Assert.IsFalse(provider.Acquire().Succeeded);
        }

        [TestMethod]
        public void ApiKeyAdapter_ReadsConfiguredValueAndEnvironmentStyleConfig()
        {
            var options = new OrcaRouterOptions { ApiKey = FakeKey };
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            var provider = new OrcaApiKeyCredentialProvider(options, store);

            var acquired = provider.Acquire();
            Assert.IsTrue(acquired.Succeeded);
            Assert.AreEqual(FakeKey, acquired.Credential.ApiKey);

            // A value supplied by host configuration is not ours to delete; after Clear the
            // provider reports what is actually in effect rather than pretending it is gone.
            provider.Clear();
            Assert.IsTrue(provider.Acquire().Succeeded, "host-configured key remains resolvable after Clear");
        }

        // ---------------------------------------------------------------- PKCE adapter

        [TestMethod]
        public void Pkce_VerifierAndState_AreFreshAndChallengeIsS256OfVerifier()
        {
            var v1 = OrcaPkce.CreateVerifier();
            var v2 = OrcaPkce.CreateVerifier();
            var s1 = OrcaPkce.CreateState();
            var s2 = OrcaPkce.CreateState();

            Assert.AreNotEqual(v1, v2, "verifier must be fresh per attempt");
            Assert.AreNotEqual(s1, s2, "state must be fresh per attempt");

            // base64url without padding
            Assert.IsFalse(v1.Contains("="), "no padding");
            Assert.IsFalse(v1.Contains("+"), "url-safe alphabet");
            Assert.IsFalse(v1.Contains("/"), "url-safe alphabet");

            // challenge is exactly base64url(sha256(verifier))
            var expected = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.Create()
                        .ComputeHash(Encoding.ASCII.GetBytes(v1)))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            Assert.AreEqual(expected, OrcaPkce.CreateChallenge(v1));
        }

        [TestMethod]
        public void Pkce_AuthorizeUrl_UsesAuthOriginAndCarriesNoVerifier()
        {
            var options = new OrcaRouterOptions();
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), new OrcaCredentialStore(options, new InMemoryPersistence()), options);

            var session = provider.Start(appName: "Zxw.Framework.NetCore");

            var url = session.AuthorizeUrl;
            StringAssert.StartsWith(url, "https://www.orcarouter.ai/auth?");
            StringAssert.Contains(url, "callback_url=oob");
            StringAssert.Contains(url, "code_challenge_method=S256");
            StringAssert.Contains(url, "code_challenge=");
            StringAssert.Contains(url, "state=");
            StringAssert.Contains(url, "app_name=Zxw.Framework.NetCore");
            StringAssert.Contains(url, "scope=api");

            // The verifier never rides on the authorize URL. The challenge is 43 characters of
            // base64url for a 32-byte verifier; anything verifier-shaped would be longer.
            StringAssert.Contains(url, "code_challenge=");
            Assert.IsFalse(url.Contains("code_verifier"), "authorize URL must not carry the verifier");

            // It must never point at the relay.
            Assert.IsFalse(url.Contains("api.orcarouter.ai"), "authorize must not use the inference origin");
            Assert.IsFalse(url.Contains("/v1/auth/keys"), "authorize must not use the relay auth path");
        }

        [TestMethod]
        public void Origins_AreResolvedIndependently_AndOverridesWin()
        {
            // Defaults
            Assert.AreEqual("https://www.orcarouter.ai", OrcaAuthEndpoints.ResolveAuthBaseUrl());
            Assert.AreEqual("https://api.orcarouter.ai/v1", OrcaAuthEndpoints.ResolveApiBaseUrl());

            // Explicit overrides win over the shared fallback
            Assert.AreEqual("https://auth.internal.test",
                OrcaAuthEndpoints.ResolveAuthBaseUrl("https://auth.internal.test", "https://shared.test"));
            Assert.AreEqual("https://relay.internal.test/v1",
                OrcaAuthEndpoints.ResolveApiBaseUrl("https://relay.internal.test/v1", "https://shared.test"));

            // Shared self-hosted fallback serves both roles, but is never used to derive one from
            // the other: the auth origin does not gain the relay's /v1 segment.
            Assert.AreEqual("https://shared.test", OrcaAuthEndpoints.ResolveAuthBaseUrl(null, "https://shared.test"));
            Assert.AreEqual("https://shared.test/v1", OrcaAuthEndpoints.ResolveApiBaseUrl(null, "https://shared.test"));

            // The two public origins are genuinely different hosts
            Assert.AreNotEqual(
                new Uri(OrcaAuthEndpoints.DefaultAuthBaseUrl).Host,
                new Uri(OrcaAuthEndpoints.DefaultApiBaseUrl).Host);
        }

        [TestMethod]
        public void Origins_TheRelayBaseUrlIsNeverTreatedAsTheAuthOrigin()
        {
            // The library's own default inference base URL must never leak into the auth role.
            // Feeding it in as a shared fallback is the "derive one origin from the other" mistake.
            Assert.ThrowsException<OrcaConfigurationException>(() =>
                OrcaAuthEndpoints.ResolveAuthBaseUrl(
                    null, OrcaAuthEndpoints.DefaultApiBaseUrl));

            // A shared self-hosted origin is legitimate; it is not the relay route.
            Assert.AreEqual("https://gateway.internal",
                OrcaAuthEndpoints.ResolveAuthBaseUrl(null, "https://gateway.internal"));
        }

        [TestMethod]
        public void Options_ResolveEachOriginIndependently_WithOverridesWinning()
        {
            // Defaults: two different origins, neither derived from the other.
            var defaults = new OrcaRouterOptions();
            Assert.AreEqual(OrcaAuthEndpoints.DefaultAuthBaseUrl, defaults.ResolveAuthBaseUrl());
            Assert.AreEqual(OrcaAuthEndpoints.DefaultApiBaseUrl, defaults.ResolveApiBaseUrl());

            // Setting only the API base (the pre-existing property) leaves auth on its own default.
            var apiOnly = new OrcaRouterOptions { BaseUrl = "https://relay.internal.test/v1" };
            Assert.AreEqual("https://relay.internal.test/v1", apiOnly.ResolveApiBaseUrl());
            Assert.AreEqual(OrcaAuthEndpoints.DefaultAuthBaseUrl, apiOnly.ResolveAuthBaseUrl(),
                "changing the inference base must not move the authentication origin");

            // Setting only the auth base leaves inference on its own default.
            var authOnly = new OrcaRouterOptions { AuthBaseUrl = "https://auth.internal.test" };
            Assert.AreEqual("https://auth.internal.test", authOnly.ResolveAuthBaseUrl());
            Assert.AreEqual(OrcaAuthEndpoints.DefaultApiBaseUrl, authOnly.ResolveApiBaseUrl(),
                "changing the authentication origin must not move the inference origin");

            // A shared self-hosted base serves both roles, keeping the relay suffix only on the API side.
            var shared = new OrcaRouterOptions { SharedBaseUrl = "https://one-box.internal" };
            Assert.AreEqual("https://one-box.internal", shared.ResolveAuthBaseUrl());
            Assert.AreEqual("https://one-box.internal/v1", shared.ResolveApiBaseUrl());

            // Explicit overrides take precedence over the shared base for their own role.
            var both = new OrcaRouterOptions
            {
                SharedBaseUrl = "https://one-box.internal",
                AuthBaseUrl = "https://auth.example.test",
                ApiBaseUrl = "https://api.example.test/v1"
            };
            Assert.AreEqual("https://auth.example.test", both.ResolveAuthBaseUrl());
            Assert.AreEqual("https://api.example.test/v1", both.ResolveApiBaseUrl());
        }

        [TestMethod]
        public void ExchangeUri_IsOnAuthOrigin_NeverTheRelayAuthPath()
        {
            var uri = OrcaAuthEndpoints.BuildExchangeUri(OrcaAuthEndpoints.DefaultAuthBaseUrl);
            Assert.AreEqual("https://www.orcarouter.ai/api/v1/auth/keys", uri.ToString());
            Assert.IsFalse(uri.ToString().Contains("api.orcarouter.ai"),
                "exchange must not use the inference origin");

            // The classic mistake is rejected rather than silently 404ing at runtime.
            Assert.ThrowsException<OrcaConfigurationException>(() =>
                OrcaAuthEndpoints.BuildExchangeUri("https://api.orcarouter.ai/v1"));
        }

        [TestMethod]
        public void Origins_RejectPlainHttpForRemoteHosts_AndAllowLoopback()
        {
            Assert.ThrowsException<OrcaConfigurationException>(() =>
                OrcaAuthEndpoints.NormalizeOrigin("http://evil.test", "auth"));

            // Loopback http is allowed for local development.
            Assert.AreEqual("http://127.0.0.1:8080", OrcaAuthEndpoints.NormalizeOrigin("http://127.0.0.1:8080", "auth"));
            Assert.AreEqual("http://localhost:9000", OrcaAuthEndpoints.NormalizeOrigin("http://localhost:9000", "auth"));
        }

        [TestMethod]
        public void Pkce_StateMismatch_IsRejectedAndUsesNoCode()
        {
            var options = new OrcaRouterOptions();
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), new OrcaCredentialStore(options, new InMemoryPersistence()), options);
            var session = provider.Start();

            var result = session.CompleteAsync("fake-code", returnedState: "a-different-state").GetAwaiter().GetResult();

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains(result.Status.Message, "state did not match");
        }

        [TestMethod]
        public void ConnectSessionManager_ReleasesTheLockOnEveryTerminalPath()
        {
            // Success, denial, exchange error, timeout, explicit cancel, switching method, closing
            // the dialog, and pagehide all funnel through Release/clearConnectUi. What matters here
            // is that the server-side lock is not left held by an abandoned attempt, and that a
            // stale attempt id cannot be resumed after the lock moved on.
            var manager = new OrcaConnectSessionManager();
            var options = new OrcaRouterOptions();
            var provider = new OrcaPkceCredentialProvider(
                new HttpClient(),
                new OrcaCredentialStore(options, new InMemoryPersistence()),
                options);

            var first = manager.Begin(provider.Start(), provider.Id);
            Assert.IsTrue(manager.IsBusy);
            Assert.IsTrue(manager.TryGetCurrent(first.AttemptId, out _));

            manager.Release(first.AttemptId);
            Assert.IsFalse(manager.IsBusy, "a completed or cancelled attempt must release the lock");
            Assert.IsFalse(manager.TryGetCurrent(first.AttemptId, out _),
                "a released attempt can no longer be resumed");

            // Switching authentication method, or starting a new sign-in, supersedes the old one
            // instead of leaving its lock held.
            var second = manager.Begin(provider.Start(), provider.Id);
            var third = manager.Begin(provider.Start(), provider.Id);
            Assert.AreNotEqual(second.Generation, third.Generation);
            Assert.IsFalse(manager.TryGetCurrent(second.AttemptId, out _),
                "a superseded attempt must not be resumable");
            Assert.IsTrue(manager.TryGetCurrent(third.AttemptId, out _));
        }

        [TestMethod]
        public void Pkce_FixedTimeEquals_BehavesLikeEquality()
        {
            Assert.IsTrue(OrcaPkce.FixedTimeEquals("abc123", "abc123"));
            Assert.IsFalse(OrcaPkce.FixedTimeEquals("abc123", "abc124"));
            Assert.IsFalse(OrcaPkce.FixedTimeEquals("abc", "abcd"));
            Assert.IsFalse(OrcaPkce.FixedTimeEquals(null, "abcd"));
        }

        [TestMethod]
        public void Pkce_ScopeSatisfaction_RefusesDowngrade()
        {
            // The same grant that was asked for, and a wider one, both satisfy the request.
            Assert.IsTrue(OrcaPkceCredentialProvider.ScopeSatisfies("api", "api"));
            Assert.IsTrue(OrcaPkceCredentialProvider.ScopeSatisfies("api", "connector"));

            // A narrower grant than requested is a downgrade and must be refused.
            Assert.IsFalse(OrcaPkceCredentialProvider.ScopeSatisfies("connector", "api"));

            // No grant at all never satisfies anything.
            Assert.IsFalse(OrcaPkceCredentialProvider.ScopeSatisfies("api", null));
            Assert.IsFalse(OrcaPkceCredentialProvider.ScopeSatisfies("api", ""));
        }

        // ---------------------------------------------------------------- seam equivalence

        [TestMethod]
        public void BothAdapters_ProduceTheSameCredentialShape_AndDownstreamIgnoresSource()
        {
            var options = new OrcaRouterOptions { ApiKey = FakeKey };
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());

            var apiKeyProvider = new OrcaApiKeyCredentialProvider(options, store);
            var pkceProvider = new OrcaPkceCredentialProvider(new HttpClient(), store, options);

            var fromApiKey = apiKeyProvider.Acquire();
            Assert.IsTrue(fromApiKey.Succeeded);

            // A PKCE-issued key is stored through the same seam, then read back by the same call.
            store.Save(new OrcaCredential(OtherFakeKey, OrcaCredentialSource.Pkce, "user-1", "api"));
            var fromPkce = pkceProvider.Acquire();
            Assert.IsTrue(fromPkce.Succeeded);

            Assert.AreEqual(typeof(OrcaCredential), fromApiKey.Credential.GetType());
            Assert.AreEqual(typeof(OrcaCredential), fromPkce.Credential.GetType());
            Assert.AreEqual(OrcaCredentialSource.ApiKey, fromApiKey.Credential.Source);
            Assert.AreEqual(OrcaCredentialSource.Pkce, fromPkce.Credential.Source);

            // Both are ordinary OrcaRouter keys: same type, same shape, no extra per-source fields.
            Assert.IsTrue(fromApiKey.Credential.HasKey);
            Assert.IsTrue(fromPkce.Credential.HasKey);

            // The credential the catalog consumes does not branch on Source: a PKCE key works
            // anywhere an API key does.
            Assert.AreEqual(OtherFakeKey, store.Load().ApiKey);
        }

        [TestMethod]
        public void CredentialStore_PrefersConfigurationOverPersistedKey()
        {
            var persistence = new InMemoryPersistence();
            persistence.Write(new OrcaPersistedCredential
            {
                ApiKey = OtherFakeKey,
                Source = nameof(OrcaCredentialSource.Pkce)
            });

            var options = new OrcaRouterOptions { ApiKey = FakeKey };
            var store = new OrcaCredentialStore(options, persistence);

            Assert.AreEqual(FakeKey, store.Load().ApiKey);
            Assert.AreEqual(OrcaCredentialSource.ApiKey, store.Load().Source);
        }

        [TestMethod]
        public void CredentialStore_PersistedPkceKeySurvivesRestart()
        {
            var persistence = new InMemoryPersistence();
            var first = new OrcaCredentialStore(new OrcaRouterOptions(), persistence);
            first.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.Pkce, "user-1", "api"));

            // A new store over the same persistence is what a process restart looks like. The key
            // is reused rather than re-authorized, which matters because OrcaRouter caps
            // PKCE-issued keys at ten per user per 24 hours.
            var second = new OrcaCredentialStore(new OrcaRouterOptions(), persistence);
            var reloaded = second.Load();

            Assert.IsNotNull(reloaded);
            Assert.AreEqual(FakeKey, reloaded.ApiKey);
            Assert.AreEqual(OrcaCredentialSource.Pkce, reloaded.Source);
            Assert.AreEqual("user-1", reloaded.AccountId);
            Assert.AreEqual("api", reloaded.Scope);
        }

        [TestMethod]
        public void CorruptedStore_IsTreatedAsMissing_NotFatal()
        {
            var persistence = new InMemoryPersistence { Raw = "{ this is not json" };
            var store = new OrcaCredentialStore(new OrcaRouterOptions(), persistence);

            Assert.IsNull(store.Load());
            Assert.AreEqual(OrcaCredentialState.Missing, store.GetStatus().State);
        }

        // ---------------------------------------------------------------- redaction

        [TestMethod]
        public void Redaction_MasksKeys_AndScrubsThemFromMessages()
        {
            var masked = OrcaSecretRedaction.Mask(FakeKey);
            Assert.AreNotEqual(FakeKey, masked);
            Assert.IsFalse(masked.Contains("0000000000000000000000000001"));

            var scrubbed = OrcaSecretRedaction.Scrub($"upstream said: Bearer {FakeKey} is bad", FakeKey);
            Assert.IsFalse(scrubbed.Contains(FakeKey), "the key must not survive scrubbing");
            StringAssert.Contains(scrubbed, OrcaSecretRedaction.Placeholder);

            // A key we were never handed is still stripped, so an echoed value cannot leak.
            var notOurs = "sk-orca-someoneelseskey000000000000";
            Assert.IsFalse(OrcaSecretRedaction.Scrub("token=" + notOurs).Contains(notOurs));
        }

        [TestMethod]
        public void Redaction_ScrubsVerifierFromExchangeErrorBodies()
        {
            var verifier = OrcaPkce.CreateVerifier();
            var body = $"{{\"error\":\"bad\",\"echoed_verifier\":\"{verifier}\"}}";

            var scrubbed = OrcaSecretRedaction.Scrub(body, verifier);
            Assert.IsFalse(scrubbed.Contains(verifier), "the verifier must not appear in a surfaced error");
        }

        // ---------------------------------------------------------------- terminal reauth

        [TestMethod]
        public void RevokedCredential_IsMarkedNeedsReauth_WithoutAnyRefreshAttempt()
        {
            var store = new OrcaCredentialStore(new OrcaRouterOptions(), new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.Pkce, "user-1", "api"));

            var credential = store.Load();
            Assert.IsTrue(store.MarkNeedsReauth(credential.Generation));

            var status = store.GetStatus();
            Assert.AreEqual(OrcaCredentialState.NeedsReauth, status.State);
            StringAssert.Contains(status.Message, "not refreshed");
            Assert.IsTrue(store.NeedsReauth);
        }

        [TestMethod]
        public void StaleGenerationFailure_DoesNotPoisonNewCredential()
        {
            var store = new OrcaCredentialStore(new OrcaRouterOptions(), new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.Pkce, "user-1", "api"));
            var oldGeneration = store.Load().Generation;

            // The user reconnects; a new key with a new generation replaces the old one.
            store.Save(new OrcaCredential(OtherFakeKey, OrcaCredentialSource.Pkce, "user-1", "api"));
            var newGeneration = store.Load().Generation;
            Assert.AreNotEqual(oldGeneration, newGeneration);

            // A late 401 raised by a request that carried the old key must not mark the new one.
            var marked = store.MarkNeedsReauth(oldGeneration);
            Assert.IsFalse(marked, "a stale failure must not touch the current credential");
            Assert.IsFalse(store.NeedsReauth);
            Assert.AreEqual(OrcaCredentialState.Active, store.GetStatus().State);
        }

        [TestMethod]
        public async Task ChatClient_On401_MarksTheExactGenerationAndDoesNotRetry()
        {
            var store = new OrcaCredentialStore(new OrcaRouterOptions(), new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.Pkce, "user-1", "api"));
            var generation = store.Load().Generation;

            var calls = 0;
            var handler = new StubHandler(request =>
            {
                calls++;
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("{\"error\":\"revoked\"}")
                };
            });

            var client = new OrcaRouterChatClient(
                new FixedFactory(handler),
                Microsoft.Extensions.Options.Options.Create(new OrcaRouterOptions()),
                store);

            await Assert.ThrowsExceptionAsync<OrcaAuthenticationException>(() =>
                client.CompleteAsync(new ChatCompletionRequest
                {
                    Messages = new List<ChatMessage> { ChatMessage.User("hi") }
                }));

            Assert.AreEqual(1, calls, "a rejected credential must not be retried or refreshed");
            Assert.IsTrue(store.NeedsReauth);
            Assert.AreEqual(generation, store.Load().Generation, "the rejected generation is still identifiable");
        }

        [TestMethod]
        public async Task ChatClient_UsesWhicheverCredentialTheSeamHolds()
        {
            // Two clients over two stores, one API-key-sourced and one PKCE-sourced. Neither client
            // is given any indication of which is which, and both must behave identically.
            foreach (var source in new[] { OrcaCredentialSource.ApiKey, OrcaCredentialSource.Pkce })
            {
                var store = new OrcaCredentialStore(new OrcaRouterOptions(), new InMemoryPersistence());
                store.Save(new OrcaCredential(FakeKey, source, "user-1", "api"));

                string authHeader = null;
                var handler = new StubHandler(request =>
                {
                    authHeader = request.Headers.Authorization?.ToString();
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            "{\"model\":\"orcarouter/auto\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"ok\"}}]}")
                    };
                });

                var client = new OrcaRouterChatClient(
                    new FixedFactory(handler),
                    Microsoft.Extensions.Options.Options.Create(new OrcaRouterOptions()),
                    store);

                var response = await client.CompleteAsync(new ChatCompletionRequest
                {
                    Messages = new List<ChatMessage> { ChatMessage.User("hi") }
                });

                Assert.AreEqual("ok", response.GetContent());
                Assert.AreEqual("Bearer " + FakeKey, authHeader, $"source={source}");
            }
        }

        [TestMethod]
        public async Task InferenceAndCatalog_OnlyEverUseTheApiOrigin()
        {
            var seen = new List<string>();
            var handler = new StubHandler(request =>
            {
                seen.Add(request.RequestUri.GetLeftPart(UriPartial.Authority));
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"data\":[]}")
                };
            });

            var options = new OrcaRouterOptions();
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.Pkce));

            var catalog = new OrcaModelCatalogClient(new HttpClient(handler), options);
            await catalog.DiscoverAsync(store.Load(), OrcaModelCapability.Chat);

            Assert.AreEqual(1, seen.Count);
            Assert.AreEqual("https://api.orcarouter.ai", seen[0]);
            Assert.IsFalse(seen.Any(origin => origin.Contains("www.orcarouter.ai")),
                "model discovery must never touch the auth origin");
        }

        [TestMethod]
        public async Task InferenceRequests_GoToTheApiOrigin_NeverTheAuthOrigin()
        {
            var uris = new List<Uri>();
            var handler = new StubHandler(request =>
            {
                uris.Add(request.RequestUri);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"model\":\"orcarouter/auto\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"ok\"}}]}")
                };
            });

            var store = new OrcaCredentialStore(new OrcaRouterOptions(), new InMemoryPersistence());
            store.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.ApiKey));

            var client = new OrcaRouterChatClient(
                new FixedFactory(handler),
                Microsoft.Extensions.Options.Options.Create(new OrcaRouterOptions()),
                store);

            await client.CompleteAsync(new ChatCompletionRequest
            {
                Messages = new List<ChatMessage> { ChatMessage.User("hi") }
            });

            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual("api.orcarouter.ai", uris[0].Host, "inference belongs on the API origin");
            Assert.AreEqual("/v1/chat/completions", uris[0].AbsolutePath);
            Assert.AreNotEqual("www.orcarouter.ai", uris[0].Host);

            // And with an explicit self-hosted override, both roles follow their own setting.
            var overridden = new OrcaRouterOptions
            {
                AuthBaseUrl = "https://auth.internal.test",
                ApiBaseUrl = "https://relay.internal.test/v1"
            };
            var overriddenStore = new OrcaCredentialStore(overridden, new InMemoryPersistence());
            overriddenStore.Save(new OrcaCredential(FakeKey, OrcaCredentialSource.ApiKey));

            var recorded = new List<Uri>();
            var overriddenClient = new OrcaRouterChatClient(
                new FixedFactory(new StubHandler(request =>
                {
                    recorded.Add(request.RequestUri);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            "{\"model\":\"orcarouter/auto\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"ok\"}}]}")
                    };
                })),
                Microsoft.Extensions.Options.Options.Create(overridden),
                overriddenStore);

            await overriddenClient.CompleteAsync(new ChatCompletionRequest
            {
                Messages = new List<ChatMessage> { ChatMessage.User("hi") }
            });

            Assert.AreEqual("relay.internal.test", recorded[0].Host,
                "an explicit API override must be honoured for inference");
            Assert.AreNotEqual("auth.internal.test", recorded[0].Host,
                "inference must not follow the auth override");
        }

        // ---------------------------------------------------------------- helpers

        private sealed class InMemoryPersistence : IOrcaCredentialPersistence
        {
            private OrcaPersistedCredential _credential;

            public string Raw { get; set; }

            public OrcaPersistedCredential Read()
            {
                if (Raw != null)
                {
                    try
                    {
                        return JsonSerializer.Deserialize<OrcaPersistedCredential>(Raw);
                    }
                    catch (JsonException)
                    {
                        return null;
                    }
                }

                return _credential;
            }

            public void Write(OrcaPersistedCredential credential) => _credential = credential;

            public void Delete() => _credential = null;
        }

        private sealed class FixedFactory : IHttpClientFactory
        {
            private readonly HttpMessageHandler _handler;

            public FixedFactory(HttpMessageHandler handler) => _handler = handler;

            public HttpClient CreateClient(string name) => new HttpClient(_handler, disposeHandler: false);
        }

        internal sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(_responder(request));
        }
    }
}
