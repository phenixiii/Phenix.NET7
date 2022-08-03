using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Runtime;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 吊车Grain
    /// key: machineId
    /// </summary>
    public abstract class CraneGrainBase : MachineGrainBase, ICraneGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected CraneGrainBase(IEventBus eventBus,
            [PersistentState(nameof(StatusInfo))] IPersistentState<MachineStatusInfo> statusInfo,
            [PersistentState(nameof(PowerInfo))] IPersistentState<PowerInfo> powerInfo,
            [PersistentState(nameof(GridCellInfo))] IPersistentState<GridCellInfo> gridCellInfo,
            [PersistentState(nameof(CraneAction))] IPersistentState<CraneAction> craneAction)
            : base(eventBus, statusInfo, powerInfo, gridCellInfo)
        {
            _craneAction = craneAction;
        }

        #region 属性

        #region Kernel

        private readonly IPersistentState<CraneAction> _craneAction;

        /// <summary>
        /// 吊车动作
        /// </summary>
        protected CraneAction CraneAction
        {
            get => _craneAction.State;
            set => _craneAction.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage CraneActionStorage => _craneAction;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task ICraneGrain.OnAction(CraneAction craneAction)
        {
            if (CraneAction != craneAction)
            {
                CraneAction = craneAction;
                await CraneActionStorage.WriteStateAsync();
            }
        }

        Task ICraneGrain.OnAction(CraneGrabAction grabAction, int hoistHeight)
        {
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}