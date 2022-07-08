using System;

namespace Phenix.iPost.CSS.Plugin.Business.Norms
{
    /// <summary>
    /// 拖车堆场动作
    /// </summary>
    [Serializable]
    public enum VehicleYardAction
    {
        /// <summary>
        /// 待命
        /// </summary>
        Standby,

        /// <summary>
        /// 任务已下达
        /// </summary>
        Issued,

        /// <summary>
        /// 到达堆场围栏
        /// </summary>
        ArrivedFence,

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