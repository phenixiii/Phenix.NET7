using System.Threading.Tasks;
using Orleans;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 泊位Grain
    /// key: BerthNo
    /// keyExtension: TerminalCode
    /// </summary>
    public interface IBerthGrain : IGrainWithIntegerCompoundKey
    {
        #region Event

        /// <summary>
        /// 刷新装备岸桥
        /// </summary>
        /// <param name="equipQuayCranesInfo">装备岸桥</param>
        Task OnRefreshEquipQuayCranes(BerthEquipQuayCranesInfo equipQuayCranesInfo);

        #endregion
    }
}