using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 泊位Grain
    /// key: BerthNo
    /// keyExtension: TerminalCode
    /// </summary>
    public class BerthGrain : Phenix.Actor.GrainBase<Berth>, IBerthGrain
    {
        #region 属性

        /// <summary>
        /// 泊位号(从左到右递增)
        /// </summary>
        protected long BerthNo
        {
            get { return PrimaryKeyLong; }
        }

        /// <summary>
        /// 码头代码
        /// </summary>
        protected string TerminalCode
        {
            get { return PrimaryKeyString; }
        }

        #endregion

        #region 方法

        #region Event

        Task IBerthGrain.OnRefresh(IList<string> usableQuayCranes)
        {
            State.OnRefresh(usableQuayCranes);
            return WriteStateAsync();
        }

        Task IBerthGrain.OnVehicleOperation(string areaCode, int carryCycle)
        {
            if (State.OnVehicleOperation(areaCode, carryCycle))
                return WriteStateAsync();
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}