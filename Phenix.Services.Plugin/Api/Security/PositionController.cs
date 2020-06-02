using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Data.Schema;
using Phenix.Services.Plugin.Actor;

namespace Phenix.Services.Plugin.Api.Security
{
    /// <summary>
    /// 用户岗位控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityPositionPath)]
    [ApiController]
    public sealed class PositionController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取岗位资料
        /// </summary>
        /// <returns>岗位资料</returns>
        [Authorize]
        [HttpGet]
        public IList<Core.Security.Position> Get()
        {
            return Phenix.Core.Security.Position.FetchAll(Database.Default);
        }

        /// <summary>
        /// 获取岗位资料
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>岗位资料</returns>
        [Authorize]
        [HttpGet]
        public async Task<Phenix.Core.Security.Position> Get(long id)
        {
            return await ClusterClient.Default.GetGrain<IPositionGrain>(id).FetchKernel();
        }

        /// <summary>
        /// 更新岗位资料
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>更新记录数</returns>
        [SystemAdminFilter]
        [Authorize]
        [HttpPost]
        public async Task<int> Post(long id)
        {
            return await ClusterClient.Default.GetGrain<IPositionGrain>(id).PatchKernel(await Request.ReadBodyAsync<NameValue[]>());
        }
    }
}