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
    /// 在线出库预约
    /// </summary>
    public class OutBookingGrain : GrainBase, IOutBookingGrain
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

        private SynchronizedMultiSortedList<DobOutBookingNote> _kernel;

        /// <summary>
        /// 根实体对象: 仅缓存计划中或作业中的预约单
        /// </summary>
        protected SynchronizedMultiSortedList<DobOutBookingNote> Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    SynchronizedMultiSortedList<DobOutBookingNote> result = new SynchronizedMultiSortedList<DobOutBookingNote>();
                    foreach (DobOutBookingNote item in DobOutBookingNote.FetchAll(Database, p =>
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

        private DobOutBookingNote GetNote(string bookingNumber, string licensePlate = null)
        {
            if (bookingNumber != null)
                return Kernel.TryGetValue(p => p.BookingNumber, bookingNumber, out DobOutBookingNote note) ? note : null;
            if (licensePlate != null)
                return Kernel.TryGetValue(p => p.LicensePlate, licensePlate, out DobOutBookingNote note) ? note : null;
            throw new ArgumentNullException(nameof(bookingNumber), "请提供预约单号");
        }

        Task<DobOutBookingNote> IOutBookingGrain.GetNote(string bookingNumber, string licensePlate)
        {
            return Task.FromResult(GetNote(bookingNumber, licensePlate));
        }

        Task<DobOutBookingNote> IOutBookingGrain.PostNote(DobOutBookingNote note)
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

        Task<DobOutBookingNote> IOutBookingGrain.PatchNote(DobOutBookingNote note)
        {
            note.Date = note.Date.Date;
            if (note.Date < DateTime.Today || note.Date == DateTime.Today && note.DateTimeSlot < DateTime.Now.Hour / TimeInterval)
                throw new ArgumentException(String.Format("预约时间段已过时: {0}-{1}", note.Date, note.DateTimeSlot), nameof(note));
            DobOutBookingNote result = GetNote(note.BookingNumber);
            if (result.Id != note.Id)
                throw new ArgumentException(String.Format("预约单号不允许修改: {0}-{1}", note.BookingNumber, note.Id), nameof(note));
            if (result.BookingStatus != note.BookingStatus)
                throw new ArgumentException(String.Format("预约单状态不允许修改: {0}-{1}", result.BookingStatus, note.Id), nameof(note));
            if (result.BookingStatus != BookingStatus.Planning)
                throw new ArgumentException(String.Format("仅计划状态下允许修改预约单内容: {0}-{1}", result.BookingStatus, note.Id), nameof(note));
            result.UpdateSelf(note);
            return Task.FromResult(result);
        }

        Task IOutBookingGrain.CancelNote(string bookingNumber)
        {
            DobOutBookingNote note = GetNote(bookingNumber);
            if (note.BookingStatus == BookingStatus.Finish)
                throw new ArgumentException(String.Format("当前状态下不允许取消预约单: {0}-{1}", note.BookingStatus, note.Id), nameof(bookingNumber));
            note.UpdateSelf(note.SetProperty(p => p.BookingStatus, BookingStatus.Cancelled));
            Kernel.Remove(note);
            return Task.CompletedTask;
        }

        #region 纯内部调用接口

        Task IOutBookingGrain.OperateNote(string bookingNumber)
        {
            DobOutBookingNote note = GetNote(bookingNumber);
            if (note.BookingStatus == BookingStatus.Cancelled || note.BookingStatus == BookingStatus.Finish)
                throw new ArgumentException(String.Format("当前状态下不允许执行预约单: {0}-{1}", note.BookingStatus, note.Id), nameof(bookingNumber));
            note.UpdateSelf(note.SetProperty(p => p.BookingStatus, BookingStatus.Operating));
            return Task.CompletedTask;
        }

        Task IOutBookingGrain.FinishNote(string bookingNumber)
        {
            DobOutBookingNote note = GetNote(bookingNumber);
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
