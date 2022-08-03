using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应机械状态事件处理器
    /// </summary>
    public class MachineStatusEventHandler : IIntegrationEventHandler<MachineStatusEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(MachineStatusEvent @event)
        {
            switch (@event.MachineType)
            {
                case MachineType.QuayCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IQuayCraneGrain>(@event.MachineId).OnChangeStatus(
                        new MachineStatusInfo(@event.MachineStatus, @event.TechnicalStatus));
                    break;
                case MachineType.YardCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IYardCraneGrain>(@event.MachineId).OnChangeStatus(
                        new MachineStatusInfo(@event.MachineStatus, @event.TechnicalStatus));
                    break;
                case MachineType.Vehicle:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IVehicleGrain>(@event.MachineId).OnChangeStatus(
                        new MachineStatusInfo(@event.MachineStatus, @event.TechnicalStatus));
                    break;
            }
        }

        #endregion
    }
}