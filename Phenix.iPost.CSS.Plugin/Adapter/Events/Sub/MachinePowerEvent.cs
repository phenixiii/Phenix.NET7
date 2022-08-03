﻿using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械动力事件
    /// </summary>
    [Serializable]
    public record MachinePowerEvent : MachineEvent
    {
        /// <summary>
        /// 机械动力事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="powerType">动力类型</param>
        /// <param name="powerStatus">动力状态</param>
        /// <param name="surplusCapacityPercent">剩余容量百分比</param>
        [Newtonsoft.Json.JsonConstructor]
        public MachinePowerEvent(string machineId, MachineType machineType,
            PowerType powerType, PowerStatus powerStatus, int? surplusCapacityPercent)
            : base(machineId, machineType)
        {
            this.PowerType = powerType;
            this.PowerStatus = powerStatus;
            this.SurplusCapacityPercent = surplusCapacityPercent;
        }

        #region 属性

        /// <summary>
        /// 动力类型
        /// </summary>
        public PowerType PowerType { get; }

        /// <summary>
        /// 动力状态
        /// </summary>
        public PowerStatus PowerStatus { get; }

        /// <summary>
        /// 剩余容量百分比
        /// </summary>
        public int? SurplusCapacityPercent { get; }

        #endregion
    }
}