﻿using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Pub
{
    /// <summary>
    /// 拖车任务事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="TaskId">任务ID</param>
    /// <param name="TaskStatus">任务状态</param>
    /// <param name="TaskType">任务类型</param>
    /// <param name="Destination">目的位置</param>
    /// <param name="CarryContainer1">（计划/实际）载箱1</param>
    /// <param name="CarryContainer2">（计划/实际）载箱2</param>
    [Serializable]
    public record VehicleCarryContainerTaskEvent(string MachineId, string TaskId, TaskStatus TaskStatus, VehicleTaskType TaskType, DriveDestinationProperty Destination,
            CarryContainerProperty CarryContainer1,
            CarryContainerProperty CarryContainer2)
        : VehicleTaskEvent(MachineId, TaskId, TaskStatus, TaskType, Destination);
}
