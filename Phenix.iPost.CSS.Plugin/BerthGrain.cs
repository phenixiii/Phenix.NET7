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
            [PersistentState(nameof(BerthEquipQuayCrane))]
            IPersistentState<BerthEquipQuayCrane> equipQuayCrane,
            [PersistentState(nameof(BerthAreaCarryCycle))]
            IPersistentState<BerthAreaCarryCycle> areaCarryCycle)
        {
            _equipQuayCrane = equipQuayCrane;
            _areaCarryCycle = areaCarryCycle;
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

        private readonly IPersistentState<BerthEquipQuayCrane> _equipQuayCrane;

        /// <summary>
        /// 装备岸桥
        /// </summary>
        protected BerthEquipQuayCrane EquipQuayCrane
        {
            get => _equipQuayCrane.State;
            set => _equipQuayCrane.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage EquipQuayCraneStorage => _equipQuayCrane;

        private readonly IPersistentState<BerthAreaCarryCycle> _areaCarryCycle;

        /// <summary>
        /// 泊位到箱区载运周期
        /// </summary>
        protected BerthAreaCarryCycle AreaCarryCycle
        {
            get { return _areaCarryCycle.State ??= new BerthAreaCarryCycle(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage AreaCarryCycleStorage => _areaCarryCycle;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IBerthGrain.OnRefreshEquip(BerthEquipQuayCrane equipQuayCrane)
        {
            EquipQuayCrane = equipQuayCrane;
            await EquipQuayCraneStorage.WriteStateAsync();
        }

        async Task IBerthGrain.OnVehicleOperation(long areaId, int carryCycle)
        {
            if (AreaCarryCycle.OnVehicleOperation(areaId, carryCycle))
                await AreaCarryCycleStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}