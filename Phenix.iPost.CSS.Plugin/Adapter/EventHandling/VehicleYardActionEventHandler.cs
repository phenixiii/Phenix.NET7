using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应拖车堆场动作事件处理器
    /// </summary>
    public class VehicleYardActionEventHandler : IIntegrationEventHandler<VehicleYardActionEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(VehicleYardActionEvent @event)
        {
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVehicleGrain>(@event.MachineId).OnAction(@event.YardAction);
        }

        #endregion
    }
}
