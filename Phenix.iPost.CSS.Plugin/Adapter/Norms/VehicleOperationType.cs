using System;

namespace Phenix.iPost.CSS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车作业类型
    /// </summary>
    [Serializable]
    public enum VehicleOperationType
    {
        /// <summary>
        /// 卸船
        /// </summary>
        Discharge,

        /// <summary>
        /// 装船
        /// </summary>
        Shipment,

        /// <summary>
        /// 转堆
        /// </summary>
        Shift,

        /// <summary>
        /// 泊车
        /// </summary>
        Park,

        /// <summary>
        /// 维修
        /// </summary>
        Maintenance,

        /// <summary>
        /// 换电
        /// </summary>
        Fueling,
    }
}
