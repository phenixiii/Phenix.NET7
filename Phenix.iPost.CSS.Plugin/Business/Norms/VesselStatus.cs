using System;

namespace Phenix.iPost.CSS.Plugin.Business.Norms
{
    /// <summary>
    /// 船舶状态
    /// </summary>
    [Serializable]
    public enum VesselStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,

        /// <summary>
        /// 靠泊中
        /// </summary>
        Berthing,

        /// <summary>
        /// 已离泊
        /// </summary>
        Departed
    }
}
