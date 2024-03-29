﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Actor.Security;
using Phenix.Core.Net.Filters;

namespace Phenix.Services.Host.Library.Security.Myself
{
    /// <summary>
    /// 登录口令控制器
    /// </summary>
    [Route(StandardPaths.SecurityMyselfPasswordPath)]
    [ApiController]
    public sealed class PasswordController : Phenix.Core.Net.Api.ControllerBase
    {
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

            await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.FormatPrimaryKey(name)).ResetPassword();
        }
    }
}