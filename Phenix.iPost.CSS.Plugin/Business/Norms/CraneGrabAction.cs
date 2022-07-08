using System;

namespace Phenix.iPost.CSS.Plugin.Business.Norms
{
    /// <summary>
    /// 吊车抓具动作
    /// </summary>
    [Serializable]
    public enum CraneGrabAction
    {
        /// <summary>
        /// 其他
        /// </summary>
        Other,

        /// <summary>
        /// 已落
        /// </summary>
        Landed,

        /// <summary>
        /// 抓住
        /// </summary>
        Grasp,

        /// <summary>
        /// 抓起
        /// </summary>
        PickUp,

        /// <summary>
        /// 放下
        /// </summary>
        PickDown,

        /// <summary>
        /// 释放
        /// </summary>
        Release,
    }
}
