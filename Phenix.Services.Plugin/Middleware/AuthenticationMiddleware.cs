using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;
using Phenix.Core.Security;
using Phenix.Services.Contract;

namespace Phenix.Services.Plugin.Middleware
{
    /// <summary>
    /// 身份验证中间件
    /// 
    /// 与客户端接口 phenix7.js 一起实现用户的身份验证功能
    /// 你也可以开发自己的客户端接口（比如桌面端、APP应用端），仅需在报文上添加身份验证 Header，格式为 Phenix-Authorization=[公司名],[登录名],[会话签名]
    ///
    /// 登录口令/动态口令应该通过第三方渠道（邮箱或短信）推送给到用户，由用户输入到系统提供的客户端登录界面上，用于加密时间戳生成报文的签名
    /// 用户登录成功后，客户端程序要将二次MD5登录口令/动态口令缓存在本地，以便每次向服务端发起 call 时都能为报文添加上 Phenix-Authorization
    /// 未添加上 Phenix-Authorization 的报文会被当作是匿名访问，仅允许访问带[AllowAnonymous]标签或不打[Authorize]标签的 Controller/Action
    ///
    /// 验证失败的话 context.Response.StatusCode = 401 Unauthorized，失败详情见报文体
    /// 验证成功的话 Phenix.Core.Security.Principal.CurrentIdentity.IsAuthenticated = true 且 context.User 会被赋值为 new ClaimsPrincipal(Principal.CurrentIdentity)
    /// </summary>
    public sealed class AuthenticationMiddleware
    {
        /// <summary>
        /// 身份验证中间件
        /// </summary>
        /// <param name="next">下一个中间件</param>
        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #region 属性

        private readonly RequestDelegate _next;

        private const string MethodOverrideHeaderName = "X-HTTP-Method-Override";

        private const string AuthorizationHeaderName = "Phenix-Authorization";

        #endregion

        #region 方法

        /// <summary>
        /// 动态执行
        /// </summary>
        /// <param name="context">上下文</param>
        public async Task InvokeAsync(HttpContext context)
        {
            string method = context.Request.Headers[MethodOverrideHeaderName].FirstOrDefault();
            if (!String.IsNullOrEmpty(method))
                context.Request.Method = method;

            string token = context.Request.Headers[AuthorizationHeaderName].FirstOrDefault();
            if (String.IsNullOrEmpty(token)) // for SignalR WebSocket
                token = context.Request.Query["access_token"];
            if (String.IsNullOrEmpty(token)) // for SignalR
            {
                token = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!String.IsNullOrEmpty(token) && token.IndexOf("Bearer ", StringComparison.Ordinal) == 0)
                    token = token.Substring(7);
            }

            Principal.CurrentIdentity = null;
            if (!String.IsNullOrEmpty(token))
            {
                //身份验证token: [公司名],[登录名],[会话签名]
                string[] strings = token.Split(Standards.ValueSeparator);
                if (strings.Length != 3)
                    throw new InvalidOperationException(String.Format("身份验证token格式错误：{0}", token));
                string companyName = Uri.UnescapeDataString(strings[0]);
                string userName = Uri.UnescapeDataString(strings[1]);
                string signature = strings[2];
                IIdentity identity = Principal.FetchIdentity(companyName, userName, context.Request.GetAcceptLanguage(), null);
                if (String.Compare(context.Request.Path, WebApiConfig.ApiSecurityGatePath, StringComparison.OrdinalIgnoreCase) == 0 && context.Request.Method == HttpMethod.Post.Method)
                {
                    await identity.Logon(signature, await context.Request.ReadBodyAsStringAsync(), context.Request.GetRemoteAddress());
                    Principal.CurrentIdentity = identity;
                    context.User = new ClaimsPrincipal(identity);
                }
                else
                {
                    await identity.Verify(signature, context.Request.GetRemoteAddress());
                    Principal.CurrentIdentity = identity;
                    context.User = new ClaimsPrincipal(identity);
                }
            }

            await _next.Invoke(context);
        }

        #endregion
    }
}