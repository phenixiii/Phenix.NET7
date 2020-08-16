using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Demo.IDOS.Plugin.Rule;
using Phenix.Actor;
using Phenix.Core.Data;

namespace Demo.IDOS.Plugin.Actor.Gate
{
    /// <summary>
    /// 进场道口
    /// </summary>
    public class InGateGrain : GrainBase, IInGateGrain
    {
        #region 属性

        private int? _timeInterval;

        /// <summary>
        /// 预约时间间隔(小时)
        /// </summary>
        protected int TimeInterval
        {
            get { return Database.AppSettings.GetProperty(ref _timeInterval, 3); }
            set { Database.AppSettings.SetProperty(ref _timeInterval, value); }
        }

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected override Database Database
        {
            get { return Database.Default.GetHandle(Id); }
        }

        private Dictionary<string, DobInBookingNote> _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected IDictionary<string, DobInBookingNote> Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    //仅关注计划中、执行中的预约单
                    _kernel = new Dictionary<string, DobInBookingNote>();
                    foreach (DobInBookingNote item in DobInBookingNote.FetchAll(Database, p =>
                            p.DpId == Id && (p.BookingStatus == BookingStatus.Planning || p.BookingStatus == BookingStatus.Operating),
                        DobInBookingNote.Ascending(p => p.Date),
                        DobInBookingNote.Ascending(p => p.DateTimeSlot)))
                        _kernel.Add(item.BookingNumber, item);
                }

                return _kernel;
            }
        }

        #endregion

        #region 方法

        private DobInBookingNote GetNote(string bookingNumber)
        {
            if (Kernel.TryGetValue(bookingNumber, out DobInBookingNote note))
                return note;
            throw new ArgumentException(String.Format("预约单不存在或已取消/完成: {0}", bookingNumber), nameof(bookingNumber));
        }

        Task<DobInBookingNote> IInBookingGrain.GetNote(string bookingNumber)
        {
            return Task.FromResult(GetNote(bookingNumber));
        }

        Task<DobInBookingNote> IInBookingGrain.PostNote(DobInBookingNote note)
        {
            note.Date = note.Date.Date;
            if (note.Date < DateTime.Today || note.Date == DateTime.Today && note.DateTimeSlot < DateTime.Now.Hour / TimeInterval)
                throw new ArgumentException(String.Format("预约时间段已过时: {0}-{1}", note.Date, note.DateTimeSlot), nameof(note));
            note.BookingNumber = String.Format("{0}{1}{2}", note.Date.ToString("YYYYMMdd"),
                Database.DataSourceSubIndex, Database.Increment.GetNext(Id.ToString()).ToString().PadLeft(6, '0'));
            note.BookingStatus = BookingStatus.Planning;
            note.InsertSelf();
            Kernel.Add(note.BookingNumber, note);
            return Task.FromResult(note);
        }

        Task<DobInBookingNote> IInBookingGrain.PatchNote(DobInBookingNote note)
        {
            note.Date = note.Date.Date;
            if (note.Date < DateTime.Today || note.Date == DateTime.Today && note.DateTimeSlot < DateTime.Now.Hour / TimeInterval)
                throw new ArgumentException(String.Format("预约时间段已过时: {0}-{1}", note.Date, note.DateTimeSlot), nameof(note));
            DobInBookingNote result = GetNote(note.BookingNumber);
            if (result.Id != note.Id)
                throw new ArgumentException(String.Format("预约单号不允许修改: {0}-{1}", note.BookingNumber, note.Id), nameof(note));
            if (result.BookingStatus != note.BookingStatus)
                throw new ArgumentException(String.Format("预约单状态不允许修改: {0}-{1}", result.BookingStatus, note.Id), nameof(note));
            if (result.BookingStatus != BookingStatus.Planning)
                throw new ArgumentException(String.Format("仅计划状态下允许修改预约单内容: {0}-{1}", result.BookingStatus, note.Id), nameof(note));
            result.UpdateSelf(note);
            return Task.FromResult(result);
        }

        #region 纯内部调用接口

        Task IInBookingGrain.CancelNote(string bookingNumber)
        {
            DobInBookingNote note = GetNote(bookingNumber);
            if (note.BookingStatus != BookingStatus.Planning && note.BookingStatus != BookingStatus.Operating)
                throw new ArgumentException(String.Format("当前状态下不允许取消预约单: {0}-{1}", note.BookingStatus, note.Id), nameof(bookingNumber));
            note.UpdateSelf(note.SetProperty(p => p.BookingStatus, BookingStatus.Cancelled));
            Kernel.Remove(note.BookingNumber);
            return Task.CompletedTask;
        }

        Task IInBookingGrain.OperateNote(string bookingNumber)
        {
            DobInBookingNote note = GetNote(bookingNumber);
            if (note.BookingStatus != BookingStatus.Planning)
                throw new ArgumentException(String.Format("当前状态下不允许执行预约单: {0}-{1}", note.BookingStatus, note.Id), nameof(bookingNumber));
            note.UpdateSelf(note.SetProperty(p => p.BookingStatus, BookingStatus.Operating));
            return Task.CompletedTask;
        }

        Task IInBookingGrain.FinishNote(string bookingNumber)
        {
            DobInBookingNote note = GetNote(bookingNumber);
            if (note.BookingStatus != BookingStatus.Operating)
                throw new ArgumentException(String.Format("当前状态下不允许完成预约单: {0}-{1}", note.BookingStatus, note.Id), nameof(bookingNumber));
            note.UpdateSelf(note.SetProperty(p => p.BookingStatus, BookingStatus.Finish));
            Kernel.Remove(note.BookingNumber);
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}
