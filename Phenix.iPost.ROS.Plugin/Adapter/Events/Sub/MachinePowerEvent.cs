using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械动力事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="PowerType">动力类型</param>
    /// <param name="PowerStatus">动力状态</param>
    /// <param name="SurplusCapacityPercent">剩余容量百分比</param>
    [Serializable]
    public record MachinePowerEvent(string MachineId,
            PowerType PowerType,
            PowerStatus PowerStatus,
            int? SurplusCapacityPercent)
        : MachineEvent(MachineId);
}