using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应机械时空事件处理器
    /// </summary>
    public class MachineSpaceTimeEventHandler : IIntegrationEventHandler<MachineSpaceTimeEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(MachineSpaceTimeEvent @event)
        {
            switch (@event.MachineType)
            {
                case Phenix.iPost.CSS.Plugin.Adapter.Norms.MachineType.QuayCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IQuayCraneGrain>(@event.MachineId).OnMoving(
                        new SpaceTimeInfo(@event.X, @event.Y, @event.Location, @event.Speed, @event.Longitude, @event.Latitude, @event.Heading));
                    break;
                case Phenix.iPost.CSS.Plugin.Adapter.Norms.MachineType.YardCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IYardCraneGrain>(@event.MachineId).OnMoving(
                        new SpaceTimeInfo(@event.X, @event.Y, @event.Location, @event.Speed, @event.Longitude, @event.Latitude, @event.Heading));
                    break;
                case Phenix.iPost.CSS.Plugin.Adapter.Norms.MachineType.Vehicle:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IVehicleGrain>(@event.MachineId).OnMoving(
                        new SpaceTimeInfo(@event.X, @event.Y, @event.Location, @event.Speed, @event.Longitude, @event.Latitude, @event.Heading));
                    break;
            }
        }

        #endregion
    }
}