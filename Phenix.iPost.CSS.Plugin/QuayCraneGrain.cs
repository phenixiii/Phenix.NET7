﻿using System;
using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Providers;
using Orleans.Runtime;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 岸桥Grain
    /// key: MachineId
    /// </summary>
    [StorageProvider]
    public class QuayCraneGrain : CraneGrainBase, IQuayCraneGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public QuayCraneGrain(IEventBus eventBus,
            [PersistentState(nameof(StatusInfo))] IPersistentState<MachineStatusInfo> statusInfo,
            [PersistentState(nameof(PowerInfo))] IPersistentState<PowerInfo> powerInfo,
            [PersistentState(nameof(GridCellInfo))] IPersistentState<GridCellInfo> gridCellInfo,
            [PersistentState(nameof(CraneAction))] IPersistentState<CraneAction> craneAction,
            [PersistentState(nameof(VehicleCarryCycles))] IPersistentState<VehicleCarryCycles> vehicleCarryCycles)
            : base(eventBus, statusInfo, powerInfo, gridCellInfo, craneAction)
        {
            _vehicleCarryCycles = vehicleCarryCycles;
        }

        #region 属性

        #region Kernel

        private readonly IPersistentState<VehicleCarryCycles> _vehicleCarryCycles;

        /// <summary>
        /// 拖车作业周期
        /// </summary>
        protected VehicleCarryCycles VehicleCarryCycles
        {
            get { return _vehicleCarryCycles.State ??= new VehicleCarryCycles(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage VehicleCarryCyclesStorage => _vehicleCarryCycles;

        #endregion

        #endregion

        #region 方法

        protected override Task ExecuteTimerAsync(object args)
        {
            throw new NotImplementedException();
        }

        #region Event
        async Task IQuayCraneGrain.OnVehicleOperation(long areaId, int carryCycle)
        {
            if (VehicleCarryCycles.OnVehicleOperation(areaId, carryCycle))
                await VehicleCarryCyclesStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}