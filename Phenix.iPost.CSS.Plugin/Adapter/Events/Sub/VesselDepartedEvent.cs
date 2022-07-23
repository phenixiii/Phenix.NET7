using System;
using Phenix.Core.Event;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 船舶离泊事件
    /// </summary>
    [Serializable]
    public record VesselDepartedEvent : IntegrationEvent
    {
        /// <summary>
        /// 船舶离泊事件
        /// </summary>
        /// <param name="vesselCode">船舶代码</param>
        [Newtonsoft.Json.JsonConstructor]
        public VesselDepartedEvent(string vesselCode)
        {
            this.VesselCode = vesselCode;
        }

        #region 属性

        /// <summary>
        /// 船舶代码
        /// </summary>
        public string VesselCode { get; }

        #endregion
    }
}