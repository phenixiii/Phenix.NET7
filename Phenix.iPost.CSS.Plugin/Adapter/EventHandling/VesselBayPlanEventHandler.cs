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
            Dictionary<int, ContainerProperty> bayPlan = new Dictionary<int, ContainerProperty>(@event.BayPlan.Count);
            foreach (Phenix.iPost.CSS.Plugin.Adapter.Property.BayPlanContainerProperty item in @event.BayPlan)
                bayPlan.Add(item.BayNo, new ContainerProperty(
                    item.ContainerNumber, item.ContainerOwner, @event.Voyage, item.LadingBillNumber,
                    Phenix.Core.Reflection.Utilities.ChangeType<ImportExport>(item.ImportExport),
                    item.LoadingPort, item.DischargingPort, item.DestinationPort, item.TransferPort,
                    item.ContainerType, item.ContainerSize, item.IsoCode, item.Weight,
                    item.OverHeight, item.OverFrontLength, item.OverBackLength, item.OverLeftWidth, item.OverRightWidth,
                    Phenix.Core.Reflection.Utilities.ChangeType<EmptyFull>(item.EmptyFull), item.IsRefrige, item.DangerousCode,
                    item.BayNo, item.RowNo, item.TierNo
                ));
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVesselGrain>(@event.VesselCode).SetBayPlan(@event.Voyage, bayPlan);
        }

        #endregion
    }
}