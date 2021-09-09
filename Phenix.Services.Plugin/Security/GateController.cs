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
    [Route(WebApiConfig.SecurityGatePath)]
    [ApiController]
    public sealed class GateController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        // phAjax.checkIn()
        /// <summary>
        /// 登记(获取动态口令)
        /// </summary>
        /// <param name="companyName">公司名</param>
        /// <param name="userName">登录名</param>
        /// <returns>返回信息</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<string> CheckIn(string companyName, string userName)
        {
            if (String.IsNullOrEmpty(companyName))
                throw new ArgumentNullException(nameof(companyName), "公司名不允许为空!");
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName), "登录名不允许为空!");

            IIdentity identity = Principal.FetchIdentity(companyName, userName, Request.GetAcceptLanguage(), null);
            return await ClusterClient.Default.GetGrain<IUserGrain>(identity.PrimaryKey).CheckIn(Request.GetRemoteAddress());
        }
        
        // phAjax.logon()
        // phAjax.changePassword()
        /// <summary>
        /// 登录
        /// </summary>
        [Authorize]
        [HttpPut]
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