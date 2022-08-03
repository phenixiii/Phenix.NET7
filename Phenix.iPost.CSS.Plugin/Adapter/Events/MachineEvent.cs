using System;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events
{
    /// <summary>
    /// 机械事件
    /// </summary>
    [Serializable]
    public record MachineEvent : IntegrationEvent
    {
        /// <summary>
        /// 机械事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        [Newtonsoft.Json.JsonConstructor]
        public MachineEvent(string machineId, MachineType machineType)
            : base()
        {
            this.MachineId = machineId;
            this.MachineType = machineType;
        }

        #region 属性

        /// <summary>
        /// 机械ID
        /// </summary>
        public string MachineId { get; }

        /// <summary>
        /// 机械类型
        /// </summary>
        public MachineType MachineType { get; }

        #endregion
    }
}
