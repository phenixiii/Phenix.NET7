using System.Threading.Tasks;

namespace Phenix.Core.Event
{
    /// <summary>
    /// 事件总线接口
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="integrationEvent">事件</param>
        Task PublishAsync(IntegrationEvent integrationEvent);
    }
}
