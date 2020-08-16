using System;
using Phenix.Core.Data.Schema;

namespace Demo.IDOS.Plugin.Rule
{
    /// <summary>
    /// 作业类型
    /// </summary>
    [Serializable]
    public enum OperationType
    {
        /// <summary>
        /// 未知
        /// </summary>
        [EnumCaption("未知")]
        Unknown,

        /// <summary>
        /// 存入
        /// </summary>
        [EnumCaption("存入")]
        StockIn,

        /// <summary>
        /// 提出
        /// </summary>
        [EnumCaption("提出")]
        StockOut,
    }
}
