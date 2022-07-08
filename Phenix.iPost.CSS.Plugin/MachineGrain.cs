using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 机械Grain
    /// key: machineId
    /// </summary>
    public abstract class MachineGrain<T, TKernel> : GrainBase<T>, IMachineGrain
        where T : MachineGrain<T, TKernel>
        where TKernel : Machine
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected MachineGrain(ILogger<T> logger, IEventBus eventBus)
            : base(logger, eventBus)
        {
        }

        #region 属性
        
        /// <summary>
        /// 设备ID
        /// </summary>
        protected string MachineId
        {
            get { return PrimaryKeyString; }
        }

        /// <summary>
        /// Kernel
        /// </summary>
        protected abstract TKernel Kernel { get; }

        #endregion

        #region 方法

        #region Event

        Task IMachineGrain.OnChangeStatus(MachineStatusProperty status)
        {
            Kernel.OnChangeStatus(status);
            return Task.CompletedTask;
        }

        Task IMachineGrain.OnMoving(SpaceTimeProperty spaceTime)
        {
            Kernel.OnMoving(spaceTime);
            return Task.CompletedTask;
        }

        Task IMachineGrain.OnChangePower(PowerProperty power)
        {
            Kernel.OnChangePower(power);
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}