using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车任务类型
    /// </summary>
    [Serializable]
    public enum VehicleTaskType
    {
        /// <summary>
        /// 卸船作业
        /// </summary>
        DischargeOperation,

        /// <summary>
        /// 装船作业
        /// </summary>
        ShipmentOperation,

        /// <summary>
        /// 转堆作业
        /// </summary>
        ShiftOperation,

        /// <summary>
        /// 停车
        /// </summary>
        Park,

        /// <summary>
        /// 维修
        /// </summary>
        Maintenance,

        /// <summary>
        /// 充电
        /// </summary>
        Fueling,
    }
}
