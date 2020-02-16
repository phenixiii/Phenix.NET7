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

        private IAsyncStream<string> _streamWorker;

        /// <summary>
        /// IAsyncStream
        /// </summary>
        protected IAsyncStream<string> StreamWorker
        {
            get { return _streamWorker ?? (_streamWorker = StreamProvider.Default.GetStream<string>(StreamId, StreamNamespace)); }
        }

        /// <summary>
        /// StreamId
        /// </summary>
        protected abstract Guid StreamId { get; }

        /// <summary>
        /// StreamNamespace
        /// </summary>
        protected abstract string StreamNamespace { get; }

        /// <summary>
        /// 是自动(激活的)观察者
        /// </summary>
        protected abstract bool IsAutoObserver { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 激活中
        /// </summary>
        public override async Task OnActivateAsync()
        {
            if (IsAutoObserver)
                await SubscribeAsync();
            await base.OnActivateAsync();
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        protected async Task SubscribeAsync(StreamSequenceToken token = null)
        {
            IList<StreamSubscriptionHandle<string>> streamHandles = await StreamWorker.GetAllSubscriptionHandles();
            if (streamHandles != null && streamHandles.Count > 0)
                foreach (StreamSubscriptionHandle<string> item in streamHandles)
                    await item.ResumeAsync((content, sequenceToken) => OnReceive(Utilities.JsonDeserialize<TEvent>(content), sequenceToken), OnSubscribeError, OnSubscribeCompleted, token);
            else
                await StreamWorker.SubscribeAsync((content, sequenceToken) => OnReceive(Utilities.JsonDeserialize<TEvent>(content), sequenceToken), OnSubscribeError, OnSubscribeCompleted, token);
        }

        /// <summary>
        /// 退订消息
        /// </summary>
        protected async Task UnsubscribeAsync()
        {
            IList<StreamSubscriptionHandle<string>> streamHandles = await StreamWorker.GetAllSubscriptionHandles();
            if (streamHandles != null && streamHandles.Count > 0)
                foreach (StreamSubscriptionHandle<string> item in streamHandles)
                    await item.UnsubscribeAsync();
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