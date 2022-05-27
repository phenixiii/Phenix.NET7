using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 载箱位置
    /// </summary>
    [Serializable]
    public enum CarryContainerPosition
    {
        /// <summary>
        /// Fore前箱
        /// </summary>
        F,

        /// <summary>
        /// Middle中箱
        /// </summary>
        M,

        /// <summary>
        /// After后箱
        /// </summary>
        A
    }
}