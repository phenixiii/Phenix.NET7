﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 用户自己控制器
    /// </summary>
    [Route(ApiConfig.ApiSecurityMyselfPath)]
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

        /// <summary>
        /// 更新自己资料
        /// </summary>
        [Authorize]
        [HttpPatch]
        public async Task Patch()
        {
            await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.PrimaryKey).PatchKernel(await Request.ReadBodyAsync<User>(true));
        }
    }
}