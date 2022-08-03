using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 机械状态
    /// </summary>
    [Serializable]
    public readonly record struct MachineStatusInfo
    {
        /// <summary>
        /// 机械状态属性
        /// </summary>
        /// <param name="machineStatus">机械状态</param>
        /// <param name="technicalStatus">工艺状态</param>
        [Newtonsoft.Json.JsonConstructor]
        public MachineStatusInfo(MachineStatus machineStatus, MachineTechnicalStatus technicalStatus)
        {
            this.MachineStatus = technicalStatus == MachineTechnicalStatus.Red ? MachineStatus.Maintenance : machineStatus;
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