﻿using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
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
        /// 已靠泊
        /// </summary>
        Berthed,

        /// <summary>
        /// 已离港
        /// </summary>
        Departed
    }
}