using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Services.Plugin.Actor;

namespace Phenix.Services.Plugin.Api.Security
{
    /// <summary>
    /// 系统入口控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityGatePath)]
    [ApiController]
    public sealed class GateController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        // phAjax.checkIn()
        /// <summary>
        /// 登记/注册(获取动态口令)
        /// </summary>
        /// <param name="name">登录名(未注册则自动注册)</param>
        /// <param name="phone">手机(注册用可为空)</param>
        /// <param name="eMail">邮箱(注册用可为空)</param>
        /// <param name="regAlias">注册昵称(注册用可为空)</param>
        /// <returns>返回信息</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<string> CheckIn(string name, string phone, string eMail, string regAlias)
        {
            return await ClusterClient.Default.GetGrain<IUserGrain>(name).CheckIn(phone, eMail, regAlias, Request.GetRemoteAddress());
        }

        // phAjax.logon()
        /// <summary>
        /// 登录
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task Logon()
        {
            await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.Name).Logon(await Request.ReadBodyAsStringAsync(true));
        }

        #endregion
    }
}