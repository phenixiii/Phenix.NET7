using System.Threading.Tasks;
using Demo.IDOS.Plugin.Actor.GateOperation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Api.GateOperation
{
    /// <summary>
    /// 车牌Controller
    /// </summary>
    [Route(ApiConfig.ApiGateOperationLicensePlatePath)]
    [ApiController]
    public sealed class LicensePlateController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 输入
        /// </summary>
        /// <param name="depotId">仓库ID</param>
        /// <param name="gateName">道口名称</param>
        [Authorize(ApiRoles.GateDevice)]
        [HttpPut]
        public async Task<string> Put(long depotId, string gateName)
        {
            return await ClusterClient.Default.GetGrain<IInGateGrain>(depotId, gateName).PermitByLicensePlate(await Request.ReadBodyAsync<string>());
        }
    }
}
