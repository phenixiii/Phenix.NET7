using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Providers;
using Orleans.Runtime;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 箱区贝Grain
    /// key: AreaId
    /// keyExtension: BayNo
    /// </summary>
    [StorageProvider]
    public class AreaBayGrain : Phenix.Actor.GrainBase, IAreaBayGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public AreaBayGrain(
            [PersistentState(nameof(AreaBayStack))]
            IPersistentState<AreaBayStack> stack)
        {
            _stack = stack;
        }

        #region 属性

        /// <summary>
        /// 箱区Id
        /// </summary>
        protected long AreaId => PrimaryKeyLong;

        /// <summary>
        /// 贝位
        /// </summary>
        protected int BayNo => int.Parse(PrimaryKeyExtension);

        #region Kernel

        private readonly IPersistentState<AreaBayStack> _stack;

        /// <summary>
        /// 堆存
        /// </summary>
        protected AreaBayStack Stack
        {
            get { return _stack.State ??= new AreaBayStack(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage StackStorage => _stack;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IAreaBayGrain.OnRefresh(IDictionary<int, IList<ContainerProperty>> tieredContainers)
        {
            Stack.OnRefresh(tieredContainers);
            await StackStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}