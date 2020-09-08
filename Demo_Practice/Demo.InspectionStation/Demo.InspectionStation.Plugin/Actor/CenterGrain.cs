using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.InspectionStation.Plugin.Business;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Message;
using Phenix.Core.Reflection;

namespace Demo.InspectionStation.Plugin.Actor
{
    /// <summary>
    /// 中控Grain
    /// </summary>
    public class CenterGrain : StreamEntityGrainBase<IsCenter, IsOperationPoint>, ICenterGrain
    {
        #region 属性

        /// <summary>
        /// ID(映射表ID字段)
        /// </summary>
        protected override long Id
        {
            get { return Kernel.Id; }
        }
        
        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override IsCenter Kernel
        {
            get
            {
                return base.Kernel ?? (base.Kernel = IsCenter.FetchRoot(Database,
                           p => p.Name == Name,
                           () => IsCenter.New(Database,
                               IsCenter.Set(p => p.Name, Name))));
            }
        }

        /// <summary>
        /// StreamId
        /// </summary>
        protected override Guid StreamId
        {
            get { return AppConfig.OperationPointStreamId; }
        }

        /// <summary>
        /// StreamNamespace
        /// </summary>
        protected override string StreamNamespace
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// (自己作为Observer)侦听的一组StreamNamespace
        /// </summary>
        protected override IList<string> ListenStreamNamespaces
        {
            get { return Kernel.OperationPoints; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        protected override Task OnReceiving(IsOperationPoint content, StreamSequenceToken token)
        {
            UserMessage.Send(StreamId.ToString(), Name, Utilities.JsonSerialize(content));
            return Task.CompletedTask;
        }

        Task<IDictionary<string, IsOperationPoint>> ICenterGrain.FetchOperationPoint()
        {
            return Task.FromResult(Kernel.OperationPointDictionary);
        }

        async Task ICenterGrain.Listen(IList<string> operationPoints)
        {
            IList<string> oldOperationPoints = new List<string>(Kernel.OperationPoints);
            Kernel.Listen(operationPoints);
            foreach (string s in oldOperationPoints)
                if (!operationPoints.Contains(s))
                    await UnsubscribeAsync(s);
            foreach (string s in operationPoints)
                if (!oldOperationPoints.Contains(s))
                    await SubscribeAsync(s);
        }

        #endregion
    }
}