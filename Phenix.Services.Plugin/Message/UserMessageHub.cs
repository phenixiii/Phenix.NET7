﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Phenix.Core.Security;
using Phenix.Services.Contract.Message;

namespace Phenix.Services.Plugin.Message
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
            if (Principal.CurrentIdentity != null)
                _pusher.AddConnectedInfo(Principal.CurrentIdentity.PrimaryKey, Context.ConnectionId);
            if (_service != null)
                await _service.OnConnected(Principal.CurrentIdentity, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Principal.CurrentIdentity != null)
                _pusher.RemoveConnectedInfo(Principal.CurrentIdentity.PrimaryKey);
            if (_service != null)
                await _service.OnDisconnected(Principal.CurrentIdentity, Context.ConnectionId, exception);
            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}