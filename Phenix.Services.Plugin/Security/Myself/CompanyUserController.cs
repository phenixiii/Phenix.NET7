using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Net.Filters;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security.Myself
{
    /// <summary>
    /// 公司用户控制器
    /// </summary>
    [Route(ApiConfig.ApiSecurityMyselfCompanyUserPath)]
    [ApiController]
    public sealed class CompanyUserController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取公司用户资料
        /// </summary>
        /// <returns>公司用户资料</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpGet]
        public async Task<string> Get()
        {
            return await EncryptAsync(await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.PrimaryKey).FetchCompanyUsers());
        }

        /// <summary>
        /// 注册公司用户资料
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
        [HttpPost]
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
        [HttpPut]
        public async Task Put(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "登录名不允许为空!");
            
            await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.FormatPrimaryKey(name)).PutKernel(await Request.ReadBodyAsync<Phenix.Services.Business.Security.User>(true));
        }
    }
}