using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 拖车堆场动作事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="YardAction">堆场动作</param>
    [Serializable]
    public record VehicleYardActionEvent(string MachineId, MachineType MachineType,
            VehicleYardAction YardAction)
        : MachineEvent(MachineId, MachineType);
}