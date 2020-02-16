using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.InspectionStation.Plugin.Business;
using Orleans;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Data.Model;

namespace Demo.InspectionStation.Plugin.Actor
{
    /// <summary>
    /// 中控Grain
    /// </summary>
    public class CenterGrain : StreamGrainBase<IsCenter, IsOperationPoint>, ICenterGrain
    {
        #region 属性

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name ?? (_name = this.GetPrimaryKeyString()); }
        }

        /// <summary>
        /// ID(映射表XX_ID字段)
        /// </summary>
        protected override long Id
        {
            get { return Kernel.Id; }
        }

        private IsCenter _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override IsCenter Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    _kernel = RootEntityBase<IsCenter>.Fetch(p => p.Name == Name);
                    if (_kernel == null)
                    {
                        _kernel = new IsCenter(Name);
                        _kernel.InsertSelf();
                    }
                }

                return _kernel;
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
            get { return AppConfig.OperationPointStreamNamespace; }
        }

        /// <summary>
        /// 是自动(激活的)观察者
        /// </summary>
        protected override bool IsAutoObserver
        {
            get { return true; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        protected override Task OnReceive(IsOperationPoint content, StreamSequenceToken token)
        {
            Kernel.OperationPointDictionary[content.Name] = content;
            return Task.CompletedTask;
        }

        Task ICenterGrain.Monitoring(IList<string> operationPoints)
        {
            Kernel.Monitoring(operationPoints);
            return Task.CompletedTask;
        }

        #endregion
    }
}