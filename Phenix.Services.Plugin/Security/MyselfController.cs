using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 用户自己控制器
    /// </summary>
    [Route(Phenix.Net.Api.Standards.SecurityMyselfPath)]
    [ApiController]
    public sealed class MyselfController : Phenix.Net.Api.ControllerBase
    {
        // phAjax.getMyself()
        /// <summary>
        /// 获取自己资料
        /// </summary>
        /// <returns>自己资料</returns>
        [Authorize]
        [HttpGet]
        public async Task<string> Get()
        {
            return await EncryptAsync(await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.PrimaryKey).FetchMyself());
        }

        // phAjax.register()
        /// <summary>
        /// 注册(初始口令即登录名)
        /// </summary>
        /// <param name="companyName">公司名</param>
        /// <param name="userName">登录名</param>
        /// <param name="phone">手机</param>
        /// <param name="eMail">邮箱</param>
        /// <param name="regAlias">昵称</param>
        [AllowAnonymous]
        [HttpPut]
        public async Task<string> Register(string companyName, string userName, string phone, string eMail, string regAlias)
        {
            if (String.IsNullOrEmpty(companyName))
                throw new ArgumentNullException(nameof(companyName), "公司名不允许为空!");
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName), "登录名不允许为空!");

            IIdentity identity = Principal.FetchIdentity(companyName, userName, Request.GetAcceptLanguage(), null);
            return await ClusterClient.Default.GetGrain<IUserGrain>(identity.PrimaryKey).Register(phone, eMail, regAlias, Request.GetRemoteAddress());
        }

        // phAjax.patchMyself()
        /// <summary>
        /// 更新自己资料
        /// </summary>
        [Authorize]
        [HttpPatch]
        public async Task Patch()
        {
            await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.PrimaryKey).PatchKernel(await Request.ReadBodyAsync<Dictionary<string, object>>(true));
        }
    }
}