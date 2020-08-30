using System;
using Phenix.Core.Data.Schema;

namespace Demo.IDOS.Plugin.Rule
{
    /// <summary>
    /// 预约单状态
    /// </summary>
    [Serializable]
    public enum BookingStatus
    {
        /// <summary>
        /// 计划中
        /// </summary>
        [EnumCaption("计划中")]
        Planning,

        /// <summary>
        /// 作业中
        /// </summary>
        [EnumCaption("作业中")]
        Operating,

        /// <summary>
        /// 已完成
        /// </summary>
        [EnumCaption("已完成")] 
        Finish,

        /// <summary>
        /// 已取消
        /// </summary>
        [EnumCaption("已取消")]
        Cancelled,
    }
}
