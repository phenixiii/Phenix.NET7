﻿using System.Threading.Tasks;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.CSS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应拖车任务事件处理器
    /// </summary>
    public class MachineTaskAckEventHandler : IIntegrationEventHandler<MachineTaskAckEvent>
    {
        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public async Task Handle(MachineTaskAckEvent @event)
        {
            await Phenix.Actor.ClusterClient.Default.GetGrain<IVehicleGrain>(@event.MachineId).OnTaskAck(
                Phenix.Core.Reflection.Utilities.ChangeType<Phenix.iPost.CSS.Plugin.Business.Norms.TaskStatus>(@event.TaskStatus));
        }

        #endregion
    }
}