using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Orleans;

namespace Demo.IDOS.Plugin.Actor.OnlineBooking
{
    /// <summary>
    /// 在线出库预约
    /// </summary>
    public interface IOutBookingGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// 获取预约单
        /// </summary>
        Task<DobOutBookingNote> GetNote(string bookingNumber);

        /// <summary>
        /// 新增预约单
        /// </summary>
        Task<DobOutBookingNote> PostNote(DobOutBookingNote note);

        /// <summary>
        /// 更新预约单
        /// </summary>
        Task<DobOutBookingNote> PatchNote(DobOutBookingNote note);

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
