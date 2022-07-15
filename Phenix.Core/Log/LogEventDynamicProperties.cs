using Phenix.Core.Security;
using Serilog.Core;
using Serilog.Events;

namespace Phenix.Core.Log
{
    /// <summary>
    /// Serilog日志动态属性
    /// </summary>
    public class LogEventDynamicProperties : ILogEventEnricher
    {
        /// <summary>
        /// 填充
        /// </summary>
        /// <param name="logEvent">日志</param>
        /// <param name="propertyFactory">日志属性工厂</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            IIdentity currentIdentity = Principal.CurrentIdentity;
            if (currentIdentity != null)
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Identity",
                    new { CompanyName = currentIdentity.CompanyName, UserName = currentIdentity.UserName }));
        }
    }
}