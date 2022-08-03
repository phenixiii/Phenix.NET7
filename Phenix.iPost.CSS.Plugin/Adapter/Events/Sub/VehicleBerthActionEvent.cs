using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 拖车泊位动作事件
    /// </summary>
    [Serializable]
    public record VehicleBerthActionEvent : MachineEvent
    {
        /// <summary>
        /// 拖车泊位动作事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="berthAction">泊位动作</param>
        [Newtonsoft.Json.JsonConstructor]
        public VehicleBerthActionEvent(string machineId, MachineType machineType,
            VehicleBerthAction berthAction)
            : base(machineId, machineType)
        {
            this.BerthAction = berthAction;
        }

        #region 属性

        /// <summary>
        /// 泊位动作
        /// </summary>
        public VehicleBerthAction BerthAction { get; }

        #endregion
    }
}