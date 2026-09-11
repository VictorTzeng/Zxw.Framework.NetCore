using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zxw.Framework.AI.Abstractions;
using Zxw.Framework.AI.Clients;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.UnitTest
{
    [TestClass]
    public class OrcaRouterChatClientTests
    {
        private string _ambientApiKey;

        [TestInitialize]
        public void SuspendAmbientCredential() =>
            _ambientApiKey = OrcaTestEnvironment.SuspendAmbientApiKey();

        [TestCleanup]
        public void RestoreAmbientCredential() =>
            OrcaTestEnvironment.RestoreAmbientApiKey(_ambientApiKey);

        [TestMethod]
        public async Task CompleteAsync_SendsOpenAiCompatibleRequest_AndParsesResponse()
        {
            HttpRequestMessage captured = null;
            string capturedBody = null;
            var handler = new StubHttpMessageHandler(async request =>
            {
                captured = request;
                capturedBody = request.Content == null
                    ? null
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                var json = @"{
                  ""id"": ""chatcmpl-1"",
                  ""model"": ""orcarouter/auto"",
                  ""choices"": [{
                    ""index"": 0,
                    ""message"": { ""role"": ""assistant"", ""content"": ""hello"" },
                    ""finish_reason"": ""stop""
                  }],
                  ""usage"": { ""prompt_tokens"": 3, ""completion_tokens"": 1, ""total_tokens"": 4 }
                }";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var client = BuildChatClient(handler, new OrcaRouterOptions
            {
                ApiKey = "sk-orca-test",
                HttpReferer = new OrcaRouterOptions().ResolveReferralLink(),
                FallbackModels = new List<string> { "openai/gpt-4o-mini" }
            });

            var response = await client.CompleteAsync(new ChatCompletionRequest
            {
                Messages = new List<ChatMessage> { ChatMessage.User("hi") }
            });

            Assert.AreEqual("hello", response.GetContent());
            Assert.AreEqual("orcarouter/auto", response.Model);
            Assert.IsNotNull(captured);
            Assert.AreEqual(HttpMethod.Post, captured.Method);
            Assert.IsTrue(captured.RequestUri.ToString().EndsWith("/chat/completions", StringComparison.Ordinal));
            Assert.AreEqual("Bearer", captured.Headers.Authorization.Scheme);
            Assert.AreEqual("sk-orca-test", captured.Headers.Authorization.Parameter);
            Assert.IsTrue(captured.Headers.Contains("HTTP-Referer"));
            Assert.IsTrue(captured.Headers.Contains("X-Title"));
            Assert.AreEqual(
                new OrcaRouterOptions().ResolveReferralLink(),
                captured.Headers.GetValues("HTTP-Referer").First());

            StringAssert.Contains(capturedBody, "\"model\":\"orcarouter/auto\"");
            StringAssert.Contains(capturedBody, "\"models\"");
            StringAssert.Contains(capturedBody, "openai/gpt-4o-mini");
            StringAssert.Contains(capturedBody, "\"stream\":false");
        }

        [TestMethod]
        public async Task CompleteStreamingAsync_YieldsDeltaContent()
        {
            var handler = new StubHttpMessageHandler(_ =>
            {
                var sse =
                    "data: {\"choices\":[{\"delta\":{\"content\":\"Hel\"}}]}\n\n" +
                    "data: {\"choices\":[{\"delta\":{\"content\":\"lo\"}}]}\n\n" +
                    "data: [DONE]\n\n";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(sse, Encoding.UTF8, "text/event-stream")
                });
            });

            var client = BuildChatClient(handler, new OrcaRouterOptions { ApiKey = "sk-orca-test" });
            var parts = new List<string>();
            await foreach (var part in client.CompleteStreamingAsync(new ChatCompletionRequest
            {
                Messages = new List<ChatMessage> { ChatMessage.User("hi") }
            }))
            {
                parts.Add(part);
            }

            CollectionAssert.AreEqual(new[] { "Hel", "lo" }, parts);
        }

        [TestMethod]
        public async Task CompleteAsync_WithoutApiKey_Throws()
        {
            var handler = new StubHttpMessageHandler(_ =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            var client = BuildChatClient(handler, new OrcaRouterOptions { ApiKey = null });

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                client.CompleteAsync(new ChatCompletionRequest
                {
                    Messages = new List<ChatMessage> { ChatMessage.User("hi") }
                }));
        }

        private static IChatClient BuildChatClient(HttpMessageHandler handler, OrcaRouterOptions options)
        {
            return new OrcaRouterChatClient(
                new FixedHttpClientFactory(handler),
                Options.Create(options));
        }

        private sealed class FixedHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpMessageHandler _handler;

            public FixedHttpClientFactory(HttpMessageHandler handler)
            {
                _handler = handler;
            }

            public HttpClient CreateClient(string name)
            {
                return new HttpClient(_handler, disposeHandler: false);
            }
        }

        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _responder;

            public StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
            {
                _responder = responder;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return _responder(request);
            }
        }
    }
}
