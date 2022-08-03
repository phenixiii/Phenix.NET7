using System.Threading.Tasks;
using Orleans;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 机械Grain
    /// key: MachineId
    /// </summary>
    public interface IMachineGrain : Phenix.Actor.IGrain, IGrainWithStringKey
    {
        #region Event

        /// <summary>
        /// 状态变化
        /// </summary>
        /// <param name="statusInfo">状态</param>
        Task OnChangeStatus(MachineStatusInfo statusInfo);

        /// <summary>
        /// 动力变化
        /// </summary>
        /// <param name="powerInfo">动力</param>
        Task OnChangePower(PowerInfo powerInfo);

        /// <summary>
        /// 在移动
        /// </summary>
        /// <param name="spaceTimeInfo">时空</param>
        Task OnMoving(SpaceTimeInfo spaceTimeInfo);

        #endregion
    }
}