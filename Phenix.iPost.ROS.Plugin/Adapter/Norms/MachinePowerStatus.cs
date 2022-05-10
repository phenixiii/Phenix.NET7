using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 机械动力状态
    /// </summary>
    [Serializable]
    public enum MachinePowerStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Green,

        /// <summary>
        /// 由人员检查判断是否需要更换电池/加油
        /// </summary>
        Yellow,

        /// <summary>
        /// 需要立刻更换电池/加油
        /// </summary>
        Red,
    }
}
