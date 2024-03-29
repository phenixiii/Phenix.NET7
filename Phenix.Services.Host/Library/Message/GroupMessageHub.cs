﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Log;
using Phenix.Core.SyncCollections;
using Phenix.Services.Contract;

namespace Phenix.Services.Host.Library.Message
{
    /// <summary>
    /// 分组消息中间件
    /// </summary>
    [Authorize]
    [EnableCors]
    public sealed class GroupMessageHub : Hub
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public GroupMessageHub()
        {
        }

        #region 属性
        
        private readonly SynchronizedDictionary<string, SynchronizedList<string>> _connectedInfos = 
            new SynchronizedDictionary<string, SynchronizedList<string>>(StringComparer.Ordinal);

        private readonly SynchronizedDictionary<string, Subscriber> _subscribers =
            new SynchronizedDictionary<string, Subscriber>(StringComparer.Ordinal);

        #endregion

        #region 内嵌类

        private class Subscriber
        {
            public Subscriber(GroupMessageHub owner, string groupName)
            {
                _owner = owner;
                _groupName = groupName;
            }

            #region 属性

            private readonly GroupMessageHub _owner;
            private readonly string _groupName;

            #endregion

            #region 方法

            public async Task SubscribeAsync(IAsyncStream<string> stream)
            {
                IList<StreamSubscriptionHandle<string>> streamHandles = await stream.GetAllSubscriptionHandles();
                if (streamHandles != null && streamHandles.Count > 0)
                    foreach (StreamSubscriptionHandle<string> item in streamHandles)
                        await item.ResumeAsync(OnReceiving, OnSubscribeError);
                else
                    await stream.SubscribeAsync(OnReceiving, OnSubscribeError);
            }

            private async Task OnReceiving(string content, StreamSequenceToken token)
            {
                await _owner.Clients.Group(_groupName).SendAsync("onReceived", content);
            }

            private Task OnSubscribeError(Exception error)
            {
                LogHelper.Error(error, "{@UserMessageGroup}", _groupName);
                return Task.CompletedTask;
            }

            #endregion
        }

        #endregion

        #region 方法

        /// <summary>
        /// 连接
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="groupName">组名</param>
        public async Task SubscribeAsync(string groupName)
        {
            _connectedInfos.GetValue(Context.ConnectionId, () => new SynchronizedList<string>()).AddOnce(groupName);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _subscribers.GetValue(groupName, async () =>
            {
                Subscriber subscriber = new Subscriber(this, groupName);
                await subscriber.SubscribeAsync(ClusterClient.Default.GetSimpleMessageStreamProvider().GetStream<string>(MessageStreamIds.GroupStreamId, groupName));
                return subscriber;
            });
        }

        /// <summary>
        /// 断开
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_connectedInfos.TryGetValue(Context.ConnectionId, out SynchronizedList<string> groupNames))
                foreach (string groupName in groupNames)
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
           
            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}