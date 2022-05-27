using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 拖车泊位作业状态
    /// </summary>
    [Serializable]
    public enum VehicleBerthOperationStatus
    {
        /// <summary>
        /// 待命
        /// </summary>
        Standby = 0,

        /// <summary>
        /// 下达
        /// </summary>
        Issued = 1,

        /// <summary>
        /// 到达桥吊缓冲区
        /// </summary>
        ArrivedQuayBuffer = 2,

        /// <summary>
        /// 上档
        /// </summary>
        ComeUpToDo = 3,

        /// <summary>
        /// 到达目的位置
        /// </summary>
        ArrivedDestination = 4,

        /// <summary>
        /// 锁车
        /// </summary>
        Locked = 5,

        /// <summary>
        /// 离开
        /// </summary>
        Leave = 6,
    }
}