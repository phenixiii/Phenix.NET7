using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 响应拖车任务事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="TaskId">任务ID</param>
    /// <param name="TaskStatus">任务状态</param>
    /// <param name="TaskType">任务类型</param>
    [Serializable]
    public record AckVehicleTaskEvent(string MachineId, string TaskId, TaskStatus TaskStatus,
            VehicleTaskType TaskType)
        : MachineTaskEvent(MachineId, TaskId, TaskStatus);
}