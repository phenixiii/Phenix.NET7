using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events
{
    /// <summary>
    /// 机械任务事件
    /// </summary>
    [Serializable]
    public record MachineTaskEvent : MachineEvent
    {
        /// <summary>
        /// 机械任务事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="taskId">任务ID</param>
        /// <param name="taskStatus">任务状态</param>
        [Newtonsoft.Json.JsonConstructor]
        public MachineTaskEvent(string machineId, MachineType machineType,
            string taskId, TaskStatus taskStatus)
            : base(machineId, machineType)
        {
            this.TaskId = taskId;
            this.TaskStatus = taskStatus;
        }

        #region 属性

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus TaskStatus { get; }

        #endregion
    }
}
