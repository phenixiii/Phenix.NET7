using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 船舶动作
    /// </summary>
    [Serializable]
    public enum VesselAction
    {
        /// <summary>
        /// 已靠泊
        /// </summary>
        Berthed,

        /// <summary>
        /// 已离港
        /// </summary>
        Departed
    }
}
