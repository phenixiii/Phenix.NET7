using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

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
        /// 刷新
        /// </summary>
        /// <param name="usableQuayCranes">可用岸桥(从左到右编排)</param>
        Task OnRefresh(IList<string> usableQuayCranes);

        /// <summary>
        /// 拖车作业
        /// </summary>
        /// <param name="areaCode">箱区代码</param>
        /// <param name="carryCycle">载运周期(秒)</param>
        Task OnVehicleOperation(string areaCode, int carryCycle);

        #endregion
    }
}