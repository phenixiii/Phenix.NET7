using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Phenix.Core.Message;
using Phenix.Core.Security;

namespace Phenix.Services.Plugin.Api.Message
{
    /// <summary>
    /// 用户消息中间件
    /// </summary>
    [Authorize]
    [EnableCors]
    public sealed class UserMessageHub : Hub
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="service">注入消息服务</param>
        public UserMessageHub(IHubContext<UserMessageHub> context, IMessageService service)
        {
            _pusher = new UserMessageHubPusher(context);
            _service = service;
        }

        #region 属性

        private readonly UserMessageHubPusher _pusher;
        private readonly IMessageService _service;

        #endregion

        #region 方法

        /// <summary>
        /// 连接
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            if (Identity.CurrentIdentity != null)
                _pusher.AddConnectedInfo(Identity.CurrentIdentity.Name, Context.ConnectionId);
            if (_service != null)
                _service.OnConnected(Identity.CurrentIdentity, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Identity.CurrentIdentity != null)
                _pusher.RemoveConnectedInfo(Identity.CurrentIdentity.Name);
            if (_service != null)
                _service.OnDisconnected(Identity.CurrentIdentity, Context.ConnectionId, exception);
            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}