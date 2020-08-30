using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Orleans;

namespace Demo.IDOS.Plugin.Actor.OnlineBooking
{
    /// <summary>
    /// 在线入库预约
    /// </summary>
    public interface IInBookingGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// 获取预约单
        /// </summary>
        Task<DobInBookingNote> GetNote(string bookingNumber, string licensePlate);

        /// <summary>
        /// 新增预约单
        /// </summary>
        Task<DobInBookingNote> PostNote(DobInBookingNote note);

        /// <summary>
        /// 更新预约单
        /// </summary>
        Task<DobInBookingNote> PatchNote(DobInBookingNote note);

        /// <summary>
        /// 取消预约单
        /// </summary>
        Task CancelNote(string bookingNumber);

        #region 纯内部调用接口

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
