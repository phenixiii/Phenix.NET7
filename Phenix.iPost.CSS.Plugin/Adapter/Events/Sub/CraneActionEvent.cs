using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 吊车动作事件
    /// </summary>
    [Serializable]
    public record CraneActionEvent : MachineEvent
    {
        /// <summary>
        /// 吊车动作事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="craneType">吊车类型</param>
        /// <param name="craneAction">吊车动作</param>
        [Newtonsoft.Json.JsonConstructor]
        public CraneActionEvent(string machineId, MachineType machineType,
            CraneType craneType, CraneAction craneAction)
            : base(machineId, machineType)
        {
            this.CraneType = craneType;
            this.CraneAction = craneAction;
        }

        #region 属性

        /// <summary>
        /// 吊车类型
        /// </summary>
        public CraneType CraneType { get; }

        /// <summary>
        /// 吊车动作
        /// </summary>
        public CraneAction CraneAction { get; }

        #endregion
    }
}