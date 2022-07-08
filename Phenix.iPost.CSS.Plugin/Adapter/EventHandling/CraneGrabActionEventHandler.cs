using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应吊车抓具动作事件处理器
    /// </summary>
    public class CraneGrabActionEventHandler : IIntegrationEventHandler<CraneGrabActionEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(CraneGrabActionEvent @event)
        {
            switch (@event.CraneType)
            {
                case Phenix.iPost.CSS.Plugin.Adapter.Norms.CraneType.QuayCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IQuayCraneGrain>(@event.MachineId).OnAction(
                        Phenix.Core.Reflection.Utilities.ChangeType<CraneGrabAction>(@event.GrabAction), @event.HoistHeight);
                    break;
                case Phenix.iPost.CSS.Plugin.Adapter.Norms.CraneType.YardCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IYardCraneGrain>(@event.MachineId).OnAction(
                        Phenix.Core.Reflection.Utilities.ChangeType<CraneGrabAction>(@event.GrabAction), @event.HoistHeight);
                    break;
            }
        }

        #endregion
    }
}
