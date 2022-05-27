using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 拖车转堆作业状态
    /// </summary>
    [Serializable]
    public enum VehicleShiftOperationStatus
    {
        /// <summary>
        /// 开始
        /// </summary>
        Start = 0,

        /// <summary>
        /// 堆场收箱1
        /// VehicleYardOperationStatus
        /// </summary>
        YardReceive1 = 1,

        /// <summary>
        /// 堆场收箱2
        /// VehicleYardOperationStatus
        /// </summary>
        YardReceive2 = 2,

        /// <summary>
        /// 堆场送箱1
        /// VehicleYardOperationStatus
        /// </summary>
        YardDeliver1 = 3,

        /// <summary>
        /// 堆场送箱2
        /// VehicleYardOperationStatus
        /// </summary>
        YardDeliver2 = 4,
    }
}