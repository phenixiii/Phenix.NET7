using System;
using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Runtime;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 拖车Grain
    /// key: MachineId
    /// keyExtension: TerminalCode
    /// </summary>
    public class VehicleGrain : MachineGrainBase, IVehicleGrain
    {
        public VehicleGrain(IEventBus eventBus,
            [PersistentState(nameof(StatusInfo))] IPersistentState<MachineStatusInfo> statusInfo,
            [PersistentState(nameof(PowerInfo))] IPersistentState<PowerInfo> powerInfo,
            [PersistentState(nameof(GridCellInfo))] IPersistentState<GridCellInfo> gridCellInfo,
            [PersistentState(nameof(VehicleOperation))] IPersistentState<VehicleOperation> vehicleOperation)
            : base(eventBus, statusInfo, powerInfo, gridCellInfo)
        {
            _vehicleOperation = vehicleOperation;
        }

        #region 属性

        #region Kernel

        private readonly IPersistentState<VehicleOperation> _vehicleOperation;

        /// <summary>
        /// 拖车作业
        /// </summary>
        protected VehicleOperation VehicleOperation
        {
            get => _vehicleOperation.State;
            set => _vehicleOperation.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage VehicleOperationStorage => _vehicleOperation;

        #endregion

        #endregion

        #region 方法

        protected override Task ExecuteTimerAsync(object args)
        {
            throw new NotImplementedException();
        }

        private VehicleOperation NewOperation(VehicleOperationType operationType)
        {
            if (VehicleOperation != null && VehicleOperation.InOperation)
                throw new InvalidOperationException($"{MachineId}({VehicleOperation.GetActivityTask().TaskStatus})当前无法新增作业需人工干预!");

            VehicleOperation = new VehicleOperation(MachineId, operationType);
            VehicleOperationStorage.WriteStateAsync();
            return VehicleOperation;
        }

        #region Event

        public override async Task OnMoving(SpaceTimeInfo spaceTimeInfo)
        {
            await base.OnMoving(spaceTimeInfo);

            VehicleOperation.OnMoving(spaceTimeInfo);
        }

        async Task IVehicleGrain.OnTaskAck(Phenix.iPost.CSS.Plugin.Business.Norms.TaskStatus taskStatus)
        {
            VehicleOperation.OnTaskAck(taskStatus);
            await VehicleOperationStorage.WriteStateAsync();
        }

        async Task IVehicleGrain.OnAction(VehicleBerthAction action)
        {
            VehicleOperation.OnActivity(action);
            await VehicleOperationStorage.WriteStateAsync();
        }

        async Task IVehicleGrain.OnAction(VehicleYardAction action)
        {
            VehicleOperation.OnActivity(action);
            await VehicleOperationStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}