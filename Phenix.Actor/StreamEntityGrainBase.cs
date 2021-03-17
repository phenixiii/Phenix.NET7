using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Core.Data.Model;

namespace Phenix.Actor
{
    /// <summary>
    /// 数据流实体Grain基类
    /// </summary>
    public abstract class StreamEntityGrainBase<TKernel> : StreamEntityGrainBase<TKernel, TKernel>
        where TKernel : EntityBase<TKernel>
    {
    }

    /// <summary>
    /// 数据流实体Grain基类
    /// </summary>
    public abstract class StreamEntityGrainBase<TKernel, TEvent> : EntityGrainBase<TKernel>
        where TKernel : EntityBase<TKernel>
    {
        #region 属性

        /// <summary>
        /// StreamId
        /// </summary>
        protected abstract Guid StreamId { get; }

        #region Observable

        /// <summary>
        /// (自己作为Observable的)StreamNamespace
        /// </summary>
        protected abstract string StreamNamespace { get; }

        private IAsyncStream<TEvent> _streamWorker;

        /// <summary>
        /// (自己作为Observable的)Stream
        /// </summary>
        protected virtual IAsyncStream<TEvent> StreamWorker
        {
            get { return _streamWorker ?? (_streamWorker = ClusterClient.GetStreamProvider().GetStream<TEvent>(StreamId, StreamNamespace)); }
        }

        #endregion

        #region Observer

        /// <summary>
        /// (自己作为Observer)侦听的一组StreamNamespace
        /// </summary>
        protected virtual IList<string> ListenStreamNamespaces
        {
            get { return null; }
        }

        private Dictionary<string, IAsyncStream<TEvent>> _listenStreamWorkers;

        /// <summary>
        /// (自己作为Observer)侦听的一组Stream
        /// </summary>
        protected IDictionary<string, IAsyncStream<TEvent>> ListenStreamWorkers
        {
            get
            {
                if (_listenStreamWorkers == null)
                {
                    Dictionary<string, IAsyncStream<TEvent>> result = new Dictionary<string, IAsyncStream<TEvent>>(StringComparer.Ordinal);
                    IList<string> listenStreamNamespaces = ListenStreamNamespaces;
                    if (listenStreamNamespaces != null)
                        foreach (string streamNamespace in listenStreamNamespaces)
                            result[streamNamespace] = ClusterClient.GetStreamProvider().GetStream<TEvent>(StreamId, streamNamespace);
                    _listenStreamWorkers = result;
                }

                return _listenStreamWorkers;
            }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 激活中
        /// </summary>
        public override async Task OnActivateAsync()
        {
            foreach (KeyValuePair<string, IAsyncStream<TEvent>> kvp in ListenStreamWorkers)
                await SubscribeAsync(kvp.Value);
            await base.OnActivateAsync();
        }

        #region Observable

        /// <summary>
        /// 发送消息
        /// </summary>
        protected Task Send(TEvent content, StreamSequenceToken token = null)
        {
            return StreamWorker.OnNextAsync(content, token);
        }

        #endregion

        #region Observer

        private async Task SubscribeAsync(IAsyncStream<TEvent> worker, StreamSequenceToken token = null)
        {
            IList<StreamSubscriptionHandle<TEvent>> streamHandles = await worker.GetAllSubscriptionHandles();
            if (streamHandles != null && streamHandles.Count > 0)
                foreach (StreamSubscriptionHandle<TEvent> item in streamHandles)
                    await item.ResumeAsync(OnReceiving, OnSubscribeError, OnSubscribed, token);
            else
                await worker.SubscribeAsync(OnReceiving, OnSubscribeError, OnSubscribed, token);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="streamNamespaces">(自己作为Observer)侦听的StreamNamespace</param>
        /// <param name="token">StreamSequenceToken</param>
        protected async Task SubscribeAsync(string streamNamespaces, StreamSequenceToken token = null)
        {
            IAsyncStream<TEvent> worker = ClusterClient.GetStreamProvider().GetStream<TEvent>(StreamId, streamNamespaces);
            await SubscribeAsync(worker, token);
            ListenStreamWorkers[streamNamespaces] = worker;
        }

        private async Task UnsubscribeAsync(IAsyncStream<TEvent> worker)
        {
            IList<StreamSubscriptionHandle<TEvent>> streamHandles = await worker.GetAllSubscriptionHandles();
            if (streamHandles != null && streamHandles.Count > 0)
                foreach (StreamSubscriptionHandle<TEvent> item in streamHandles)
                    await item.UnsubscribeAsync();
        }

        /// <summary>
        /// 退订消息
        /// </summary>
        /// <param name="streamNamespaces">(自己作为Observer)侦听的StreamNamespace</param>
        protected async Task UnsubscribeAsync(string streamNamespaces)
        {
            if (ListenStreamWorkers.TryGetValue(streamNamespaces, out IAsyncStream<TEvent> worker))
                await UnsubscribeAsync(worker);
        }

        /// <summary>
        /// 退订消息
        /// </summary>
        protected async Task UnsubscribeAllAsync()
        {
            foreach (KeyValuePair<string, IAsyncStream<TEvent>> kvp in ListenStreamWorkers)
                await UnsubscribeAsync(kvp.Value);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        protected abstract Task OnReceiving(TEvent content, StreamSequenceToken token);

        /// <summary>
        /// 订阅失败
        /// </summary>
        protected virtual Task OnSubscribeError(Exception error)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 订阅成功
        /// </summary>
        protected virtual Task OnSubscribed()
        {
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}