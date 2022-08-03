using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Pub
{
    /// <summary>
    /// 拖车装船允许事件
    /// </summary>
    [Serializable]
    public record VehicleShipmentAllowEvent : MachineTaskEvent
    {
        /// <summary>
        /// 拖车装船允许事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="taskId">任务ID</param>
        /// <param name="taskStatus">任务状态</param>
        /// <param name="loadable">允许上档</param>
        /// <param name="order">上档次序</param>
        [Newtonsoft.Json.JsonConstructor]
        public VehicleShipmentAllowEvent(string machineId, MachineType machineType, string taskId, TaskStatus taskStatus,
            bool loadable, int order)
            : base(machineId, machineType, taskId, taskStatus)
        {
            this.Loadable = loadable;
            this.Order = order;
        }

        #region 属性

        /// <summary>
        /// 允许上档
        /// </summary>
        public bool Loadable { get; }

        /// <summary>
        /// 上档次序
        /// </summary>
        public int Order { get; }

        #endregion
    }
}