using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Runtime;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 机械Grain基类
    /// key: MachineId
    /// </summary>
    public abstract class MachineGrainBase : GrainBase, IMachineGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected MachineGrainBase(IEventBus eventBus,
            [PersistentState(nameof(StatusInfo))] IPersistentState<MachineStatusInfo> statusInfo,
            [PersistentState(nameof(PowerInfo))] IPersistentState<PowerInfo> powerInfo,
            [PersistentState(nameof(GridCellInfo))] IPersistentState<GridCellInfo> gridCellInfo)
            : base(eventBus)
        {
            _statusInfo = statusInfo;
            _powerInfo = powerInfo;
            _gridCellInfo = gridCellInfo;
        }

        #region 属性

        /// <summary>
        /// 设备ID
        /// </summary>
        protected string MachineId => PrimaryKeyString;

        #region Kernel

        private readonly IPersistentState<MachineStatusInfo> _statusInfo;

        /// <summary>
        /// 状态
        /// </summary>
        protected MachineStatusInfo StatusInfo
        {
            get => _statusInfo.State;
            set => _statusInfo.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage StatusInfoStorage => _statusInfo;

        private readonly IPersistentState<PowerInfo> _powerInfo;

        /// <summary>
        /// 动力
        /// </summary>
        protected PowerInfo PowerInfo
        {
            get => _powerInfo.State;
            set => _powerInfo.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage PowerInfoStorage => _powerInfo;

        private readonly IPersistentState<GridCellInfo> _gridCellInfo;

        /// <summary>
        /// 工艺状态
        /// </summary>
        protected GridCellInfo GridCellInfo
        {
            get => _gridCellInfo.State;
            set => _gridCellInfo.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage GridCellInfoStorage => _gridCellInfo;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IMachineGrain.OnChangeStatus(MachineStatusInfo statusInfo)
        {
            if (StatusInfo != statusInfo)
            {
                StatusInfo = statusInfo;
                await StatusInfoStorage.WriteStateAsync();
            }
        }

        async Task IMachineGrain.OnChangePower(PowerInfo powerInfo)
        {
            if (PowerInfo != powerInfo)
            {
                PowerInfo = powerInfo;
                await PowerInfoStorage.WriteStateAsync();
            }
        }

        public virtual async Task OnMoving(SpaceTimeInfo spaceTimeInfo)
        {
            string location = GridCellInfo.Location;
            GridCellInfo = new GridCellInfo(spaceTimeInfo.X, spaceTimeInfo.Y, spaceTimeInfo.Location);
            if (location != spaceTimeInfo.Location)
                await GridCellInfoStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}