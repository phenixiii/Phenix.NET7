using System;
using Phenix.Core.Event;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 船舶离泊事件
    /// </summary>
    /// <param name="VesselCode">船舶代码（全域唯一）</param>
    /// <param name="TerminalCode">码头代码（全域唯一）</param>
    /// <param name="Voyage">航次</param>
    [Serializable]
    public record VesselDepartEvent(
            string VesselCode,
            string TerminalCode,
            string Voyage)
        : IntegrationEvent;
}