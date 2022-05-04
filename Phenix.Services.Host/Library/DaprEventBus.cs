using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;

namespace Phenix.Services.Host.Library
{
    /// <summary>
    /// Dapr事件总线
    /// </summary>
    public class DaprEventBus : IEventBus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public DaprEventBus(DaprClient client, ILogger<DaprEventBus> logger)
        {
            _client = client;
            _logger = logger;
        }

        #region 属性
        
        private readonly DaprClient _client;
        private readonly ILogger _logger;

        #endregion

        #region 方法


        Task IEventBus.PublishAsync(IntegrationEvent @event)
        {
            string topicName = @event.GetType().Name;

            _logger.LogInformation("Publishing event {0} to {1}.{2}", @event, EventConfig.PubSubName, topicName);

            return _client.PublishEventAsync(EventConfig.PubSubName, topicName, (object) @event);
        }

        #endregion
    }
}
