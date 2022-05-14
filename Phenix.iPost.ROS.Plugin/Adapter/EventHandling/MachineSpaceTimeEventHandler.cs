using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;
using Phenix.iPost.ROS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.ROS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应机械时空事件处理器
    /// </summary>
    public class MachineSpaceTimeEventHandler : IIntegrationEventHandler<MachineSpaceTimeEvent>
    {       
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="logger">日志</param>
        public MachineSpaceTimeEventHandler(ILogger<MachineSpaceTimeEventHandler> logger)
        {
            _logger = logger;
        }

        #region 属性

        private readonly ILogger _logger;

        #endregion

        #region 方法

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件</param>
        public Task Handle(MachineSpaceTimeEvent @event)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
