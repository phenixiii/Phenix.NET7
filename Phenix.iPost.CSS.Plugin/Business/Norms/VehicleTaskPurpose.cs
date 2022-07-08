using System;

namespace Phenix.iPost.CSS.Plugin.Business.Norms
{
    /// <summary>
    /// 拖车任务目的
    /// </summary>
    [Serializable]
    public enum VehicleTaskPurpose
    {
        /// <summary>
        /// 收货
        /// </summary>
        Receive,

        /// <summary>
        /// 送货
        /// </summary>
        Deliver,

        /// <summary>
        /// 其他
        /// </summary>
        Other,
    }
}