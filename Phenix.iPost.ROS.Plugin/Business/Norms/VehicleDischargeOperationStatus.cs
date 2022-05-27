using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 拖车卸船作业状态
    /// </summary>
    [Serializable]
    public enum VehicleDischargeOperationStatus
    {
        /// <summary>
        /// 开始
        /// </summary>
        Start = 0,

        /// <summary>
        /// 泊位收箱1
        /// VehicleBerthOperationStatus
        /// </summary>
        BerthReceive1 = 1,

        /// <summary>
        /// 泊位收箱2
        /// VehicleBerthOperationStatus
        /// </summary>
        BerthReceive2 = 2,

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