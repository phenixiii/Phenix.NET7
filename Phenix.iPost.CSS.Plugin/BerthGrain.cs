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
            [PersistentState(nameof(BerthEquipQuayCranes))]
            IPersistentState<BerthEquipQuayCranes> equipQuayCranes,
            [PersistentState(nameof(BerthAreaCarryCycles))]
            IPersistentState<BerthAreaCarryCycles> areaCarryCycles)
        {
            _equipQuayCranes = equipQuayCranes;
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

        private readonly IPersistentState<BerthEquipQuayCranes> _equipQuayCranes;

        /// <summary>
        /// 装备岸桥
        /// </summary>
        protected BerthEquipQuayCranes EquipQuayCranes
        {
            get => _equipQuayCranes.State;
            set => _equipQuayCranes.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage EquipQuayCranesStorage => _equipQuayCranes;

        private readonly IPersistentState<BerthAreaCarryCycles> _areaCarryCycles;

        /// <summary>
        /// 泊位到箱区载运周期
        /// </summary>
        protected BerthAreaCarryCycles AreaCarryCycles
        {
            get { return _areaCarryCycles.State ??= new BerthAreaCarryCycles(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage AreaCarryCyclesStorage => _areaCarryCycles;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IBerthGrain.OnRefreshEquipQuayCranes(BerthEquipQuayCranes equipQuayCranes)
        {
            EquipQuayCranes = equipQuayCranes;
            await EquipQuayCranesStorage.WriteStateAsync();
        }

        async Task IBerthGrain.OnVehicleOperation(long areaId, int carryCycle)
        {
            if (AreaCarryCycles.OnVehicleOperation(areaId, carryCycle))
                await AreaCarryCyclesStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}