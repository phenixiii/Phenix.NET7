using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 拖车堆场作业状态
    /// </summary>
    [Serializable]
    public enum VehicleYardOperationStatus
    {
        /// <summary>
        /// 待命
        /// </summary>
        Standby = 0,

        /// <summary>
        /// 任务已下达
        /// </summary>
        Issued = 1,

        /// <summary>
        /// 到达堆场围栏
        /// </summary>
        ArrivedFence = 2,

        /// <summary>
        /// 到达目的位置
        /// </summary>
        ArrivedDestination = 3,

        /// <summary>
        /// 锁车
        /// </summary>
        Locked = 4,

        /// <summary>
        /// 离开
        /// </summary>
        Leave = 5,
    }
}