﻿using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 机械工艺状态
    /// </summary>
    [Serializable]
    public enum MachineTechnicalStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Green,

        /// <summary>
        /// 还能使用
        /// 但最好找机会维修
        /// </summary>
        Yellow,

        /// <summary>
        /// 能够完成当前指令
        /// 但后续无法接指令
        /// </summary>
        Orange,

        /// <summary>
        /// 无法使用
        /// 需要维修
        /// </summary>
        Red,
    }
}