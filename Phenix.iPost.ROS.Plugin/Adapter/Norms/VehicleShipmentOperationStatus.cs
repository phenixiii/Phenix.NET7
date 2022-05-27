﻿using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车装船作业状态
    /// </summary>
    [Serializable]
    public enum VehicleShipmentOperationStatus
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
        /// 泊位送箱1
        /// VehicleBerthOperationStatus
        /// </summary>
        BerthDeliver1,

        /// <summary>
        /// 泊位送箱2
        /// VehicleBerthOperationStatus
        /// </summary>
        BerthDeliver2,
    }
}