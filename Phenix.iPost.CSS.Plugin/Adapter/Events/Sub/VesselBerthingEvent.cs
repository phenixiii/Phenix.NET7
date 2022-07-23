using System;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 船舶靠泊事件
    /// </summary>
    [Serializable]
    public record VesselBerthingEvent : IntegrationEvent
    {
        /// <summary>
        /// 船舶靠泊事件
        /// </summary>
        /// <param name="vesselCode">船舶代码</param>
        /// <param name="terminalCode">码头代码</param>
        /// <param name="berthNo">泊位号（从小到大坐标排序）</param>
        /// <param name="berthingDirection">靠泊方向</param>
        /// <param name="bowBollardNo">船头缆桩号</param>
        /// <param name="bowBollardOffset">船头缆桩偏差值cm</param>
        /// <param name="sternBollardNo">船尾缆桩号</param>
        /// <param name="sternBollardOffset">船尾缆桩偏差值cm</param>
        [Newtonsoft.Json.JsonConstructor]
        public VesselBerthingEvent(string vesselCode, string terminalCode, long berthNo,
            VesselBerthingDirection berthingDirection, long bowBollardNo, int bowBollardOffset, long sternBollardNo, int sternBollardOffset)
        {
            this.VesselCode = vesselCode;
            this.TerminalCode = terminalCode;
            this.BerthNo = berthNo;
            this.BerthingDirection = berthingDirection;
            this.BowBollardNo = bowBollardNo;
            this.BowBollardOffset = bowBollardOffset;
            this.SternBollardNo = sternBollardNo;
            this.SternBollardOffset = sternBollardOffset;
        }

        #region 属性

        /// <summary>
        /// 船舶代码
        /// </summary>
        public string VesselCode { get; }

        /// <summary>
        /// 码头代码
        /// </summary>
        public string TerminalCode { get; }

        /// <summary>
        /// 泊位号（从小到大坐标排序）
        /// </summary>
        public long BerthNo { get; }

        /// <summary>
        /// 靠泊方向
        /// </summary>
        public VesselBerthingDirection BerthingDirection { get; }

        /// <summary>
        /// 船头缆桩号
        /// </summary>
        public long BowBollardNo { get; }

        /// <summary>
        /// 船头缆桩偏差值cm
        /// </summary>
        public int BowBollardOffset { get; }

        /// <summary>
        /// 船尾缆桩号
        /// </summary>
        public long SternBollardNo { get; }

        /// <summary>
        /// 船尾缆桩偏差值cm
        /// </summary>
        public int SternBollardOffset { get; }

        #endregion
    }
}