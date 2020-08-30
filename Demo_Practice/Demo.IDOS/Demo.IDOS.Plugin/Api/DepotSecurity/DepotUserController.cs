using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Actor.DepotSecurity;
using Demo.IDOS.Plugin.Business.DepotSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Net.Filters;

namespace Demo.IDOS.Plugin.Api.DepotSecurity
{
    /// <summary>
    /// 仓库用户
    /// </summary>
    [Route(ApiConfig.ApiDepotSecurityDepotUserPath)]
    [ApiController]
    public sealed class DepotUserController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取用户可操作的仓库
        /// </summary>
        /// <returns>仓库用户</returns>
        [Authorize]
        [HttpGet]
        public IList<DdsDepotUser> Get()
        {
            return DdsDepotUser.FetchAll(Database.Default, p => p.UsId == User.Identity.Id && p.Disabled == false);
        }

        /// <summary>
        /// 某用户是否可操作某仓库
        /// </summary>
        /// <param name="depotId">仓库ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>是否可用</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpGet]
        public async Task<bool> Usable(long depotId, long userId)
        {
            return await ClusterClient.Default.GetGrain<IDepotUserGrain>(depotId, userId.ToString()).Usable();
        }

        /// <summary>
        /// 某用户可操作某仓库
        /// </summary>
        /// <param name="depotId">仓库ID</param>
        /// <param name="userId">用户ID</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPut]
        public async Task Able(long depotId, long userId)
        {
            await ClusterClient.Default.GetGrain<IDepotUserGrain>(depotId, userId.ToString()).Able();
        }

        /// <summary>
        /// 某用户不可操作某仓库
        /// </summary>
        /// <param name="depotId">仓库ID</param>
        /// <param name="userId">用户ID</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpDelete]
        public async Task Disable(long depotId, long userId)
        {
            await ClusterClient.Default.GetGrain<IDepotUserGrain>(depotId, userId.ToString()).Disable();
        }
    }
}
