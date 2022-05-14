using System.Threading.Tasks;

namespace Phenix.Core.Event
{
    /// <summary>
    /// 事件处理器接口
    /// </summary>
    /// <typeparam name="T">事件</typeparam>
    public interface IIntegrationEventHandler<in T> : IIntegrationEventHandler
        where T : IntegrationEvent
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        Task Handle(T @event);
    }

    /// <summary>
    /// 事件处理器接口
    /// </summary>
    public interface IIntegrationEventHandler
    {
    }
}
