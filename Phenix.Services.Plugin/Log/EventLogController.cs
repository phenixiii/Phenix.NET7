using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Log;

namespace Phenix.Services.Plugin.Log
{
    /// <summary>
    /// 事件日志控制器
    /// </summary>
    [Route(ApiConfig.ApiLogEventLogPath)]
    [ApiController]
    public sealed class EventLogController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 保存
        /// </summary>
        [Authorize]
        [HttpPost]
        public bool Save([FromBody] EventInfo eventInfo)
        {
            return EventLog.Save(eventInfo);
        }
    }
}
