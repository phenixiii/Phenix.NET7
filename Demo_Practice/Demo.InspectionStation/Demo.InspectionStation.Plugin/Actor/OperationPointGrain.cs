using System;
using System.Threading.Tasks;
using Demo.InspectionStation.Plugin.Business;
using Orleans;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Data.Schema;

namespace Demo.InspectionStation.Plugin.Actor
{
    /// <summary>
    /// 作业点Grain
    /// </summary>
    public class OperationPointGrain : StreamEntityGrainBase<IsOperationPoint, IsOperationPoint>, IOperationPointGrain
    {
        #region 属性

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        protected string Name
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

        private IsOperationPoint _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override IsOperationPoint Kernel
        {
            get
            {
                return _kernel ?? (_kernel = IsOperationPoint.FetchRoot(Database,
                           p => p.Name == Name,
                           () => IsOperationPoint.New(Database,
                               NameValue.Set<IsOperationPoint>(p => p.Name, Name))));
            }
            set { _kernel = value; }
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
            get { return Name; }
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        Task<OperationPointStatus> IOperationPointGrain.GetStatus()
        {
            return Task.FromResult(Kernel.OperationPointStatus);
        }

        Task<int> IOperationPointGrain.GetWeighbridge()
        {
            return Task.FromResult(Kernel.Weighbridge);
        }

        Task IOperationPointGrain.SetWeighbridge(int value)
        {
            Kernel.SetWeighbridge(value);
            Send(Kernel);
            return Task.CompletedTask;
        }
        
        Task IOperationPointGrain.WeighbridgeAlive()
        {
            Kernel.WeighbridgeAlive();
            Send(Kernel);
            return Task.CompletedTask;
        }
        
        Task<string> IOperationPointGrain.GetLicensePlate()
        {
            return Task.FromResult(Kernel.LicensePlate);
        }

        Task IOperationPointGrain.SetLicensePlate(string value)
        {
            Kernel.SetLicensePlate(value);
            Send(Kernel);
            return Task.CompletedTask;
        }

        Task IOperationPointGrain.LicensePlateAlive()
        {
            Kernel.LicensePlateAlive();
            Send(Kernel);
            return Task.CompletedTask;
        }

        Task IOperationPointGrain.PermitThrough()
        {
            Kernel.PermitThrough();
            Send(Kernel);
            return Task.CompletedTask;
        }

        #endregion
    }
}