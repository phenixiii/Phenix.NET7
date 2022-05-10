using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 拖车泊位作业事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="OperationStatus">作业状态</param>
    [Serializable]
    public record VehicleBerthOperationEvent(string MachineId,
            VehicleBerthOperationStatus OperationStatus)
        : MachineEvent(MachineId);
}