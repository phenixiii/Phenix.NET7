using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.InspectionStation.Plugin.Actor;
using Demo.InspectionStation.Plugin.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;

namespace Demo.InspectionStation.Plugin.Api.Center
{
    /// <summary>
    /// 侦听Controller
    /// </summary>
    [Route(ApiConfig.ApiCenterListenPath)]
    [ApiController]
    public sealed class ListenController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取监控的作业点
        /// </summary>
        /// <returns>监控的作业点</returns>
        [Authorize]
        [HttpGet]
        public async Task<IDictionary<string, IsOperationPoint>> Get()
        {
            return await ClusterClient.Default.GetGrain<ICenterGrain>(User.Identity.Name).FetchOperationPoint();
        }

        /// <summary>
        /// 监控指定的作业点
        /// </summary>
        [Authorize]
        [HttpPut]
        public async Task Put()
        {
            await ClusterClient.Default.GetGrain<ICenterGrain>(User.Identity.Name).Listen(await Request.ReadBodyAsync<List<string>>());
        }
    }
}

