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
        /// <param name="equipQuayCranes">装备岸桥</param>
        Task OnRefreshEquipQuayCranes(BerthEquipQuayCranes equipQuayCranes);

        /// <summary>
        /// 拖车作业
        /// </summary>
        /// <param name="areaId">箱区ID</param>
        /// <param name="carryCycle">载运周期(秒)</param>
        Task OnVehicleOperation(long areaId, int carryCycle);

        #endregion
    }
}