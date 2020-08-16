using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.DepotGateOperation;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Orleans;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.Gate
{
    /// <summary>
    /// 进场道口
    /// </summary>
    public interface IInGateGrain : IEntityGrain<DgoInGateOperation>, IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 获取预约单
        /// </summary>
        Task<DobInBookingNote> GetNote(string bookingNumber);

        /// <summary>
        /// 新增预约单
        /// </summary>
        Task<DobInBookingNote> PostNote(DobInBookingNote note);

        /// <summary>
        /// 更新预约单
        /// </summary>
        Task<DobInBookingNote> PatchNote(DobInBookingNote note);

        #region 纯内部调用接口

        /// <summary>
        /// 取消预约单
        /// </summary>
        Task CancelNote(string bookingNumber);

        /// <summary>
        /// 执行预约单
        /// </summary>
        Task OperateNote(string bookingNumber);
        
        /// <summary>
        /// 完成预约单
        /// </summary>
        Task FinishNote(string bookingNumber);

        #endregion
    }
}
