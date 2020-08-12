using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Net.Filters;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor.Security;

namespace Phenix.Services.Plugin.Api.Security.Myself
{
    /// <summary>
    /// 用户顶层团体控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityMyselfRootTeamsPath)]
    [ApiController]
    public sealed class RootTeamsController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取顶层团体资料
        /// </summary>
        /// <returns>顶层团体资料</returns>
        [Authorize]
        [HttpGet]
        public async Task<Teams> Get()
        {
            return User.Identity.RootTeamsProxy != null
                ? await User.Identity.RootTeamsProxy.FetchKernel()
                : null;
        }

        /// <summary>
        /// 新增/更新顶层团体资料
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>顶层团体主键</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task<long> Patch(string name)
        {
            return await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.Name).PatchRootTeams(name);
        }
    }
}