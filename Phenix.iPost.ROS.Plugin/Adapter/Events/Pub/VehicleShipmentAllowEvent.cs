using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Pub
{
    /// <summary>
    /// 拖车装船允许事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="TaskId">任务ID</param>
    /// <param name="TaskStatus">任务状态</param>
    /// <param name="Loadable">是否允许上档</param>
    /// <param name="Order">Loadable=true时有意义为上档次序</param>
    [Serializable]
    public record VehicleShipmentAllowEvent(string MachineId, string TaskId, TaskStatus TaskStatus,
            bool Loadable,
            int Order)
        : MachineTaskEvent(MachineId, TaskId, TaskStatus);
}