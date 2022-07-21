using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Providers;
using Orleans.Runtime;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 箱区Grain
    /// key: AreaId
    /// keyExtension: TerminalCode
    /// </summary>
    [StorageProvider]
    public class AreaGrain : Phenix.Actor.GrainBase, IAreaGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public AreaGrain(
            [PersistentState(nameof(Phenix.iPost.CSS.Plugin.Business.AreaRule))]
            IPersistentState<AreaRule> areaRule,
            [PersistentState(nameof(AreaEquipYardCrane))]
            IPersistentState<AreaEquipYardCrane> equipYardCrane)
        {
            _areaRule = areaRule;
            _equipYardCrane = equipYardCrane;
        }

        #region 属性

        /// <summary>
        /// 箱区Id
        /// </summary>
        protected long AreaId => PrimaryKeyLong;

        /// <summary>
        /// 码头代码
        /// </summary>
        protected string TerminalCode => PrimaryKeyExtension;

        #region Kernel

        private readonly IPersistentState<AreaRule> _areaRule;

        /// <summary>
        /// 箱区规范
        /// </summary>
        protected AreaRule AreaRule
        {
            get => _areaRule.State;
            set => _areaRule.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage AreaRuleStorage => _areaRule;

        private readonly IPersistentState<AreaEquipYardCrane> _equipYardCrane;

        /// <summary>
        /// 装备场桥
        /// </summary>
        protected AreaEquipYardCrane EquipYardCrane
        {
            get => _equipYardCrane.State;
            set => _equipYardCrane.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage EquipYardCraneStorage => _equipYardCrane;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IAreaGrain.OnRefreshRule(AreaRule areaRule)
        {
            AreaRule = areaRule;
            await AreaRuleStorage.WriteStateAsync();
        }

        async Task IAreaGrain.OnRefreshEquip(AreaEquipYardCrane equipYardCrane)
        {
            EquipYardCrane = equipYardCrane;
            await EquipYardCraneStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}