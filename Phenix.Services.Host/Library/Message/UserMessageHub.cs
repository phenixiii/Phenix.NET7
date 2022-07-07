using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Phenix.Core.Security;

namespace Phenix.Services.Host.Library.Message
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
        public UserMessageHub(IHubContext<UserMessageHub> context)
        {
            _pusher = new UserMessageHubPusher(context);
        }

        #region 属性

        private readonly UserMessageHubPusher _pusher;

        #endregion

        #region 方法

        /// <summary>
        /// 连接
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            if (Principal.CurrentIdentity != null)
                _pusher.AddConnectedInfo(Principal.CurrentIdentity.PrimaryKey, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Principal.CurrentIdentity != null)
                _pusher.RemoveConnectedInfo(Principal.CurrentIdentity.PrimaryKey);

            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}