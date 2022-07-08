using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business.Property
{
    /// <summary>
    /// 机械状态属性
    /// </summary>
    /// <param name="MachineStatus">机械状态</param>
    /// <param name="TechnicalStatus">工艺状态</param>
    [Serializable]
    public readonly record struct MachineStatusProperty(
        MachineStatus MachineStatus,
        MachineTechnicalStatus TechnicalStatus)
    {
        #region 属性

        /// <summary>
        /// 机械状态
        /// </summary>
        public MachineStatus MachineStatus { get; init; } = TechnicalStatus == MachineTechnicalStatus.Red ? MachineStatus.Maintenance : MachineStatus;

        /// <summary>
        /// 工艺状态
        /// </summary>
        public MachineTechnicalStatus TechnicalStatus { get; init; } = TechnicalStatus;

        #endregion
    }
}