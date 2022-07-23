using System.Threading.Tasks;
using Orleans;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 箱区Grain
    /// key: AreaId
    /// keyExtension: TerminalCode
    /// </summary>
    public interface IAreaGrain : IGrainWithIntegerCompoundKey
    {
        #region Event

        /// <summary>
        /// 刷新箱区规范
        /// </summary>
        /// <param name="areaRule">箱区规范</param>
        Task OnRefreshAreaRule(AreaRule areaRule);

        /// <summary>
        /// 刷新装备场桥
        /// </summary>
        /// <param name="equipYardCranes">装备场桥</param>
        Task OnRefreshEquipYardCranes(AreaEquipYardCranes equipYardCranes);

        #endregion
    }
}