using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车卸船作业状态
    /// </summary>
    [Serializable]
    public enum VehicleDischargeOperationStatus
    {
        /// <summary>
        /// 泊位收箱
        /// BerthOperationStatus
        /// </summary>
        BerthReceiveF,

        /// <summary>
        /// 泊位收箱
        /// BerthOperationStatus
        /// </summary>
        BerthReceiveA,

        /// <summary>
        /// 堆场送箱
        /// YardOperationStatus
        /// </summary>
        YardDeliverF,

        /// <summary>
        /// 堆场送箱
        /// YardOperationStatus
        /// </summary>
        YardDeliverA,
    }
}