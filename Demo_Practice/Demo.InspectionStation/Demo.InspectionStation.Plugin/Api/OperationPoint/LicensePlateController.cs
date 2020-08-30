using System.Threading.Tasks;
using Demo.InspectionStation.Plugin.Actor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;

namespace Demo.InspectionStation.Plugin.Api.OperationPoint
{
    /// <summary>
    /// 车牌Controller
    /// </summary>
    [Route(ApiConfig.ApiOperationPointLicensePlatePath)]
    [ApiController]
    public sealed class LicensePlateController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="operationPointName">作业点名称</param>
        /// <returns>车牌号</returns>
        [Authorize]
        [HttpGet]
        public async Task<string> Get(string operationPointName)
        {
            return await ClusterClient.Default.GetGrain<IOperationPointGrain>(operationPointName).GetLicensePlate();
        }

        /// <summary>
        /// 输入
        /// </summary>
        /// <param name="operationPointName">作业点名称</param>
        [Authorize]
        [HttpPut]
        public async Task Set(string operationPointName)
        {
            await ClusterClient.Default.GetGrain<IOperationPointGrain>(operationPointName).SetLicensePlate(await Request.ReadBodyAsync<string>());
        }

        /// <summary>
        /// 心跳(每10秒至少2次)
        /// </summary>
        /// <param name="operationPointName">作业点名称</param>
        [Authorize]
        [HttpPost]
        public async Task Alive(string operationPointName)
        {
            await ClusterClient.Default.GetGrain<IOperationPointGrain>(operationPointName).LicensePlateAlive();
        }
    }
}
