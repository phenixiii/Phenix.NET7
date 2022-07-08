using System.Threading.Tasks;
using Orleans;
using Phenix.iPost.CSS.Plugin.Business.Property;

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
        /// <param name="status">状态</param>
        Task OnChangeStatus(MachineStatusProperty status);

        /// <summary>
        /// 在移动
        /// </summary>
        /// <param name="spaceTime">时空</param>
        Task OnMoving(SpaceTimeProperty spaceTime);

        /// <summary>
        /// 动力变化
        /// </summary>
        /// <param name="power">动力</param>
        Task OnChangePower(PowerProperty power);

        #endregion
    }
}