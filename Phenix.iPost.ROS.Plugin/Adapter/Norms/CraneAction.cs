using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 吊车动作
    /// </summary>
    [Serializable]
    public enum CraneAction
    {
        /// <summary>
        /// 移动中
        /// </summary>
        Moving,

        /// <summary>
        /// 龙吊到位/桥吊落贝
        /// </summary>
        ArriveBay,
    }
}
