using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.iPost.CSS.Plugin.Business.Property;

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
        /// 设置进口船图
        /// </summary>
        /// <param name="voyage">航次</param>
        /// <param name="bayPlan">贝-船图箱</param>
        Task SetBayPlan(string voyage, IDictionary<int, BayPlanContainerProperty> bayPlan);

        /// <summary>
        /// 靠泊
        /// </summary>
        /// <param name="voyage">航次</param>
        /// <param name="alongSide">靠泊信息</param>
        Task OnBerth(string voyage, VesselAlongSideProperty alongSide);

        /// <summary>
        /// 离港
        /// </summary>
        /// <param name="voyage">航次</param>
        Task OnDepart(string voyage);

        #endregion
    }
}
