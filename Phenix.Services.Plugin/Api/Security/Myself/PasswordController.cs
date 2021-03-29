using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Net.Filters;
using Phenix.Services.Plugin.Actor.Security;

namespace Phenix.Services.Plugin.Api.Security.Myself
{
    /// <summary>
    /// 登录口令控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityMyselfPasswordPath)]
    [ApiController]
    public sealed class PasswordController : Phenix.Core.Net.Api.ControllerBase
    {
        // phAjax.changePassword()
        /// <summary>
        /// 修改登录口令
        /// </summary>
        [Authorize]
        [HttpPut]
        public async Task<bool> Put()
        {
            string[] strings = (await Request.ReadBodyAsStringAsync(true)).Split(Standards.RowSeparator);
            return await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.PrimaryKey).ChangePassword(strings[0], strings[1], Request.GetRemoteAddress(), true);
        }

        /// <summary>
        /// 重置公司用户登录口令
        /// </summary>
        /// <param name="name">登录名</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task Patch(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "登录名不允许为空!");

            await ClusterClient.Default.GetGrain<IUserGrain>(String.Format("{0}{1}{2}", User.Identity.CompanyName, Standards.RowSeparator, name)).ResetPassword();
        }
    }
}