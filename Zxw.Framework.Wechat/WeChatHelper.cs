using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Senparc.Weixin;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Entities;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.Wechat
{
    public class WeChatHelper
    {
        /// <summary>
        /// 微信APPID
        /// </summary>
        public static string WeChatAppId => ConfigHelper.GetConfigurationValue("WeChat:AppId");
        /// <summary>
        /// 微信AppSecret
        /// </summary>
        public static string WeChatAppSecret => ConfigHelper.GetConfigurationValue("WeChat:AppSecret");
        /// <summary>
        /// Token
        /// </summary>
        public static string WeChatToken => ConfigHelper.GetConfigurationValue("WeChat:Token");
        /// <summary>
        /// State
        /// </summary>
        public static string WeChatAppState => ConfigHelper.GetConfigurationValue("WeChat:AppState");
        /// <summary>
        /// EncodingAESKey
        /// </summary>
        public static string WeChatEncodingAesKey => ConfigHelper.GetConfigurationValue("WeChat:EncodingAESKey");
        /// <summary>
        /// 网站域名
        /// </summary>
        public static string WebsiteDomain => ConfigHelper.GetConfigurationValue("WebsiteDomain");
        /// <summary>
        /// 用户绑定页面URL
        /// </summary>
        public static string UserBindingUrl => ConfigHelper.GetConfigurationValue("UserBindingUrl");
        /// <summary>
        /// 关注公众号页面URL
        /// </summary>
        public static string SubscribeUrl => ConfigHelper.GetConfigurationValue("SubscribeUrl");

        /// <summary>
        /// 获取OpenID对应的Session Key
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetOpenId(HttpContext context)
        {
            var type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            var request = context.Request;
            var response = context.Response;
            var openId = string.Empty;
#if DEBUG
            openId = "";
#endif
            if (!string.IsNullOrEmpty(openId))
            {
                return openId;
            }

            var code = request.Query["code"];
            var state = request.Query["state"];
            var authUrl = OAuthApi.GetAuthorizeUrl(WeChatAppId,
                WebsiteDomain + request.Path.Value,
                WeChatAppState,
                OAuthScope.snsapi_base);
            if (!string.IsNullOrEmpty(code))
            {
                if (state != WeChatAppState)
                {
                    response.WriteAsync("<h1>验证失败！请从正规途径进入！</h1>");
                    return null;
                }

                //通过，用code换取access_token
                try
                {
                    var token = OAuthApi.GetAccessToken(WeChatAppId, WeChatAppSecret, code);
                    Log4NetHelper.WriteInfo(type, $"toekn：{JsonConvert.SerializeObject(token)}");
                    if (token.errcode != ReturnCode.请求成功)
                    {
                        throw new Exception(JsonConvert.SerializeObject(token));
                    }
                    openId = token.openid;

                    return openId;
                }
                catch (ErrorJsonResultException e)
                {
                    Log4NetHelper.WriteError(type, e);
                    response.Redirect(authUrl, true);
                    return null;
                }
            }
            //Log4netHelper.WriteInfo(type, "没取到code");
            response.Redirect(authUrl, true);
            return null;
        }

        /// <summary>
        /// 根据OpenId获取用户信息
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public static WeixinUserInfoResult GetWechatUserInfo(string openId)
        {
            var type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            try
            {
                var user = CommonApi.GetUserInfo(WeChatAppId, openId);
                return user;
            }
            catch (Exception)
            {
                Log4NetHelper.WriteInfo(type, "没取到code");
                throw;
            }
        }

        /// <summary>
        /// 判断用户是否关注本公众号
        /// </summary>
        /// <param name="openId"></param>
        /// <returns>
        /// <value>true:已关注</value>
        /// <value>false:未关注</value>
        /// </returns>
        public static bool IsSubscribe(string openId)
        {
            return GetWechatUserInfo(openId).subscribe != 0;
        }
    }
}
