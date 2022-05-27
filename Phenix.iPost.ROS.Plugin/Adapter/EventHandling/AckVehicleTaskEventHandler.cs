﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;
using Phenix.iPost.ROS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.ROS.Plugin.Adapter.EventHandling
{
    /// <summary>
    /// 响应拖车任务事件处理器
    /// </summary>
    public class AckVehicleTaskEventHandler : IIntegrationEventHandler<AckVehicleTaskEvent>
    {       
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="logger">日志</param>
        public AckVehicleTaskEventHandler(ILogger<AckVehicleTaskEventHandler> logger)
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
        public Task Handle(AckVehicleTaskEvent @event)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}