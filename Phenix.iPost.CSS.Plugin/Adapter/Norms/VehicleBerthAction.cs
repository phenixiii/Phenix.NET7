using System;

namespace Phenix.iPost.CSS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 拖车泊位动作
    /// </summary>
    [Serializable]
    public enum VehicleBerthAction
    {
        /// <summary>
        /// 待命
        /// </summary>
        Standby,

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
        ComeUp,

        /// <summary>
        /// 已就位
        /// </summary>
        InPlace,

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