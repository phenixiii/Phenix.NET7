using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 船舶靠泊
    /// </summary>
    [Serializable]
    public readonly record struct VesselBerthingInfo
    {
        /// <summary>
        /// 船舶靠泊
        /// </summary>
        /// <param name="terminalCode">码头代码</param>
        /// <param name="berthNo">泊位</param>
        /// <param name="planBerthingTime">计划靠泊时间</param>
        /// <param name="planDepartureTime">计划离泊时间</param>
        /// <param name="berthingDirection">靠泊方向</param>
        /// <param name="bowBollardNo">船头缆桩号</param>
        /// <param name="bowBollardOffset">船头缆桩偏差值cm</param>
        /// <param name="sternBollardNo">船尾缆桩号</param>
        /// <param name="sternBollardOffset">船尾缆桩偏差值cm</param>
        [Newtonsoft.Json.JsonConstructor]
        public VesselBerthingInfo(string terminalCode, long berthNo,
            DateTime planBerthingTime, DateTime planDepartureTime,
            VesselBerthingDirection berthingDirection, long bowBollardNo, int bowBollardOffset, long sternBollardNo, int sternBollardOffset)
        {
            this.TerminalCode = terminalCode;
            this.BerthNo = berthNo;
            this.PlanBerthingTime = planBerthingTime;
            this.PlanDepartureTime = planDepartureTime;
            this.BerthingDirection = berthingDirection;
            this.BowBollardNo = bowBollardNo;
            this.BowBollardOffset = bowBollardOffset;
            this.SternBollardNo = sternBollardNo;
            this.SternBollardOffset = sternBollardOffset;
        }

        #region 属性

        /// <summary>
        /// 码头代码
        /// </summary>
        public string TerminalCode { get; }

        /// <summary>
        /// 泊位号（从小到大坐标排序）
        /// </summary>
        public long BerthNo { get; }

        /// <summary>
        /// 计划靠泊时间
        /// </summary>
        public DateTime PlanBerthingTime { get; }

        /// <summary>
        /// 计划离泊时间
        /// </summary>
        public DateTime PlanDepartureTime { get; }

        /// <summary>
        /// 靠泊方向
        /// </summary>
        public VesselBerthingDirection BerthingDirection { get; }

        /// <summary>
        /// 船头缆桩号（从小到大坐标排序）
        /// </summary>
        public long BowBollardNo { get; }

        /// <summary>
        /// 船头缆桩偏差值cm
        /// </summary>
        public int BowBollardOffset { get; }

        /// <summary>
        /// 船尾缆桩号（从小到大坐标排序）
        /// </summary>
        public long SternBollardNo { get; }

        /// <summary>
        /// 船尾缆桩偏差值cm
        /// </summary>
        public int SternBollardOffset { get; }

        #endregion
    }
}