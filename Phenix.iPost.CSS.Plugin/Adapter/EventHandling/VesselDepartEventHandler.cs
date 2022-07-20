using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应船舶离泊事件处理器
    /// </summary>
    public class VesselDepartEventHandler : IIntegrationEventHandler<VesselDepartEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(VesselDepartEvent @event)
        {
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVesselGrain>(@event.VesselCode).OnDepart(@event.TerminalCode, @event.Voyage);
        }

        #endregion
    }
}