using System;
using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2020-07-03 20:53:33
   mapping to: dob_booking_div_time
*/

namespace Demo.IDOS.Plugin.Business.OnlineBooking
{
    /// <summary>
    /// 分时预约
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = "分时预约")]
    public class DobBookingDivTime : EntityBase<DobBookingDivTime>
    {
        private DobBookingDivTime()
        {
            // used to fetch object, do not add code
        }

        [Newtonsoft.Json.JsonConstructor]
        public DobBookingDivTime(long id, long dpId, DateTime date, short dateTimeSlot) 
        {
            _id = id;
            _dpId = dpId;
            _date = date;
            _dateTimeSlot = dateTimeSlot;
        }

        protected override void InitializeSelf()
        {
        }

        private long _dpId;
        /// <summary>
        /// 仓库
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "仓库")]
        public long DpId
        {
            get { return _dpId; }
            set { _dpId = value; }
        }

        private DateTime _date;
        /// <summary>
        /// 日期
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "日期")]
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        private short _dateTimeSlot;
        /// <summary>
        /// 时间段
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "时间段")]
        public short DateTimeSlot
        {
            get { return _dateTimeSlot; }
            set { _dateTimeSlot = value; }
        }

    }
}
