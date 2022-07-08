using System;
using System.Collections.Generic;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Property;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 进口船图事件
    /// </summary>
    /// <param name="VesselCode">船舶代码（全域唯一）</param>
    /// <param name="Voyage">航次</param>
    /// <param name="BayPlan">船图</param>
    [Serializable]
    public record VesselBayPlanEvent(
            string VesselCode,
            string Voyage,
            List<BayPlanContainerProperty> BayPlan)
        : IntegrationEvent;
}