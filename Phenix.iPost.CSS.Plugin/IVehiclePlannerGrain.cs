using Orleans;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 拖车规划者Grain
    /// key: TerminalCode
    /// </summary>
    public interface IVehiclePlannerGrain : Phenix.Actor.IGrain, IGrainWithStringKey
    {
        #region Event

        #endregion
    }
}