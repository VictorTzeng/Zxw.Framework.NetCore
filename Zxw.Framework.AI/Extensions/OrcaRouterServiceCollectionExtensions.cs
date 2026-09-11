using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Zxw.Framework.AI.Abstractions;
using Zxw.Framework.AI.Clients;
using Zxw.Framework.AI.Credentials;
using Zxw.Framework.AI.Models;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.AI.Extensions
{
    public static class OrcaRouterServiceCollectionExtensions
    {
        /// <summary>Named client used for the relay (inference and model catalog).</summary>
        public const string RelayHttpClientName = OrcaRouterOptions.HttpClientName + ".Relay";

        /// <summary>Named client used for the auth origin (PKCE code exchange only).</summary>
        public const string AuthHttpClientName = OrcaRouterOptions.HttpClientName + ".Auth";

        /// <summary>
        /// 注册 OrcaRouter 为默认 <see cref="IChatClient"/>（内置 LLM 网关选项）。
        /// <para>
        /// Registers the credential seam and <b>both</b> authentication choices: pasting an
        /// <c>sk-orca-…</c> key (<see cref="OrcaApiKeyCredentialProvider"/>) and authorizing with an
        /// OrcaRouter account over OAuth 2.0 + PKCE (<see cref="OrcaPkceCredentialProvider"/>).
        /// </para>
        /// </summary>
        public static IServiceCollection AddOrcaRouter(
            this IServiceCollection services,
            Action<OrcaRouterOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions<OrcaRouterOptions>();
            if (configure != null)
                services.Configure(configure);

            // Relay client: inference and model discovery, both on the API origin.
            services.AddHttpClient(RelayHttpClientName)
                .ConfigureHttpClient((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value;
                    var baseUrl = options
                        .ResolveApiBaseUrl()
                        .TrimEnd('/') + "/";
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = options.Timeout <= TimeSpan.Zero
                        ? TimeSpan.FromSeconds(100)
                        : options.Timeout;
                });

            // Auth client: the code exchange, on the auth origin. Separate from the relay client so
            // the two origins can never be confused for one another.
            services.AddHttpClient(AuthHttpClientName)
                .ConfigureHttpClient((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value;
                    var baseUrl = OrcaAuthEndpoints.ResolveAuthBaseUrl(options.AuthBaseUrl, options.SharedBaseUrl);
                    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                    client.Timeout = options.Timeout <= TimeSpan.Zero
                        ? TimeSpan.FromSeconds(100)
                        : options.Timeout;
                });

            // Keep the original named client resolvable for hosts that already refer to it.
            services.AddHttpClient(OrcaRouterOptions.HttpClientName)
                .ConfigureHttpClient((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value;
                    var baseUrl = options
                        .ResolveApiBaseUrl()
                        .TrimEnd('/') + "/";
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = options.Timeout <= TimeSpan.Zero
                        ? TimeSpan.FromSeconds(100)
                        : options.Timeout;
                });

            services.TryAddSingleton<IOrcaCredentialPersistence>(_ =>
                new OrcaUserProfileCredentialPersistence());

            services.TryAddSingleton<IOrcaCredentialStore>(sp =>
                new OrcaCredentialStore(
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value,
                    sp.GetService<IOrcaCredentialPersistence>()));

            services.TryAddSingleton<IChatClient, OrcaRouterChatClient>();

            // The two authentication choices, and the catalog reader. All three sit on the same seam.
            services.TryAddSingleton<OrcaApiKeyCredentialProvider>(sp =>
                new OrcaApiKeyCredentialProvider(
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value,
                    sp.GetRequiredService<IOrcaCredentialStore>()));

            services.TryAddSingleton<OrcaPkceCredentialProvider>(sp =>
                new OrcaPkceCredentialProvider(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(AuthHttpClientName),
                    sp.GetRequiredService<IOrcaCredentialStore>(),
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value,
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value.AuthBaseUrl));

            services.TryAddSingleton<OrcaModelCatalogClient>(sp =>
                new OrcaModelCatalogClient(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(RelayHttpClientName),
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value));

            services.TryAddSingleton<OrcaModelSelector>();

            return services;
        }

        /// <summary>
        /// Register only the credential seam and the two authentication choices, not a chat client.
        /// Useful for hosts that only read the catalog or manage credentials, and for tests.
        /// </summary>
        public static IServiceCollection AddOrcaRouterCredentials(
            this IServiceCollection services,
            Action<OrcaRouterOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions<OrcaRouterOptions>();
            if (configure != null)
                services.Configure(configure);

            services.AddHttpClient(AuthHttpClientName);
            services.AddHttpClient(RelayHttpClientName);

            services.TryAddSingleton<IOrcaCredentialPersistence>(_ =>
                new OrcaUserProfileCredentialPersistence());

            services.TryAddSingleton<IOrcaCredentialStore>(sp =>
                new OrcaCredentialStore(
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value,
                    sp.GetService<IOrcaCredentialPersistence>()));

            services.TryAddSingleton<OrcaApiKeyCredentialProvider>(sp =>
                new OrcaApiKeyCredentialProvider(
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value,
                    sp.GetRequiredService<IOrcaCredentialStore>()));

            services.TryAddSingleton<OrcaPkceCredentialProvider>(sp =>
                new OrcaPkceCredentialProvider(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(AuthHttpClientName),
                    sp.GetRequiredService<IOrcaCredentialStore>(),
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value,
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value.AuthBaseUrl));

            services.TryAddSingleton<OrcaModelCatalogClient>(sp =>
                new OrcaModelCatalogClient(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(RelayHttpClientName),
                    sp.GetRequiredService<IOptions<OrcaRouterOptions>>().Value));

            services.TryAddSingleton<OrcaModelSelector>();

            return services;
        }
    }
}
