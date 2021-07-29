using System;

namespace Phenix.Norm
{
    /// <summary>
    /// 设备状态
    /// </summary>
    [Serializable]
    public enum DeviceStatus
    {
        /// <summary>
        /// 待机中
        /// </summary>
        [Phenix.Core.Data.EnumCaption("待机中", Key = "S")]
        Standby,

        /// <summary>
        /// 运行中
        /// </summary>
        [Phenix.Core.Data.EnumCaption("运行中", Key = "R")]
        Running,

        /// <summary>
        /// 故障中
        /// </summary>
        [Phenix.Core.Data.EnumCaption("故障中", Key = "B")]
        Breakdown,

        /// <summary>
        /// 保养中
        /// </summary>
        [Phenix.Core.Data.EnumCaption("保养中", Key = "M")]
        Maintenance,

        /// <summary>
        /// 废弃的
        /// </summary>
        [Phenix.Core.Data.EnumCaption("废弃的", Key = "D")]
        Discarded,
    }
}
