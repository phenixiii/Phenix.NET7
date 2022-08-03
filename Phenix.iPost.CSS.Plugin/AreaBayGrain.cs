using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Providers;
using Orleans.Runtime;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 箱区贝Grain
    /// key: BayNo
    /// keyExtension: AreaId
    /// </summary>
    [StorageProvider]
    public class AreaBayGrain : Phenix.Actor.GrainBase, IAreaBayGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public AreaBayGrain(
            [PersistentState(nameof(TieredContainers))] IPersistentState<AreaBayTieredContainers> tieredContainers)
        {
            _tieredContainers = tieredContainers;
        }

        #region 属性

        /// <summary>
        /// 贝位
        /// </summary>
        protected long BayNo => PrimaryKeyLong;

        /// <summary>
        /// 箱区Id
        /// </summary>
        protected string AreaId => PrimaryKeyExtension;

        #region Kernel

        private readonly IPersistentState<AreaBayTieredContainers> _tieredContainers;

        /// <summary>
        /// 堆箱
        /// </summary>
        protected AreaBayTieredContainers TieredContainers
        {
            get { return _tieredContainers.State ??= new AreaBayTieredContainers(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage TieredContainersStorage => _tieredContainers;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IAreaBayGrain.OnRefreshTieredContainers(IDictionary<int, IList<ContainerInfo>> info)
        {
            TieredContainers.OnRefresh(info);
            await TieredContainersStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}