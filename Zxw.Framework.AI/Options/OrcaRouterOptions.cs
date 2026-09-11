using System;
using System.Collections.Generic;
using Zxw.Framework.AI.Credentials;

namespace Zxw.Framework.AI.Options
{
    /// <summary>
    /// OrcaRouter（OpenAI 兼容网关）配置。
    /// </summary>
    public class OrcaRouterOptions
    {
        public const string DefaultBaseUrl = Credentials.OrcaAuthEndpoints.DefaultApiBaseUrl;
        public const string DefaultAuthBaseUrl = Credentials.OrcaAuthEndpoints.DefaultAuthBaseUrl;
        public const string DefaultModelId = "orcarouter/auto";
        public const string HttpClientName = "OrcaRouter";
        public const string ApiKeyEnvironmentVariable = "ORCAROUTER_API_KEY";
        public const string ReferralLinkBaseUrl = "https://www.orcarouter.ai/ref";
        public const string DefaultReferralCode = "ref_4efd338f7db91cf2aa1d";

        /// <summary>
        /// 网关 Base URL，默认 https://api.orcarouter.ai/v1
        /// </summary>
        public string BaseUrl { get; set; } = DefaultBaseUrl;

        /// <summary>
        /// Inference + model-catalog base URL override. Explicit value wins; when unset the shared
        /// <c>ORCA_BASE_URL</c> fallback is used, then <see cref="DefaultBaseUrl"/>. Never derived
        /// from <see cref="AuthBaseUrl"/>.
        /// </summary>
        public string ApiBaseUrl { get; set; }

        /// <summary>
        /// Authentication origin override, e.g. <c>https://www.orcarouter.ai</c>. Explicit value
        /// wins; when unset the shared <c>ORCA_BASE_URL</c> fallback is used, then
        /// <see cref="DefaultAuthBaseUrl"/>. Never derived from <see cref="ApiBaseUrl"/>.
        /// </summary>
        public string AuthBaseUrl { get; set; }

        /// <summary>
        /// Shared self-hosted base URL, used for <b>both</b> roles when a deployment serves auth
        /// and inference from one origin. It is a fallback only: <see cref="AuthBaseUrl"/> and
        /// <see cref="ApiBaseUrl"/> each take precedence over it for their own role. Equivalent to
        /// the <c>ORCA_BASE_URL</c> environment variable.
        /// </summary>
        public string SharedBaseUrl { get; set; }

        /// <summary>
        /// Resolve the authentication origin: <see cref="AuthBaseUrl"/>, then
        /// <c>ORCA_AUTH_BASE_URL</c>, then the shared base, then the public default.
        /// </summary>
        public string ResolveAuthBaseUrl() =>
            OrcaAuthEndpoints.ResolveAuthBaseUrl(AuthBaseUrl, SharedBaseUrl);

        /// <summary>
        /// Resolve the inference and model-catalog origin: <see cref="ApiBaseUrl"/>, then the
        /// legacy <see cref="BaseUrl"/> when it was changed from its default, then
        /// <c>ORCA_API_BASE_URL</c>, then the shared base, then the public default.
        /// </summary>
        public string ResolveApiBaseUrl()
        {
            var explicitApi = !string.IsNullOrWhiteSpace(ApiBaseUrl)
                ? ApiBaseUrl
                : (!string.IsNullOrWhiteSpace(BaseUrl) &&
                   !string.Equals(BaseUrl, DefaultBaseUrl, StringComparison.OrdinalIgnoreCase)
                    ? BaseUrl
                    : null);

            return OrcaAuthEndpoints.ResolveApiBaseUrl(explicitApi, SharedBaseUrl);
        }

        /// <summary>
        /// API Key（sk-orca-...）。为空时回退读取环境变量 ORCAROUTER_API_KEY。
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// 默认模型，推荐 orcarouter/auto。
        /// </summary>
        public string DefaultModel { get; set; } = DefaultModelId;

        /// <summary>
        /// 默认 fallback 模型链（写入请求体 models 字段）。
        /// </summary>
        public IList<string> FallbackModels { get; set; } = new List<string>();

        /// <summary>
        /// HTTP 超时，默认 100 秒。
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// OrcaRouter OSS 推广码（合作伙伴中心「你的推广码」），用于生成推广链接与 API 归因。
        /// </summary>
        public string ReferralCode { get; private set; } = DefaultReferralCode;

        /// <summary>
        /// HTTP Referer 归因。未设置时由 <see cref="ReferralCode"/> 自动生成推广链接。
        /// </summary>
        public string HttpReferer { get; set; }

        /// <summary>
        /// 应用标题（X-Title），用于 OrcaRouter 控制台流量归因。
        /// </summary>
        public string AppTitle { get; } = "Zxw.Framework.NetCore";

        public string ResolveApiKey()
        {
            if (!string.IsNullOrWhiteSpace(ApiKey))
                return ApiKey.Trim();

            return Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable)?.Trim();
        }

        /// <summary>
        /// 生成完整推广链接，例如 https://www.orcarouter.ai/ref/ref_xxx
        /// </summary>
        public string ResolveReferralLink()
        {
            if (string.IsNullOrWhiteSpace(ReferralCode))
                return null;

            return $"{ReferralLinkBaseUrl.TrimEnd('/')}/{ReferralCode.Trim()}";
        }

        /// <summary>
        /// 解析 HTTP-Referer：显式 HttpReferer 优先，否则回退为推广链接。
        /// </summary>
        public string ResolveHttpReferer()
        {
            if (!string.IsNullOrWhiteSpace(HttpReferer))
                return HttpReferer.Trim();

            return ResolveReferralLink();
        }
    }
}
