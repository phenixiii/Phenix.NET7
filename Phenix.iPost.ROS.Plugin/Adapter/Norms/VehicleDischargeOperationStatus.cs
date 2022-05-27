﻿using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车卸船作业状态
    /// </summary>
    [Serializable]
    public enum VehicleDischargeOperationStatus
    {
        /// <summary>
        /// 泊位收箱1
        /// VehicleBerthOperationStatus
        /// </summary>
        BerthReceive1,

        /// <summary>
        /// 泊位收箱2
        /// VehicleBerthOperationStatus
        /// </summary>
        BerthReceive2,

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