using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械任务反馈事件
    /// </summary>
    [Serializable]
    public record MachineTaskAckEvent : MachineTaskEvent
    {
        /// <summary>
        /// 机械任务反馈事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="taskId">任务ID</param>
        /// <param name="taskStatus">任务状态</param>
        [Newtonsoft.Json.JsonConstructor]
        public MachineTaskAckEvent(string machineId, MachineType machineType, string taskId, TaskStatus taskStatus)
            : base(machineId, machineType, taskId, taskStatus)
        {
        }
    }
}