using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应拖车泊位动作事件处理器
    /// </summary>
    public class VehicleBerthActionEventHandler : IIntegrationEventHandler<VehicleBerthActionEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(VehicleBerthActionEvent @event)
        {
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVehicleGrain>(@event.MachineId).OnAction(@event.BerthAction);
        }

        #endregion
    }
}
