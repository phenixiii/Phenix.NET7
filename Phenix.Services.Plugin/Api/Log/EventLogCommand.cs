using Microsoft.AspNetCore.Authorization;
using Phenix.Core.Log;

namespace Phenix.Services.Plugin.Api.Log
{
    /// <summary>
    /// 事件日志指令
    /// </summary>
    public static class EventLogCommand
    {
        /// <summary>
        /// 保存
        /// </summary>
        [Authorize]
        public static bool Save(EventInfo eventInfo)
        {
            return EventLog.Save(eventInfo);
        }
    }
}
