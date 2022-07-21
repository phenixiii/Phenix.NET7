using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.iPost.CSS.Plugin.Business.Property;

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
        /// 刷新
        /// </summary>
        /// <param name="tieredContainers">排号-叠箱</param>
        Task OnRefresh(IDictionary<int, IList<ContainerProperty>> tieredContainers);

        #endregion
    }
}