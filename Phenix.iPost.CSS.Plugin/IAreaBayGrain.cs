using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 箱区贝Grain
    /// key: AreaId
    /// keyExtension: BayNo
    /// </summary>
    public interface IAreaBayGrain : IGrainWithIntegerCompoundKey
    {
        #region Event

        /// <summary>
        /// 刷新堆箱
        /// </summary>
        /// <param name="info">排号-叠箱</param>
        Task OnRefreshTieredContainers(IDictionary<int, IList<Container>> info);

        #endregion
    }
}