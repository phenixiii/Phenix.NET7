using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Contract;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 用户自己控制器
    /// </summary>
    [Route(WebApiConfig.SecurityMyselfPath)]
    [ApiController]
    public sealed class MyselfController : Phenix.Core.Net.Api.ControllerBase
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
        [AllowAnonymous]
        [HttpPut]
        public async Task<string> Register()
        {
            dynamic body = await Request.ReadBodyAsDynamicAsync();
            if (String.IsNullOrEmpty((string) body.companyName))
                throw new ArgumentNullException(nameof(body.companyName), "公司名不允许为空!");
            if (String.IsNullOrEmpty((string) body.userName))
                throw new ArgumentNullException(nameof(body.userName), "登录名不允许为空!");

            IIdentity identity = Principal.FetchIdentity((string) body.companyName, (string) body.userName, Request.GetAcceptLanguage(), null);
            return await ClusterClient.Default.GetGrain<IUserGrain>(identity.PrimaryKey).Register((string) body.phone, (string) body.eMail, (string) body.regAlias, Request.GetRemoteAddress());
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