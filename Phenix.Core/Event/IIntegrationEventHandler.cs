using System.Threading.Tasks;

namespace Phenix.Core.Event
{
    /// <summary>
    /// 事件总线处理器
    /// </summary>
    /// <typeparam name="TIntegrationEvent">事件包</typeparam>
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="event">事件包</param>
        Task Handle(TIntegrationEvent @event);
    }

    /// <summary>
    /// 事件总线处理器接口
    /// </summary>
    public interface IIntegrationEventHandler
    {
    }
}
