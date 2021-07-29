using System;

namespace Phenix.Norm
{
    /// <summary>
    /// 资料状态
    /// </summary>
    [Serializable]
    public enum DocsStatus
    {
        /// <summary>
        /// 未核对
        /// </summary>
        [Phenix.Core.Data.EnumCaption("未核对", Key = "U")]
        Unverified,

        /// <summary>
        /// 废弃的
        /// </summary>
        [Phenix.Core.Data.EnumCaption("废弃的", Key = "D")]
        Discarded,

        /// <summary>
        /// 有效的
        /// </summary>
        [Phenix.Core.Data.EnumCaption("有效的", Key = "V")]
        Valid,

        /// <summary>
        /// 归档的
        /// </summary>
        [Phenix.Core.Data.EnumCaption("归档的", Key = "A")]
        Archived,
    }
}
