using System.Threading.Tasks;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 拖车Grain
    /// key: MachineId
    /// </summary>
    public interface IVehicleGrain : IMachineGrain
    {
        #region Event
        
        /// <summary>
        /// 有动作
        /// </summary>
        /// <param name="action">动作</param>
        Task OnAction(VehicleBerthAction action);

        /// <summary>
        /// 有动作
        /// </summary>
        /// <param name="action">动作</param>
        Task OnAction(VehicleYardAction action);
        
        /// <summary>
        /// 任务反馈
        /// </summary>
        /// <param name="taskStatus">任务状态</param>
        Task OnTaskAck(Phenix.iPost.CSS.Plugin.Business.Norms.TaskStatus taskStatus);

        #endregion
    }
}