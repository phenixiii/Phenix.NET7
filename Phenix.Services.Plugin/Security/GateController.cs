using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Contract;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 系统入口控制器
    /// </summary>
    [Route(WebApiConfig.ApiSecurityGatePath)]
    [ApiController]
    public sealed class GateController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        // phAjax.checkIn()
        /// <summary>
        /// 登记(获取动态口令)/注册(静态口令即登录名)
        /// </summary>
        /// <param name="companyName">公司名</param>
        /// <param name="userName">登录名</param>
        /// <param name="phone">手机(注册时可空)</param>
        /// <param name="eMail">邮箱(注册时可空)</param>
        /// <param name="regAlias">昵称(注册时可空)</param>
        /// <returns>返回信息</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<string> CheckIn(string companyName, string userName, string phone, string eMail, string regAlias)
        {
            if (String.IsNullOrEmpty(companyName))
                throw new ArgumentNullException(nameof(companyName), "公司名不允许为空!");
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName), "登录名不允许为空!");

            IIdentity identity = Principal.FetchIdentity(companyName, userName, Request.GetAcceptLanguage(), null);
            return await ClusterClient.Default.GetGrain<IUserGrain>(identity.PrimaryKey).CheckIn(phone, eMail, regAlias, Request.GetRemoteAddress());
        }

        // phAjax.logon()
        /// <summary>
        /// 登录
        /// </summary>
        [Authorize]
        [HttpPost]
        public void Logon()
        {
            // ignored
        }

        // phAjax.logout()
        /// <summary>
        /// 登出
        /// </summary>
        [Authorize]
        [HttpDelete]
        public async Task Logout()
        {
            await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.PrimaryKey).Logout();
        }

        #endregion
    }
}