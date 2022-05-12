using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Pub
{
    /// <summary>
    /// 拖车装船任务事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="TaskId">任务ID</param>
    /// <param name="TaskStatus">任务状态</param>
    /// <param name="PlanDestination">计划目的位置</param>
    /// <param name="CarryContainerProperty1">（计划/实装）载箱1属性</param>
    /// <param name="CarryContainerProperty2">（计划/实装）载箱2属性</param>
    /// <param name="OperationStatus">作业状态</param>
    /// <param name="OneByOneLoading">双箱时有意义一个一个装船</param>
    [Serializable]
    public record VehicleShipmentTaskEvent(string MachineId, string TaskId, TaskStatus TaskStatus, DriveDestinationProperty PlanDestination, CarryContainerProperty CarryContainerProperty1, CarryContainerProperty CarryContainerProperty2,
            VehicleShipmentOperationStatus OperationStatus,
            bool OneByOneLoading)
        : VehicleCarryContainerTaskEvent(MachineId, TaskId, TaskStatus, VehicleTaskType.ShipmentOperation, PlanDestination, CarryContainerProperty1, CarryContainerProperty2);
}