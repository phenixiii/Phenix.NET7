using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 拖车泊位动作事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="BerthAction">泊位动作</param>
    [Serializable]
    public record VehicleBerthActionEvent(string MachineId, MachineType MachineType,
            VehicleBerthAction BerthAction)
        : MachineEvent(MachineId, MachineType);
}