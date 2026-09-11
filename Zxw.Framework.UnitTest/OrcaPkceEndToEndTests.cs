using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zxw.Framework.AI.Credentials;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.UnitTest
{
    /// <summary>
    /// Drives the real PKCE connect adapter against a real local authorization server, covering
    /// authorize → out-of-band code → exchange → persist in full. Only fake codes and fake keys are
    /// used; no real consent is simulated or bypassed.
    /// </summary>
    [TestClass]
    public class OrcaPkceEndToEndTests
    {
        private const string FakeIssuedKey = "sk-orca-test-00000000000000000000000000e2e";

        private string _ambientApiKey;

        [TestInitialize]
        public void SuspendAmbientCredential() =>
            _ambientApiKey = OrcaTestEnvironment.SuspendAmbientApiKey();

        [TestCleanup]
        public void RestoreAmbientCredential() =>
            OrcaTestEnvironment.RestoreAmbientApiKey(_ambientApiKey);

        [TestMethod]
        public async Task ConnectFlow_AuthorizeThenExchangeThenPersist()
        {
            using var server = new FakeAuthServer();
            var persistence = new InMemoryPersistence();
            var options = new OrcaRouterOptions { AuthBaseUrl = server.BaseUrl };
            var store = new OrcaCredentialStore(options, persistence);
            var provider = new OrcaPkceCredentialProvider(
                new HttpClient(), store, options, server.BaseUrl);

            // 1. Begin: a fresh verifier/challenge/state, out-of-band, S256.
            var session = provider.Start(appName: "Zxw.Framework.NetCore");
            var authorizeUri = new Uri(session.AuthorizeUrl);
            Assert.AreEqual(server.BaseUrl, authorizeUri.GetLeftPart(UriPartial.Authority));
            Assert.AreEqual("/auth", authorizeUri.AbsolutePath);
            Assert.IsTrue(session.AuthorizeUrl.Contains("callback_url=oob"));
            Assert.IsTrue(session.AuthorizeUrl.Contains("code_challenge_method=S256"));
            Assert.IsFalse(session.AuthorizeUrl.Contains("code_verifier"));

            // 2. The consent screen shows a code; the user enters it.
            server.IssueKey = FakeIssuedKey;
            server.IssueScope = "api";

            var result = await session.CompleteAsync("fake-oob-code");

            // 3. The key was persisted through the project's credential store.
            Assert.IsTrue(result.Succeeded, result.Status.Message);
            Assert.AreEqual(OrcaCredentialSource.Pkce, result.Credential.Source);
            Assert.AreEqual(FakeIssuedKey, result.Credential.ApiKey);
            Assert.AreEqual("api", result.Credential.Scope);
            Assert.AreEqual("user-42", result.Credential.AccountId);

            var reloaded = new OrcaCredentialStore(options, persistence).Load();
            Assert.IsNotNull(reloaded, "the key must survive a restart");
            Assert.AreEqual(FakeIssuedKey, reloaded.ApiKey);
            Assert.AreEqual(OrcaCredentialSource.Pkce, reloaded.Source);

            // 4. The exchange went to the auth origin's exchange path, with the right body, and the
            //    verifier was presented there — not on the authorize URL.
            Assert.AreEqual("/api/v1/auth/keys", server.LastExchangePath);
            Assert.IsNotNull(server.LastExchangeBody);
            Assert.AreEqual("fake-oob-code", server.LastExchangeBody.Code);
            Assert.AreEqual("S256", server.LastExchangeBody.CodeChallengeMethod);
            Assert.IsFalse(string.IsNullOrEmpty(server.LastExchangeBody.CodeVerifier));

            // The presented verifier hashes to the challenge that was sent on the authorize URL.
            var challenge = ExtractQuery(session.AuthorizeUrl, "code_challenge");
            Assert.AreEqual(challenge, OrcaPkce.CreateChallenge(server.LastExchangeBody.CodeVerifier));

            // 5. The authorization URL the UI would hand the user carries no credential material.
            Assert.IsFalse(session.AuthorizeUrl.Contains(FakeIssuedKey));
            Assert.IsFalse(session.AuthorizeUrl.Contains(server.LastExchangeBody.CodeVerifier));
        }

        [TestMethod]
        public async Task ConnectFlow_ReusesStoredKey_InsteadOfAuthorizingAgain()
        {
            using var server = new FakeAuthServer();
            var persistence = new InMemoryPersistence();
            var options = new OrcaRouterOptions { AuthBaseUrl = server.BaseUrl };
            var store = new OrcaCredentialStore(options, persistence);

            var first = new OrcaPkceCredentialProvider(new HttpClient(), store, options, server.BaseUrl);
            server.IssueKey = FakeIssuedKey;
            Assert.IsTrue((await first.Start().CompleteAsync("code-one")).Succeeded);

            // A fresh provider over the same store is what a relaunch looks like. It must reuse the
            // stored key: OrcaRouter caps PKCE-issued keys at ten per user per 24 hours.
            var second = new OrcaPkceCredentialProvider(new HttpClient(), store, options, server.BaseUrl);
            var reused = second.Acquire();

            Assert.IsTrue(reused.Succeeded);
            Assert.AreEqual(FakeIssuedKey, reused.Credential.ApiKey);
            Assert.AreEqual(1, server.ExchangeAttempts, "a stored key must not trigger a second authorization");
        }

        [TestMethod]
        public async Task ConnectFlow_Denial_EndsCleanlyWithoutStoringAnything()
        {
            using var server = new FakeAuthServer { FailWithStatus = 403 };
            var persistence = new InMemoryPersistence();
            var options = new OrcaRouterOptions { AuthBaseUrl = server.BaseUrl };
            var store = new OrcaCredentialStore(options, persistence);
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), store, options, server.BaseUrl);

            var result = await provider.Start().CompleteAsync("denied-code");

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains(result.Status.Message, "single-use");
            Assert.IsNull(persistence.Read(), "nothing may be stored when authorization is refused");
        }

        [TestMethod]
        public async Task ConnectFlow_CodeReuse_IsReportedAsTerminal()
        {
            using var server = new FakeAuthServer { SingleUse = true };
            server.IssueKey = FakeIssuedKey;
            var options = new OrcaRouterOptions { AuthBaseUrl = server.BaseUrl };
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), store, options, server.BaseUrl);

            // First redemption succeeds.
            Assert.IsTrue((await provider.Start().CompleteAsync("one-shot")).Succeeded);

            // The same code presented again is refused; the server answers 403.
            var second = await provider.Start().CompleteAsync("one-shot");
            Assert.IsFalse(second.Succeeded);
            StringAssert.Contains(second.Status.Message, "403");
        }

        [TestMethod]
        public async Task ConnectFlow_RateLimited_ExplainsTheTwentyFourHourKeyCap()
        {
            using var server = new FakeAuthServer { FailWithStatus = 429 };
            var options = new OrcaRouterOptions { AuthBaseUrl = server.BaseUrl };
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), store, options, server.BaseUrl);

            var result = await provider.Start().CompleteAsync("any-code");

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains(result.Status.Message, "24 hours");
        }

        [TestMethod]
        public async Task ConnectFlow_ScopeDowngrade_IsRefusedAndNothingIsStored()
        {
            using var server = new FakeAuthServer { IssueKey = FakeIssuedKey, IssueScope = "read-only" };
            var persistence = new InMemoryPersistence();
            var options = new OrcaRouterOptions { AuthBaseUrl = server.BaseUrl };
            var store = new OrcaCredentialStore(options, persistence);
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), store, options, server.BaseUrl);

            var result = await provider.Start(scope: "api").CompleteAsync("scoped-code");

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains(result.Status.Message, "does not cover");
            Assert.IsNull(persistence.Read(),
                "a key granting less than the requested scope must not be silently adopted");
        }

        [TestMethod]
        public async Task ConnectFlow_UnreachableAuthServer_FailsWithAnActionableMessage()
        {
            // A port nothing is listening on.
            var dead = "http://127.0.0.1:" + FreePort();
            var options = new OrcaRouterOptions { AuthBaseUrl = dead };
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), store, options, dead);

            var result = await provider.Start().CompleteAsync("any-code");

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains(result.Status.Message, "Could not reach");
        }

        [TestMethod]
        public async Task ConnectFlow_VerifierNeverAppearsInAnySurfacedError()
        {
            // The server echoes the submitted verifier back in its error body; the adapter must
            // scrub it before the message reaches the user or a log.
            using var server = new FakeAuthServer { FailWithStatus = 500, EchoVerifier = true };
            var options = new OrcaRouterOptions { AuthBaseUrl = server.BaseUrl };
            var store = new OrcaCredentialStore(options, new InMemoryPersistence());
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), store, options, server.BaseUrl);

            var session = provider.Start();
            var verifierPresented = string.Empty;
            server.OnExchange = body => verifierPresented = body.CodeVerifier;

            var result = await session.CompleteAsync("any-code");

            Assert.IsFalse(result.Succeeded);
            Assert.IsFalse(string.IsNullOrEmpty(verifierPresented));
            Assert.IsFalse(result.Status.Message.Contains(verifierPresented),
                "the verifier must not survive into a user-facing error");
            Assert.IsFalse(session.AuthorizeUrl.Contains(verifierPresented));
        }

        [TestMethod]
        public async Task ConnectFlow_RejectionsAfterSuccess_DoNotDisturbTheStoredKey()
        {
            using var server = new FakeAuthServer { FailWithStatus = 403 };
            var persistence = new InMemoryPersistence();
            var options = new OrcaRouterOptions { AuthBaseUrl = server.BaseUrl };
            var store = new OrcaCredentialStore(options, persistence);
            var provider = new OrcaPkceCredentialProvider(new HttpClient(), store, options, server.BaseUrl);

            // A good key is already stored from an earlier session.
            store.Save(new OrcaCredential(FakeIssuedKey, OrcaCredentialSource.Pkce, "user-42", "api"));

            var failed = await provider.Start().CompleteAsync("bad-code");
            Assert.IsFalse(failed.Succeeded);

            // A failed attempt must not delete a working credential.
            Assert.IsNotNull(persistence.Read(), "the previous credential must survive a failed attempt");
            Assert.AreEqual(FakeIssuedKey, store.Load().ApiKey);
        }

        private static string ExtractQuery(string url, string key)
        {
            var query = new Uri(url).Query.TrimStart('?');
            foreach (var pair in query.Split('&'))
            {
                var parts = pair.Split(new[] { '=' }, 2);
                if (parts.Length == 2 && parts[0] == key)
                    return Uri.UnescapeDataString(parts[1]);
            }

            return null;
        }

        private static int FreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private sealed class InMemoryPersistence : IOrcaCredentialPersistence
        {
            private OrcaPersistedCredential _credential;

            public OrcaPersistedCredential Read() => _credential;

            public void Write(OrcaPersistedCredential credential) => _credential = credential;

            public void Delete() => _credential = null;
        }

        internal sealed class ExchangeBody
        {
            public string Code { get; set; }
            public string CodeVerifier { get; set; }
            public string CodeChallengeMethod { get; set; }
        }

        /// <summary>
        /// A real loopback authorization server that speaks the documented exchange contract:
        /// POST /api/v1/auth/keys with the JSON body, answering {"key","user_id","scope"}.
        /// </summary>
        private sealed class FakeAuthServer : IDisposable
        {
            private readonly HttpListener _listener;
            private readonly Task _loop;
            private readonly HashSet<string> _redeemed = new HashSet<string>(StringComparer.Ordinal);

            public FakeAuthServer()
            {
                var port = FreePort();
                BaseUrl = $"http://127.0.0.1:{port}";
                _listener = new HttpListener();
                _listener.Prefixes.Add(BaseUrl + "/");
                _listener.Start();
                _loop = Task.Run(LoopAsync);
            }

            public string BaseUrl { get; }

            public string IssueKey { get; set; } = "sk-orca-test-00000000000000000000000000e2e";

            public string IssueScope { get; set; } = "api";

            public int? FailWithStatus { get; set; }

            public bool EchoVerifier { get; set; }

            public bool SingleUse { get; set; }

            public string LastExchangePath { get; private set; }

            public ExchangeBody LastExchangeBody { get; private set; }

            public int ExchangeAttempts { get; private set; }

            public Action<ExchangeBody> OnExchange { get; set; }

            private async Task LoopAsync()
            {
                while (_listener.IsListening)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = await _listener.GetContextAsync().ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    try
                    {
                        Handle(context);
                    }
                    catch (Exception)
                    {
                        // A malformed request must not take the fake server down.
                    }
                }
            }

            private void Handle(HttpListenerContext context)
            {
                LastExchangePath = context.Request.Url.AbsolutePath;
                ExchangeAttempts++;

                string raw;
                using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                    raw = reader.ReadToEnd();

                var body = new ExchangeBody();
                try
                {
                    using var document = JsonDocument.Parse(raw);
                    if (document.RootElement.TryGetProperty("code", out var code))
                        body.Code = code.GetString();
                    if (document.RootElement.TryGetProperty("code_verifier", out var verifier))
                        body.CodeVerifier = verifier.GetString();
                    if (document.RootElement.TryGetProperty("code_challenge_method", out var method))
                        body.CodeChallengeMethod = method.GetString();
                }
                catch (JsonException)
                {
                    // Left as defaults; the test asserts the parsed values.
                }

                LastExchangeBody = body;
                OnExchange?.Invoke(body);

                var status = FailWithStatus ?? 200;

                // Single-use enforcement mirrors the documented 10-minute, one-shot auth code.
                if (status == 200 && SingleUse && !_redeemed.Add(body.Code ?? string.Empty))
                    status = 403;

                if (status != 200)
                {
                    var message = EchoVerifier
                        ? $"{{\"error\":\"server_error\",\"echoed\":\"{body.CodeVerifier}\"}}"
                        : "{\"error\":\"invalid_request\"}";
                    Write(context, status, message);
                    return;
                }

                Write(context, 200, JsonSerializer.Serialize(new
                {
                    key = IssueKey,
                    user_id = "user-42",
                    scope = IssueScope
                }));
            }

            private static void Write(HttpListenerContext context, int status, string body)
            {
                var bytes = Encoding.UTF8.GetBytes(body);
                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Close();
            }

            public void Dispose()
            {
                try
                {
                    _listener.Stop();
                    _listener.Close();
                }
                catch (Exception)
                {
                    // Nothing to clean up.
                }
            }
        }
    }
}
