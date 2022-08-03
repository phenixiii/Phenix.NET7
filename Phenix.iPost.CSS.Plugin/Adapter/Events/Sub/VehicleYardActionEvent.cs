using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 拖车堆场动作事件
    /// </summary>
    [Serializable]
    public record VehicleYardActionEvent : MachineEvent
    {
        /// <summary>
        /// 拖车堆场动作事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="yardAction">堆场动作</param>
        [Newtonsoft.Json.JsonConstructor]
        public VehicleYardActionEvent(string machineId, MachineType machineType,
            VehicleYardAction yardAction)
            : base(machineId, machineType)
        {
            this.YardAction = yardAction;
        }

        #region 属性

        /// <summary>
        /// 堆场动作
        /// </summary>
        public VehicleYardAction YardAction { get; }

        #endregion
    }
}