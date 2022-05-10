using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车泊位作业状态
    /// </summary>
    [Serializable]
    public enum VehicleBerthOperationStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        None,

        /// <summary>
        /// 下达
        /// </summary>
        Issued,
        
        /// <summary>
        /// 到达岸桥缓冲区
        /// </summary>
        ArrivedQuayBuffer,

        /// <summary>
        /// 上档
        /// </summary>
        ComeUpToDo,

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