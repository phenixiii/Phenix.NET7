using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 动力
    /// </summary>
    [Serializable]
    public readonly record struct PowerInfo
    {
        /// <summary>
        /// 动力
        /// </summary>
        /// <param name="powerType">动力类型</param>
        /// <param name="powerStatus">动力状态</param>
        /// <param name="surplusCapacityPercent">剩余容量百分比</param>
        [Newtonsoft.Json.JsonConstructor]
        public PowerInfo(PowerType powerType, PowerStatus powerStatus, int? surplusCapacityPercent)
        {
            this.PowerType = powerType;
            this.PowerStatus = powerStatus;
            this.SurplusCapacityPercent = surplusCapacityPercent;
        }

        #region 属性

        /// <summary>
        /// 动力类型
        /// </summary>
        public PowerType PowerType { get; }

        /// <summary>
        /// 动力状态
        /// </summary>
        public PowerStatus PowerStatus { get; }

        /// <summary>
        /// 剩余容量百分比
        /// </summary>
        public int? SurplusCapacityPercent { get; }

        #endregion
    }
}