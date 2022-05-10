using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 吊车吊具尺寸
    /// </summary>
    [Serializable]
    public enum CraneSpreaderSize
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,

        /// <summary>
        /// 20尺
        /// </summary>
        Ft20,

        /// <summary>
        /// 40尺
        /// </summary>
        Ft40,

        /// <summary>
        /// 45尺
        /// </summary>
        Ft45,

        /// <summary>
        /// 双20尺
        /// </summary>
        DoubleFt20,
    }
}
