using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应船舶靠泊事件处理器
    /// </summary>
    public class VesselBerthingEventHandler : IIntegrationEventHandler<VesselBerthingEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(VesselBerthingEvent @event)
        {
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVesselGrain>(@event.VesselCode).OnBerthing(
                new VesselBerthing(@event.TerminalCode, @event.BerthNo,
                    Phenix.Core.Reflection.Utilities.ChangeType<VesselBerthingDirection>(@event.BerthingDirection),
                    @event.BowBollardNo, @event.BowBollardOffset, @event.SternBollardNo, @event.SternBollardOffset));
        }

        #endregion
    }
}