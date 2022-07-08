using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械状态事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="MachineStatus">机械状态</param>
    /// <param name="TechnicalStatus">工艺状态</param>
    [Serializable]
    public record MachineStatusEvent(string MachineId, MachineType MachineType,
            MachineStatus MachineStatus,
            MachineTechnicalStatus TechnicalStatus)
        : MachineEvent(MachineId, MachineType);
}
