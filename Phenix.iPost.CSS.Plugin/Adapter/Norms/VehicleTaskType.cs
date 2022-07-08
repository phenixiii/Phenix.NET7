using System;

namespace Phenix.iPost.CSS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车任务类型
    /// </summary>
    [Serializable]
    public enum VehicleTaskType
    {
        /// <summary>
        /// 泊位
        /// </summary>
        Berth,

        /// <summary>
        /// 堆场
        /// </summary>
        Yard,
        
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
