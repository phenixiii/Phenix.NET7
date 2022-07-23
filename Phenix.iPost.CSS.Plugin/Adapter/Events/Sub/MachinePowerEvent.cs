using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械动力事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="PowerType">动力类型</param>
    /// <param name="PowerStatus">动力状态</param>
    /// <param name="SurplusCapacityPercent">剩余容量百分比</param>
    [Serializable]
    public record MachinePowerEvent(string MachineId, MachineType MachineType,
            PowerType PowerType,
            PowerStatus PowerStatus,
            int? SurplusCapacityPercent)
        : MachineEvent(MachineId, MachineType);
}