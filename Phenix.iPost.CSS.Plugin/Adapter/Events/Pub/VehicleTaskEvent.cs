using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Pub
{
    /// <summary>
    /// 拖车任务事件
    /// </summary>
    [Serializable]
    public record VehicleTaskEvent : MachineTaskEvent
    {
        /// <summary>
        /// 拖车任务事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="taskId">任务ID</param>
        /// <param name="taskStatus">任务状态</param>
        /// <param name="taskType">任务类型</param>
        /// <param name="taskPurpose">任务目标</param>
        /// <param name="operationType">作业类型</param>
        /// <param name="location">目的地（装卸点/换电站/停车位）</param>
        /// <param name="lane">目的地车道（目的地是装卸点才有意义）</param>
        [Newtonsoft.Json.JsonConstructor]
        public VehicleTaskEvent(string machineId, MachineType machineType, string taskId, TaskStatus taskStatus,
            VehicleTaskType taskType, VehicleTaskPurpose taskPurpose, VehicleOperationType operationType, string location, int lane)
            : base(machineId, machineType, taskId, taskStatus)
        {
            this.TaskType = taskType;
            this.TaskPurpose = taskPurpose;
            this.OperationType = operationType;
            this.Location = location;
            this.Lane = lane;
        }

        #region 属性

        /// <summary>
        /// 任务类型
        /// </summary>
        public VehicleTaskType TaskType { get; }

        /// <summary>
        /// 任务目标
        /// </summary>
        public VehicleTaskPurpose TaskPurpose { get; }

        /// <summary>
        /// 作业类型
        /// </summary>
        public VehicleOperationType OperationType { get; }

        /// <summary>
        /// 目的地（装卸点/换电站/停车位）
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// 目的地车道（目的地是装卸点才有意义）
        /// </summary>
        public int Lane { get; }

        #endregion
    }
}
