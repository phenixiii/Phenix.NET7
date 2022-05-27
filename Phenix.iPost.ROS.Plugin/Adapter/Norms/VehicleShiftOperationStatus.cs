﻿using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车转堆作业状态
    /// </summary>
    [Serializable]
    public enum VehicleShiftOperationStatus
    {
        /// <summary>
        /// 堆场收箱1
        /// VehicleYardOperationStatus
        /// </summary>
        YardReceive1,

        /// <summary>
        /// 堆场收箱2
        /// VehicleYardOperationStatus
        /// </summary>
        YardReceive2,

        /// <summary>
        /// 堆场送箱1
        /// VehicleYardOperationStatus
        /// </summary>
        YardDeliver1,

        /// <summary>
        /// 堆场送箱2
        /// VehicleYardOperationStatus
        /// </summary>
        YardDeliver2,
    }
}