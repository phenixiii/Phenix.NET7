using System;
using Phenix.Core.Event;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 船舶状态事件
    /// </summary>
    /// <param name="VesselId">机械ID（全域唯一）</param>
    /// <param name="Status">船舶状态</param>
    /// <param name="AlongSide">Status=Berthed时有意义为靠泊方向</param>
    /// <param name="BowBollardId">Status=Berthed时有意义为船头缆桩号</param>
    /// <param name="BowBollardOffset">Status=Berthed时有意义为船头缆桩偏差值cm</param>
    /// <param name="SternBollardId">Status=Berthed时有意义为船尾缆桩号</param>
    /// <param name="SternBollardOffset">Status=Berthed时有意义为船尾缆桩偏差值cm</param>
    [Serializable]
    public record VesselStatusEvent(string VesselId,
            VesselStatus Status,
            VesselAlongSide AlongSide,
            string BowBollardId,
            string BowBollardOffset,
            string SternBollardId,
            string SternBollardOffset
        )
        : IntegrationEvent;
}