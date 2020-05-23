using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data.Schema;
using Phenix.Services.Plugin.Actor;

namespace Phenix.Services.Plugin.Api.Security.Myself
{
    /// <summary>
    /// 公司用户控制器
    /// </summary>
    [EnableCors]
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityMyselfCompanyUserPath)]
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
            return await EncryptAsync(await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.Name).FetchCompanyUsers());
        }

        /// <summary>
        /// 登记/注册公司用户
        /// </summary>
        /// <param name="name">登录名</param>
        /// <param name="phone">手机(注册用可为空)</param>
        /// <param name="eMail">邮箱(注册用可为空)</param>
        /// <param name="regAlias">注册昵称(注册用可为空)</param>
        /// <param name="teamsId">所属团体ID</param>
        /// <param name="positionId">担任岗位ID</param>
        /// <returns>返回信息</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPut]
        public async Task<string> Put(string name, string phone, string eMail, string regAlias, long teamsId, long positionId)
        {
            return await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.Name).RegisterCompanyUser(name, phone, eMail, regAlias, Request.GetRemoteAddress(), teamsId, positionId);
        }

        /// <summary>
        /// 更新公司用户资料
        /// </summary>
        /// <param name="name">登录名</param>
        /// <returns>更新记录数</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task<int> Patch(string name)
        {
            return await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.Name).PatchCompanyUser(name, await Request.ReadBodyAsync<NameValue[]>(true));
        }
    }
}