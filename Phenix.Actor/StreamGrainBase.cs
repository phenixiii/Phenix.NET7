using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Core.Data.Model;
using Phenix.Core.Reflection;

namespace Phenix.Actor
{
    /// <summary>
    /// 数据流Grain基类
    /// </summary>
    public abstract class StreamGrainBase<TRootEntityBase, TEvent> : RootEntityGrainBase<TRootEntityBase>
        where TRootEntityBase : RootEntityBase<TRootEntityBase>
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

        private IAsyncStream<string> _streamWorker;

        /// <summary>
        /// (自己作为Observable的)Stream
        /// </summary>
        protected IAsyncStream<string> StreamWorker
        {
            get { return _streamWorker ?? (_streamWorker = StreamProvider.Default.GetStream<string>(StreamId, StreamNamespace)); }
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

        private Dictionary<string, IAsyncStream<string>> _listenStreamWorkers;

        /// <summary>
        /// (自己作为Observer)侦听的一组Stream
        /// </summary>
        protected IDictionary<string, IAsyncStream<string>> ListenStreamWorkers
        {
            get
            {
                if (_listenStreamWorkers == null)
                {
                    Dictionary<string, IAsyncStream<string>> result = new Dictionary<string, IAsyncStream<string>>(StringComparer.Ordinal);
                    if (ListenStreamNamespaces != null)
                        foreach (string streamNamespace in ListenStreamNamespaces)
                            result[streamNamespace] = StreamProvider.Default.GetStream<string>(StreamId, streamNamespace);
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
            foreach (KeyValuePair<string, IAsyncStream<string>> kvp in ListenStreamWorkers)
                await SubscribeAsync(kvp.Value);
            await base.OnActivateAsync();
        }

        private async Task SubscribeAsync(IAsyncStream<string> worker, StreamSequenceToken token = null)
        {
            IList<StreamSubscriptionHandle<string>> streamHandles = await worker.GetAllSubscriptionHandles();
            if (streamHandles != null && streamHandles.Count > 0)
                foreach (StreamSubscriptionHandle<string> item in streamHandles)
                    await item.ResumeAsync((content, sequenceToken) => OnReceive(Utilities.JsonDeserialize<TEvent>(content), sequenceToken), OnSubscribeError, OnSubscribeCompleted, token);
            else
                await worker.SubscribeAsync((content, sequenceToken) => OnReceive(Utilities.JsonDeserialize<TEvent>(content), sequenceToken), OnSubscribeError, OnSubscribeCompleted, token);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="streamNamespaces">(自己作为Observer)侦听的StreamNamespace</param>
        /// <param name="token">StreamSequenceToken</param>
        protected async Task SubscribeAsync(string streamNamespaces, StreamSequenceToken token = null)
        {
            IAsyncStream<string> worker = StreamProvider.Default.GetStream<string>(StreamId, streamNamespaces);
            await SubscribeAsync(worker, token);
            ListenStreamWorkers[streamNamespaces] = worker;
        }

        private async Task UnsubscribeAsync(IAsyncStream<string> worker)
        {
            IList<StreamSubscriptionHandle<string>> streamHandles = await worker.GetAllSubscriptionHandles();
            if (streamHandles != null && streamHandles.Count > 0)
                foreach (StreamSubscriptionHandle<string> item in streamHandles)
                    await item.UnsubscribeAsync();
        }

        /// <summary>
        /// 退订消息
        /// </summary>
        /// <param name="streamNamespaces">(自己作为Observer)侦听的StreamNamespace</param>
        protected async Task UnsubscribeAsync(string streamNamespaces)
        {
            if (ListenStreamWorkers.TryGetValue(streamNamespaces, out IAsyncStream<string> worker))
                await UnsubscribeAsync(worker);
        }

        /// <summary>
        /// 退订消息
        /// </summary>
        protected async Task UnsubscribeAllAsync()
        {
            foreach (KeyValuePair<string, IAsyncStream<string>> kvp in ListenStreamWorkers)
                await UnsubscribeAsync(kvp.Value);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        protected abstract Task OnReceive(TEvent content, StreamSequenceToken token);

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
        protected virtual Task OnSubscribeCompleted()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        protected Task Send(TEvent content, StreamSequenceToken token = null)
        {
            return StreamWorker.OnNextAsync(Utilities.JsonSerialize(content), token);
        }

        #endregion
    }
}