using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车转堆作业状态
    /// </summary>
    [Serializable]
    public enum VehicleShiftOperationStatus
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