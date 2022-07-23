using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 船舶Grain
    /// key: VesselCode
    /// </summary>
    public interface IVesselGrain : Phenix.Actor.IGrain, IGrainWithStringKey
    {
        #region Event

        /// <summary>
        /// 靠泊
        /// </summary>
        /// <param name="vesselBerthing">船舶靠泊</param>
        Task OnBerthing(VesselBerthing vesselBerthing);

        /// <summary>
        /// 离泊
        /// </summary>
        Task OnDeparted();

        /// <summary>
        /// 刷新进口船图
        /// </summary>
        /// <param name="info">贝位-排号-叠箱</param>
        Task OnRefreshImportBayPlan(IDictionary<int, IDictionary<int, IList<Container>>> info);

        /// <summary>
        /// 刷新预配船图
        /// </summary>
        /// <param name="info">贝位-排号-叠箱</param>
        Task OnRefreshPreBayPlan(IDictionary<int, IDictionary<int, IList<Container>>> info);
        
        /// <summary>
        /// 刷新出口船图
        /// </summary>
        /// <param name="info">贝位-排号-叠箱</param>
        Task OnRefreshExportBayPlan(IDictionary<int, IDictionary<int, IList<Container>>> info);

        #endregion
    }
}
