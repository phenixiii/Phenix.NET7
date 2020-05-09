using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Log;

namespace Phenix.Services.Plugin.Api.Log
{
    /// <summary>
    /// 事件日志控制器
    /// </summary>
    [EnableCors]
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiLogEventLogPath)]
    [ApiController]
    public sealed class EventLogController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 保存
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task Save()
        {
            EventLog.Save(await Request.ReadBodyAsync<EventInfo>());
        }
    }
}
