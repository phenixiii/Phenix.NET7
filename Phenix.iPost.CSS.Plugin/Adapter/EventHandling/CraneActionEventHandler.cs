using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 吊车动作事件处理器
    /// </summary>
    public class CraneActionEventHandler : IIntegrationEventHandler<CraneActionEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(CraneActionEvent @event)
        {
            switch (@event.CraneType)
            {
                case CraneType.QuayCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IQuayCraneGrain>(@event.MachineId).OnAction(@event.CraneAction);
                    break;
                case CraneType.YardCrane:
                    await Phenix.Actor.ClusterClient.Default.GetGrain<IYardCraneGrain>(@event.MachineId).OnAction(@event.CraneAction);
                    break;
            }
        }

        #endregion
    }
}