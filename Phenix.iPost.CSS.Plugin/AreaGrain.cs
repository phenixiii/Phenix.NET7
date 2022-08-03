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
            [PersistentState(nameof(AreaRuleInfo))] IPersistentState<AreaRuleInfo> areaRuleInfo,
            [PersistentState(nameof(EquipYardCranesInfo))] IPersistentState<AreaEquipYardCranesInfo> equipYardCranesInfo)
        {
            _areaRuleInfo = areaRuleInfo;
            _equipYardCranesInfo = equipYardCranesInfo;
        }

        #region 属性

        /// <summary>
        /// 箱区Id
        /// </summary>
        protected string AreaId => PrimaryKeyString;

        /// <summary>
        /// 码头代码
        /// </summary>
        protected string TerminalCode => PrimaryKeyExtension;

        #region Kernel

        private readonly IPersistentState<AreaRuleInfo> _areaRuleInfo;

        /// <summary>
        /// 箱区规范
        /// </summary>
        protected AreaRuleInfo AreaRuleInfo
        {
            get => _areaRuleInfo.State;
            set => _areaRuleInfo.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage AreaRuleInfoStorage => _areaRuleInfo;

        private readonly IPersistentState<AreaEquipYardCranesInfo> _equipYardCranesInfo;

        /// <summary>
        /// 装备场桥
        /// </summary>
        protected AreaEquipYardCranesInfo EquipYardCranesInfo
        {
            get => _equipYardCranesInfo.State;
            set => _equipYardCranesInfo.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage EquipYardCranesInfoStorage => _equipYardCranesInfo;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IAreaGrain.OnRefreshAreaRule(AreaRuleInfo areaRuleInfo)
        {
            if (AreaRuleInfo != areaRuleInfo)
            {
                AreaRuleInfo = areaRuleInfo;
                await AreaRuleInfoStorage.WriteStateAsync();
            }
        }

        async Task IAreaGrain.OnRefreshEquipYardCranes(AreaEquipYardCranesInfo equipYardCranesInfo)
        {
            EquipYardCranesInfo = equipYardCranesInfo;
            await EquipYardCranesInfoStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}