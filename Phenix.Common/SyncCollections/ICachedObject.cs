using System;

namespace Phenix.Common.SyncCollections
{
    /// <summary>
    /// 缓存对象接口
    /// </summary>
    public interface ICachedObject
    {
        #region 属性

        /// <summary>
        /// 失效时间
        /// </summary>
        DateTime InvalidTime { get; set; }

        /// <summary>
        /// 是否失效
        /// </summary>
        bool IsInvalid { get; }

        #endregion
    }
}