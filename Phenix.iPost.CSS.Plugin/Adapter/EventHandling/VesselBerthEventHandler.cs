using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;
using Phenix.iPost.CSS.Plugin.Business.Norms;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应船舶靠泊事件处理器
    /// </summary>
    public class VesselBerthEventHandler : IIntegrationEventHandler<VesselBerthEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(VesselBerthEvent @event)
        {
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVesselGrain>(@event.VesselCode).OnBerth(@event.TerminalCode, @event.Voyage,
                new VesselAlongSideProperty(Phenix.Core.Reflection.Utilities.ChangeType<VesselAlongSide>(@event.AlongSide),
                    @event.BowBollardId, @event.BowBollardOffset, @event.SternBollardId, @event.SternBollardOffset));
        }

        #endregion
    }
}