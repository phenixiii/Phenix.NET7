using System;
using Phenix.Core.Data.Schema;

namespace Demo.IDOS.Plugin.Rule
{
    /// <summary>
    /// 货物尺寸
    /// </summary>
    [Serializable]
    public enum GoodsSize
    {
        /// <summary>
        /// 小箱
        /// </summary>
        [EnumCaption("小箱")]
        Small,
        
        /// <summary>
        /// 大箱
        /// </summary>
        [EnumCaption("大箱")]
        Big,
    }
}
