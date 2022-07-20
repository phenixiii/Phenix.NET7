using System;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 船舶靠泊事件
    /// </summary>
    /// <param name="VesselCode">船舶代码（全域唯一）</param>
    /// <param name="TerminalCode">码头代码（全域唯一）</param>
    /// <param name="Voyage">航次</param>
    /// <param name="AlongSide">靠泊方向</param>
    /// <param name="BowBollardId">船头缆桩号</param>
    /// <param name="BowBollardOffset">船头缆桩偏差值cm</param>
    /// <param name="SternBollardId">船尾缆桩号</param>
    /// <param name="SternBollardOffset">船尾缆桩偏差值cm</param>
    [Serializable]
    public record VesselBerthEvent(
            string VesselCode,
            string TerminalCode,
            string Voyage,
            VesselAlongSide AlongSide,
            string BowBollardId,
            int BowBollardOffset,
            string SternBollardId,
            int SternBollardOffset)
        : IntegrationEvent;
}