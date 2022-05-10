using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 船舶动作事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="Action">船舶动作</param>
    /// <param name="AlongSide">Action=Berthed时有意义为靠泊方向</param>
    /// <param name="BowBollardId">Action=Berthed时有意义为船头缆桩号</param>
    /// <param name="BowBollardOffset">Action=Berthed时有意义为船头缆桩偏差值cm</param>
    /// <param name="SternBollardId">Action=Berthed时有意义为船尾缆桩号</param>
    /// <param name="SternBollardOffset">Action=Berthed时有意义为船尾缆桩偏差值cm</param>
    [Serializable]
    public record VesselActionEvent(string MachineId,
            VesselAction Action,
            VesselAlongSide AlongSide,
            string BowBollardId,
            string BowBollardOffset,
            string SternBollardId,
            string SternBollardOffset
        )
        : MachineEvent(MachineId);
}