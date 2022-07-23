﻿using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应船舶预配船图事件处理器
    /// </summary>
    public class VesselPreBayPlanEventHandler : IIntegrationEventHandler<VesselPreBayPlanEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(VesselPreBayPlanEvent @event)
        {
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVesselGrain>(@event.VesselCode).OnRefreshPreBayPlan(@event.Info);
        }

        #endregion
    }
}