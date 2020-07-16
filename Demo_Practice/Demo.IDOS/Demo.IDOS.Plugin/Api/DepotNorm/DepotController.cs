using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Actor.DepotNorm;
using Demo.IDOS.Plugin.Business.DepotNorm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Data.Schema;
using Phenix.Core.Net.Filters;

namespace Demo.IDOS.Plugin.Api.DepotNorm
{
    /// <summary>
    /// 仓库
    /// </summary>
    [Route(ApiConfig.ApiDepotNormDepotPath)]
    [ApiController]
    public sealed class DepotController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取仓库资料
        /// </summary>
        /// <returns>仓库资料</returns>
        [Authorize]
        [HttpGet]
        public IList<DdnDepot> Get()
        {
            return DdnDepot.FetchAll(Database.Default);
        }

        /// <summary>
        /// 获取仓库资料
        /// </summary>
        /// <param name="id">仓库ID</param>
        /// <returns>仓库资料</returns>
        [Authorize]
        [HttpGet]
        public async Task<DdnDepot> Get(long id)
        {
            return await ClusterClient.Default.GetGrain<IDepotGrain>(id).FetchKernel();
        }

        /// <summary>
        /// 更新仓库资料(如不存在则新增)
        /// </summary>
        /// <param name="id">仓库ID</param>
        /// <returns>更新记录数</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPost]
        public async Task<int> Post(long id)
        {
            return await ClusterClient.Default.GetGrain<IDepotGrain>(id).PatchKernel(await Request.ReadBodyAsync<NameValue[]>());
        }
    }
}
