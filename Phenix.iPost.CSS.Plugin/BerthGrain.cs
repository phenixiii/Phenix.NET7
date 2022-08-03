using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Providers;
using Orleans.Runtime;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 泊位Grain
    /// key: BerthNo
    /// keyExtension: TerminalCode
    /// </summary>
    [StorageProvider]
    public class BerthGrain : Phenix.Actor.GrainBase, IBerthGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public BerthGrain(
            [PersistentState(nameof(EquipQuayCranesInfo))] IPersistentState<BerthEquipQuayCranesInfo> equipQuayCranesInfo,
            [PersistentState(nameof(AreaCarryCycles))] IPersistentState<VehicleCarryCycles> areaCarryCycles)
        {
            _equipQuayCranesInfo = equipQuayCranesInfo;
            _areaCarryCycles = areaCarryCycles;
        }

        #region 属性

        /// <summary>
        /// 泊位号（从小到大坐标排序）
        /// </summary>
        protected long BerthNo => PrimaryKeyLong;

        /// <summary>
        /// 码头代码
        /// </summary>
        protected string TerminalCode => PrimaryKeyExtension;

        #region Kernel

        private readonly IPersistentState<BerthEquipQuayCranesInfo> _equipQuayCranesInfo;

        /// <summary>
        /// 装备岸桥
        /// </summary>
        protected BerthEquipQuayCranesInfo EquipQuayCranesInfo
        {
            get => _equipQuayCranesInfo.State;
            set => _equipQuayCranesInfo.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage EquipQuayCranesInfoStorage => _equipQuayCranesInfo;

        private readonly IPersistentState<VehicleCarryCycles> _areaCarryCycles;

        /// <summary>
        /// 泊位到箱区载运周期
        /// </summary>
        protected VehicleCarryCycles AreaCarryCycles
        {
            get { return _areaCarryCycles.State ??= new VehicleCarryCycles(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage AreaCarryCyclesStorage => _areaCarryCycles;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IBerthGrain.OnRefreshEquipQuayCranes(BerthEquipQuayCranesInfo equipQuayCranesInfo)
        {
            EquipQuayCranesInfo = equipQuayCranesInfo;
            await EquipQuayCranesInfoStorage.WriteStateAsync();
        }
        
        #endregion

        #endregion
    }
}