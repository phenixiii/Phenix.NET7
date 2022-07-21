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
        /// 刷新规范
        /// </summary>
        /// <param name="areaRule">箱区规范</param>
        Task OnRefreshRule(AreaRule areaRule);

        /// <summary>
        /// 刷新装备
        /// </summary>
        /// <param name="equipYardCrane">装备场桥</param>
        Task OnRefreshEquip(AreaEquipYardCrane equipYardCrane);

        #endregion
    }
}