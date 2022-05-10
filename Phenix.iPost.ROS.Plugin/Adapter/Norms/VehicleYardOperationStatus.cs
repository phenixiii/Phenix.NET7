using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车堆场作业状态
    /// </summary>
    [Serializable]
    public enum VehicleYardOperationStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        None,

        /// <summary>
        /// 任务已下达
        /// </summary>
        Issued,

        /// <summary>
        /// 到达堆场围栏
        /// </summary>
        ArrivedFence,

        /// <summary>
        /// 到达目的位置
        /// </summary>
        ArrivedDestination,

        /// <summary>
        /// 锁车
        /// </summary>
        Locked,

        /// <summary>
        /// 离开
        /// </summary>
        Leave,
    }
}