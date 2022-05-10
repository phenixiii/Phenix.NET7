using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械状态事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="MachineStatus">机械状态</param>
    /// <param name="TechnicalStatus">工艺状态</param>
    [Serializable]
    public record MachineStatusEvent(string MachineId,
            MachineStatus MachineStatus,
            MachineTechnicalStatus TechnicalStatus)
        : MachineEvent(MachineId);
}
