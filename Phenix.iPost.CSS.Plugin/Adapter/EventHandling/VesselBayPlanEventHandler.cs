using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;
using Phenix.iPost.CSS.Plugin.Business.Norms;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应船舶进口船图事件处理器
    /// </summary>
    public class VesselBayPlanEventHandler : IIntegrationEventHandler<VesselBayPlanEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(VesselBayPlanEvent @event)
        {
            Dictionary<int, BayPlanContainerProperty> bayPlan = new Dictionary<int, BayPlanContainerProperty>(@event.BayPlan.Count);
            foreach (Phenix.iPost.CSS.Plugin.Adapter.Property.BayPlanContainerProperty item in @event.BayPlan)
                bayPlan.Add(item.Bay, new BayPlanContainerProperty(item.Bay, item.Row, item.Tier,
                    item.ContainerNumber, item.ContainerOwner, item.LadingBillNumber,
                    Phenix.Core.Reflection.Utilities.ChangeType<EmptyFull>(item.EmptyFull),
                    Phenix.Core.Reflection.Utilities.ChangeType<ImportExport>(item.ImportExport),
                    item.LoadingPort, item.DischargingPort, item.DestinationPort, item.TransferPort,
                    item.ContainerType, item.ContainerSize, item.IsoCode, item.Weight,
                    item.OverHeight, item.OverFrontLength, item.OverBackLength, item.OverLeftWidth, item.OverRightWidth,
                    item.Refrigerated, item.DangerousCode
                ));
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVesselGrain>(@event.VesselCode).SetBayPlan(@event.Voyage, bayPlan);
        }

        #endregion
    }
}