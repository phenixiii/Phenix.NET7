using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Business;
using Phenix.Core.Data;

namespace Phenix.Actor
{

    /// <summary>
    /// 数据流树实体Grain基类
    /// </summary>
    public abstract class StreamTreeEntityGrainBase<TKernel> : StreamTreeEntityGrainBase<TKernel, TKernel>
        where TKernel : TreeEntityBase<TKernel>
    {
        #region 方法

        /// <summary>
        /// 操作根实体对象之后
        /// </summary>
        /// <param name="executeAction">执行动作</param>
        /// <param name="tag">标记</param>
        protected override void OnKernelOperated(ExecuteAction executeAction, object tag)
        {
            if (executeAction == ExecuteAction.Insert)
                Send(Kernel);
            else
                Send(Kernel, Kernel.PrimaryKey.ToString());
        }
        
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        protected override long AddChildNode(long parentId, IDictionary<string, object> propertyValues)
        {
            long result = base.AddChildNode(parentId, propertyValues);
            Send(Kernel, Kernel.PrimaryKey.ToString());
            return result;
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        protected override void ChangeParentNode(long id, long parentId)
        {
            base.ChangeParentNode(id, parentId);
            Send(Kernel, Kernel.PrimaryKey.ToString());
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected override void UpdateNode(long id, IDictionary<string, object> propertyValues)
        {
            base.UpdateNode(id, propertyValues);
            Send(Kernel, Kernel.PrimaryKey.ToString());
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        protected override int DeleteBranch(long id)
        {
            int result = base.DeleteBranch(id);
            Send(Kernel, Kernel.PrimaryKey.ToString());
            return result;
        }

        #endregion
    }

    /// <summary>
    /// 数据流树实体Grain基类
    /// </summary>
    public abstract class StreamTreeEntityGrainBase<TKernel, TEvent> : TreeEntityGrainBase<TKernel>
        where TKernel : TreeEntityBase<TKernel>
    {
        #region 属性

        /// <summary>
        /// StreamId
        /// </summary>
        protected abstract Guid StreamId { get; }

        #region Observer

        /// <summary>
        /// (自己作为Observer)侦听的一组StreamNamespace
        /// </summary>
        protected virtual string[] ListenStreamNamespaces
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
                    string[] listenStreamNamespaces = ListenStreamNamespaces;
                    if (listenStreamNamespaces != null)
                        foreach (string streamNamespace in listenStreamNamespaces)
                            result[streamNamespace] = ClusterClient.GetSimpleMessageStreamProvider().GetStream<TEvent>(StreamId, streamNamespace);
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
        protected Task Send(TEvent content, string streamNamespace = Standards.UnknownValue, StreamSequenceToken token = null)
        {
            return ClusterClient.GetSimpleMessageStreamProvider().GetStream<TEvent>(StreamId, streamNamespace).OnNextAsync(content, token);
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
            IAsyncStream<TEvent> worker = ClusterClient.GetSimpleMessageStreamProvider().GetStream<TEvent>(StreamId, streamNamespaces);
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
        protected virtual Task OnReceiving(TEvent content, StreamSequenceToken token)
        {
            throw new NotImplementedException();
        }

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