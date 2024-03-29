﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Actor.Security;
using Phenix.Core.Data;
using Phenix.Core.Net.Filters;
using Phenix.Mapper.Expressions;

namespace Phenix.Services.Host.Library.Security.Myself
{
    /// <summary>
    /// 公司用户控制器
    /// </summary>
    [Route(StandardPaths.SecurityMyselfCompanyUserPath)]
    [ApiController]
    public sealed class CompanyUserController : Phenix.Core.Net.Api.ControllerBase
    {
        // phAjax.getMyselfCompanyUsers()
        /// <summary>
        /// 获取公司用户资料
        /// </summary>
        /// <param name="includeDisabled">包含已注销的</param>
        /// <returns>公司用户资料</returns>
        [Authorize]
        [HttpGet("all")]
        public async Task<string> GetAll(bool includeDisabled)
        {
            return await EncryptAsync(Phenix.Actor.Security.User.FetchList(Database.Default,
                includeDisabled
                    ? p => p.RootTeamsId == User.Identity.RootTeamsId && p.RootTeamsId != p.TeamsId
                    : p => p.RootTeamsId == User.Identity.RootTeamsId && p.RootTeamsId != p.TeamsId && !p.Disabled,
                OrderBy.Ascending<User>(p => p.RegAlias)));
        }

        /// <summary>
        /// 注册公司用户
        /// </summary>
        /// <param name="name">登录名</param>
        /// <param name="phone">手机(注册时可空)</param>
        /// <param name="eMail">邮箱(注册时可空)</param>
        /// <param name="regAlias">昵称(注册时可空)</param>
        /// <param name="teamsId">所属团体ID</param>
        /// <param name="positionId">担任岗位ID</param>
        /// <returns>返回信息</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPut]
        public async Task<string> Register(string name, string phone, string eMail, string regAlias, long teamsId, long positionId)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "登录名不允许为空!");

            return await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.FormatPrimaryKey(name)).Register(phone, eMail, regAlias, Request.GetRemoteAddress(), teamsId, positionId);
        }

        /// <summary>
        /// 更新公司用户资料
        /// </summary>
        /// <param name="name">登录名</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task Patch(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "登录名不允许为空!");

            await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.FormatPrimaryKey(name)).PatchKernel(await Request.ReadBodyAsync<Dictionary<string, object>>(true));
        }
    }
}