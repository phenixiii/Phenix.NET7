using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 拖车Grain
    /// key: machineId
    /// </summary>
    public class VehicleGrain : MachineGrain<VehicleGrain, Vehicle>, IVehicleGrain
    {
        public VehicleGrain(ILogger<VehicleGrain> logger, IEventBus eventBus)
            : base(logger, eventBus)
        {
        }

        #region 属性

        private Vehicle _kernel;

        /// <summary>
        /// Kernel
        /// </summary>
        protected override Vehicle Kernel
        {
            get { return _kernel ??= new Vehicle(MachineId); }
        }

        #endregion

        #region 方法

        protected override Task ExecuteTimerAsync(object args)
        {
            throw new NotImplementedException();
        }

        #region Event

        Task IVehicleGrain.OnTaskAck(Phenix.iPost.CSS.Plugin.Business.Norms.TaskStatus taskStatus)
        {
            Kernel.OnTaskAck(taskStatus);
            return Task.CompletedTask;
        }

        Task IVehicleGrain.OnAction(VehicleBerthAction action)
        {
            Kernel.OnActivity(action);
            return Task.CompletedTask;
        }

        Task IVehicleGrain.OnAction(VehicleYardAction action)
        {
            Kernel.OnActivity(action);
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}