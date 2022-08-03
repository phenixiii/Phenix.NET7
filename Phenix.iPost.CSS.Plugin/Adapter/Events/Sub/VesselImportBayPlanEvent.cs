using System;
using System.Collections.Generic;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 进口船图事件
    /// </summary>
    [Serializable]
    public record VesselImportBayPlanEvent : IntegrationEvent
    {
        /// <summary>
        /// 进口船图事件
        /// </summary>
        /// <param name="vesselCode">船舶代码</param>
        /// <param name="info">贝位-排号-叠箱</param>
        [Newtonsoft.Json.JsonConstructor]
        public VesselImportBayPlanEvent(string vesselCode,
            IDictionary<int, IDictionary<int, IList<ContainerInfo>>> info)
        {
            this.VesselCode = vesselCode;
            this.Info = info;
        }

        #region 属性

        /// <summary>
        /// 船舶代码
        /// </summary>
        public string VesselCode { get; }

        /// <summary>
        /// 贝位-排号-叠箱
        /// </summary>
        public IDictionary<int, IDictionary<int, IList<ContainerInfo>>> Info { get; }

        #endregion
    }
}