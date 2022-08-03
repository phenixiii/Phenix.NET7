using System.Threading.Tasks;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 岸桥Grain
    /// key: MachineId
    /// </summary>
    public interface IQuayCraneGrain : ICraneGrain
    {
        #region Event

        /// <summary>
        /// 拖车作业
        /// </summary>
        /// <param name="areaId">箱区ID</param>
        /// <param name="carryCycle">载运周期(秒)</param>
        Task OnVehicleOperation(long areaId, int carryCycle);
        
        #endregion
    }
}
