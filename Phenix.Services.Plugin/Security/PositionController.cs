using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Net.Filters;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 岗位资料控制器
    /// </summary>
    [Route(Phenix.Net.Api.StandardPaths.SecurityPositionPath)]
    [ApiController]
    public sealed class PositionController : Phenix.Net.Api.ControllerBase
    {
        // phAjax.getPositions()
        /// <summary>
        /// 获取岗位资料
        /// </summary>
        /// <returns>岗位资料</returns>
        [Authorize]
        [HttpGet("all")]
        public IList<Position> GetAll()
        {
            return Position.FetchList(Database.Default);
        }

        /// <summary>
        /// 更新岗位资料
        /// </summary>
        /// <param name="id">ID</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task Patch(long id)
        {
            await ClusterClient.Default.GetGrain<IPositionGrain>(id).PatchKernel(await Request.ReadBodyAsync<Dictionary<string, object>>());
        }
    }
}