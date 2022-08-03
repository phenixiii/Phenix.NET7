using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应机械动力事件处理器
    /// </summary>
    public class MachinePowerEventHandler : IIntegrationEventHandler<MachinePowerEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(MachinePowerEvent @event)
        {
            switch (@event.MachineType)
            {
                case MachineType.QuayCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IQuayCraneGrain>(@event.MachineId).OnChangePower(
                        new PowerInfo(@event.PowerType, @event.PowerStatus, @event.SurplusCapacityPercent));
                    break;
                case MachineType.YardCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IYardCraneGrain>(@event.MachineId).OnChangePower(
                        new PowerInfo(@event.PowerType, @event.PowerStatus, @event.SurplusCapacityPercent));
                    break;
                case MachineType.Vehicle:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IVehicleGrain>(@event.MachineId).OnChangePower(
                        new PowerInfo(@event.PowerType, @event.PowerStatus, @event.SurplusCapacityPercent));
                    break;
            }
        }

        #endregion
    }
}