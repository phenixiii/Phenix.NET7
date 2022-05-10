using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 吊车吊具动作
    /// </summary>
    [Serializable]
    public enum CraneSpreaderAction
    {
        /// <summary>
        /// 其他/舱盖板模式（HatchCoverMode）
        /// </summary>
        Other,

        /// <summary>
        /// 着箱
        /// </summary>
        Landed,

        /// <summary>
        /// 闭锁
        /// </summary>
        Locked,

        /// <summary>
        /// 已抓箱
        /// </summary>
        PickUp,

        /// <summary>
        /// 已放箱
        /// </summary>
        Grounded,

        /// <summary>
        /// 开锁
        /// </summary>
        UnLock,
    }
}
