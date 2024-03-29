﻿using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Phenix.Actor;
using Phenix.Core.Log;
using Phenix.Core.Reflection;
using Phenix.Core.SyncCollections;

namespace Phenix.Services.Host.Library.Message
{
    internal sealed class UserMessageHubPusher
    {
        public UserMessageHubPusher(IHubContext<UserMessageHub> context)
        {
            _context = context;
        }

        #region 属性

        private readonly IHubContext<UserMessageHub> _context;

        private readonly SynchronizedDictionary<string, ConnectedInfo> _connectedInfos = 
            new SynchronizedDictionary<string, ConnectedInfo>(StringComparer.Ordinal);
        private Thread _taskThread;
        
        #endregion

        #region 内嵌类

        private class ConnectedInfo
        {
            public ConnectedInfo(string connectionId)
            {
                _connectionId = connectionId;
            }

            private readonly string _connectionId;

            public string ConnectionId
            {
                get { return _connectionId; }
            }

            private readonly SynchronizedDictionary<long, DateTime> _sentDateTimes = new SynchronizedDictionary<long, DateTime>();

            public IDictionary<long, DateTime> SentDateTimes
            {
                get { return _sentDateTimes; }
            }

            private DateTime _lastActionTime = DateTime.Now;

            public DateTime LastActionTime
            {
                get { return _lastActionTime; }
                set { _lastActionTime = value; }
            }
        }

        #endregion

        #region 方法

        public void AddConnectedInfo(string key, string connectionId)
        {
            InitializeTask();

            _connectedInfos[key] = new ConnectedInfo(connectionId);
        }

        public void RemoveConnectedInfo(string key)
        {
            InitializeTask();

            _connectedInfos.Remove(key);
        }

        private void InitializeTask()
        {
            if (_taskThread == null)
                lock (_connectedInfos)
                    if (_taskThread == null)
                    {
                        _taskThread = new Thread(ExecuteTask);
                        _taskThread.IsBackground = true;
                        _taskThread.Start();
                    }

            Thread.MemoryBarrier();
        }

        private void ExecuteTask()
        {
            try
            {
                while (true)
                    try
                    {
                        foreach (KeyValuePair<string, ConnectedInfo> connectedInfo in _connectedInfos)
                        {
                            if (connectedInfo.Value.SentDateTimes.Count == 0 && DateTime.Now.Subtract(connectedInfo.Value.LastActionTime).TotalSeconds <= 1)
                                continue;
                            IDictionary<long, UserMessage> receivedMessages = ClusterClient.Default.GetGrain<IUserMessageGrain>(connectedInfo.Key).Receive().Result;
                            if (receivedMessages.Count > 0)
                            {
                                Dictionary<long, UserMessage> packages = new Dictionary<long, UserMessage>(receivedMessages.Count);
                                foreach (KeyValuePair<long, UserMessage> receivedMessage in receivedMessages)
                                {
                                    if (connectedInfo.Value.SentDateTimes.TryGetValue(receivedMessage.Key, out DateTime sentDateTime) && DateTime.Now.Subtract(sentDateTime).TotalMinutes <= 1)
                                        continue;
                                    packages[receivedMessage.Key] = receivedMessage.Value;
                                    connectedInfo.Value.SentDateTimes[receivedMessage.Key] = DateTime.Now;
                                }

                                if (packages.Count > 0)
                                {
                                    IClientProxy clientProxy = _context.Clients.Client(connectedInfo.Value.ConnectionId);
                                    if (clientProxy != null)
                                        clientProxy.SendAsync("onReceived", Utilities.JsonSerialize(packages));
                                }
                            }
                            else
                            {
                                connectedInfo.Value.SentDateTimes.Clear();
                                connectedInfo.Value.LastActionTime = DateTime.Now;
                            }
                        }

                        Thread.Sleep(100);
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex, "{@UserMessageHubConnections}", _connectedInfos);
                        Thread.Sleep(1000 * 60);
                    }
            }
            finally
            {
                _taskThread = null;
            }
        }

        #endregion
    }
}