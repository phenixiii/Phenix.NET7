using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车装船作业状态
    /// </summary>
    [Serializable]
    public enum VehicleShipmentOperationStatus
    {
        /// <summary>
        /// 堆场收箱
        /// YardOperationStatus
        /// </summary>
        YardReceiveF,

        /// <summary>
        /// 堆场收箱
        /// YardOperationStatus
        /// </summary>
        YardReceiveA,

        /// <summary>
        /// 泊位送箱
        /// BerthOperationStatus
        /// </summary>
        BerthDeliverF,

        /// <summary>
        /// 泊位送箱
        /// BerthOperationStatus
        /// </summary>
        BerthDeliverA,
    }
}