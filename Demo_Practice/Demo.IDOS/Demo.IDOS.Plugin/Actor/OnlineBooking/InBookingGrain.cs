using System;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Demo.IDOS.Plugin.Rule;
using Phenix.Actor;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.SyncCollections;

namespace Demo.IDOS.Plugin.Actor.OnlineBooking
{
    /// <summary>
    /// 在线入库预约
    /// </summary>
    public class OnlineInBookingGrain : GrainBase, IInBookingGrain
    {
        #region 属性

        private int? _timeInterval;

        /// <summary>
        /// 预约时间间隔(小时)
        /// </summary>
        protected int TimeInterval
        {
            get { return AppSettings.GetProperty(ref _timeInterval, 3); }
            set { AppSettings.SetProperty(ref _timeInterval, value); }
        }

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected override Database Database
        {
            get { return Database.Default.GetHandle(Id); }
        }

        private SynchronizedMultiSortedList<DobInBookingNote> _kernel;

        /// <summary>
        /// 根实体对象: 仅缓存计划中或作业中的预约单
        /// </summary>
        protected SynchronizedMultiSortedList<DobInBookingNote> Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    SynchronizedMultiSortedList<DobInBookingNote> result = new SynchronizedMultiSortedList<DobInBookingNote>();
                    foreach (DobInBookingNote item in DobInBookingNote.FetchAll(Database, p =>
                            p.DpId == Id && (p.BookingStatus == BookingStatus.Planning || p.BookingStatus == BookingStatus.Operating),
                        DobInBookingNote.Ascending(p => p.Date).Ascending(p => p.DateTimeSlot)))
                        result.Add(item);
                    _kernel = result;
                }

                return _kernel;
            }
        }

        #endregion

        #region 方法

        private DobInBookingNote GetNote(string bookingNumber, string licensePlate = null)
        {
            if (bookingNumber != null)
                return Kernel.TryGetValue(p => p.BookingNumber, bookingNumber, out DobInBookingNote note) ? note : null;
            if (licensePlate != null)
                return Kernel.TryGetValue(p => p.LicensePlate, licensePlate, out DobInBookingNote note) ? note : null;
            throw new ArgumentNullException(nameof(bookingNumber), "请提供预约单号");
        }

        Task<DobInBookingNote> IInBookingGrain.GetNote(string bookingNumber, string licensePlate)
        {
            return Task.FromResult(GetNote(bookingNumber, licensePlate));
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
            Kernel.Add(note);
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

        Task IInBookingGrain.CancelNote(string bookingNumber)
        {
            DobInBookingNote note = GetNote(bookingNumber);
            if (note.BookingStatus == BookingStatus.Finish)
                throw new ArgumentException(String.Format("当前状态下不允许取消预约单: {0}-{1}", note.BookingStatus, note.Id), nameof(bookingNumber));
            note.UpdateSelf(note.SetProperty(p => p.BookingStatus, BookingStatus.Cancelled));
            Kernel.Remove(note);
            return Task.CompletedTask;
        }

        #region 纯内部调用接口

        Task IInBookingGrain.OperateNote(string bookingNumber)
        {
            DobInBookingNote note = GetNote(bookingNumber);
            if (note.BookingStatus == BookingStatus.Cancelled || note.BookingStatus == BookingStatus.Finish)
                throw new ArgumentException(String.Format("当前状态下不允许执行预约单: {0}-{1}", note.BookingStatus, note.Id), nameof(bookingNumber));
            note.UpdateSelf(note.SetProperty(p => p.BookingStatus, BookingStatus.Operating));
            return Task.CompletedTask;
        }

        Task IInBookingGrain.FinishNote(string bookingNumber)
        {
            DobInBookingNote note = GetNote(bookingNumber);
            if (note.BookingStatus == BookingStatus.Planning || note.BookingStatus == BookingStatus.Cancelled)
                throw new ArgumentException(String.Format("当前状态下不允许完成预约单: {0}-{1}", note.BookingStatus, note.Id), nameof(bookingNumber));
            note.UpdateSelf(note.SetProperty(p => p.BookingStatus, BookingStatus.Finish));
            Kernel.Remove(note);
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}
