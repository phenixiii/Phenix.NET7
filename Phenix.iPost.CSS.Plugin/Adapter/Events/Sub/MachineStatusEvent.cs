﻿using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械状态事件
    /// </summary>
    [Serializable]
    public record MachineStatusEvent : MachineEvent
    {
        /// <summary>
        /// 机械状态事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="machineStatus">机械状态</param>
        /// <param name="technicalStatus">工艺状态</param>
        [Newtonsoft.Json.JsonConstructor]
        public MachineStatusEvent(string machineId, MachineType machineType,
            MachineStatus machineStatus, MachineTechnicalStatus technicalStatus)
            : base(machineId, machineType)
        {
            this.MachineStatus = machineStatus;
            this.TechnicalStatus = technicalStatus;
        }

        #region 属性

        /// <summary>
        /// 机械状态
        /// </summary>
        public MachineStatus MachineStatus { get; }

        /// <summary>
        /// 工艺状态
        /// </summary>
        public MachineTechnicalStatus TechnicalStatus { get; }

        #endregion
    }
}