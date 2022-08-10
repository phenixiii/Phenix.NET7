using System.Threading.Tasks;
using Dapr.Client;
using Phenix.Core.Event;

namespace Phenix.Services.Host.Library
{
    /// <summary>
    /// Dapr事件总线
    /// </summary>
    public sealed class DaprEventBus : IEventBus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public DaprEventBus(DaprClient client)
        {
            _client = client;
        }

        #region 属性

        private readonly DaprClient _client;

        #endregion

        #region 方法

        Task IEventBus.PublishAsync(IntegrationEvent @event)
        {
            return _client.PublishEventAsync(IntegrationEvent.PubSubName, @event.EventName, (object)@event);
        }

        #endregion
    }
}
