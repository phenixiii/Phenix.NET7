using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 岗位资料控制器
    /// </summary>
    [Route(WebApiConfig.SecurityPositionPath)]
    [ApiController]
    public sealed class PositionController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取岗位资料
        /// </summary>
        /// <returns>岗位资料</returns>
        [Authorize]
        [HttpGet("all")]
        public IList<Position> Get()
        {
            return Position.FetchList(Database.Default);
        }

        /// <summary>
        /// 获取岗位资料
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>岗位资料</returns>
        [Authorize]
        [HttpGet]
        public async Task<Position> Get(long id)
        {
            return await ClusterClient.Default.GetGrain<IPositionGrain>(id).FetchKernel();
        }
    }
}