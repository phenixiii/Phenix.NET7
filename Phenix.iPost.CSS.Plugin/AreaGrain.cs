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
            [PersistentState(nameof(AreaEquipYardCranes))]
            IPersistentState<AreaEquipYardCranes> equipYardCranes)
        {
            _areaRule = areaRule;
            _equipYardCranes = equipYardCranes;
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

        private readonly IPersistentState<AreaEquipYardCranes> _equipYardCranes;

        /// <summary>
        /// 装备场桥
        /// </summary>
        protected AreaEquipYardCranes EquipYardCranes
        {
            get => _equipYardCranes.State;
            set => _equipYardCranes.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage EquipYardCranesStorage => _equipYardCranes;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IAreaGrain.OnRefreshAreaRule(AreaRule areaRule)
        {
            AreaRule = areaRule;
            await AreaRuleStorage.WriteStateAsync();
        }

        async Task IAreaGrain.OnRefreshEquipYardCranes(AreaEquipYardCranes equipYardCranes)
        {
            EquipYardCranes = equipYardCranes;
            await EquipYardCranesStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}