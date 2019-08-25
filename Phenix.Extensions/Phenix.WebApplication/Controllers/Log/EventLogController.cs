using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Log;
using Phenix.Core.Net;
using Phenix.Core.Reflection;

namespace Phenix.WebApplication.Controllers.Log
{
    /// <summary>
    /// 事件日志控制器
    /// </summary>
    [EnableCors]
    [Route(NetConfig.LogEventLogPath)]
    [ApiController]
    public sealed class EventLogController : Phenix.Core.Net.ControllerBase
    {
        // POST: /api/log/event-log
        /// <summary>
        /// 保存
        /// </summary>
        [Authorize]
        [HttpPost]
        public void Post()
        {
            EventLog.Save(Utilities.JsonDeserialize<EventInfo>(Request.ReadBodyAsString()));
        }
    }
}
