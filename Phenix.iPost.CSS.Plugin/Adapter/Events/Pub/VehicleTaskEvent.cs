using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Pub
{
    /// <summary>
    /// 拖车任务事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="TaskId">任务ID</param>
    /// <param name="TaskStatus">任务状态</param>
    /// <param name="TaskType">任务类型</param>
    /// <param name="TaskPurpose">任务目标</param>
    /// <param name="OperationType">作业类型</param>
    /// <param name="Site">目的地（装卸点/换电站/停车位，全域唯一，即可控生产区域内各类目的地之间都不允许重复）</param>
    /// <param name="Bay">目的地贝位（目的地是装卸点才有意义）</param>
    /// <param name="Lane">目的地车道（目的地是装卸点才有意义）</param>
    [Serializable]
    public record VehicleTaskEvent(string MachineId, MachineType MachineType, string TaskId, TaskStatus TaskStatus,
            VehicleTaskType TaskType,
            VehicleTaskPurpose TaskPurpose,
            VehicleOperationType OperationType,
            string Site,
            string Bay,
            string Lane)
        : MachineTaskEvent(MachineId, MachineType, TaskId, TaskStatus);
}
