using Microsoft.AspNetCore.Authorization;
using Phenix.Core.Log;
using Phenix.Core.Net.Api.Service;

namespace Phenix.Services.Plugin.Api.Log
{
    /// <summary>
    /// 事件日志指令
    /// </summary>
    public sealed class EventLogCommand : CommandBase<EventLogCommand>
    {
        /// <summary>
        /// 保存
        /// </summary>
        [Authorize]
        public bool Save(EventInfo eventInfo)
        {
            return EventLog.Save(eventInfo);
        }
    }
}
