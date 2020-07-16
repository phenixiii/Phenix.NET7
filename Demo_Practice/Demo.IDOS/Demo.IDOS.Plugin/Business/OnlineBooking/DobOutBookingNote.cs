using System;
using Demo.IDOS.Plugin.Rule;
using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2020-07-03 20:53:33
   mapping to: dob_out_booking_note
*/

namespace Demo.IDOS.Plugin.Business.OnlineBooking
{
    /// <summary>
    /// 出库预约单
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = "出库预约单")]
    public class DobOutBookingNote : EntityBase<DobOutBookingNote>
    {
        private DobOutBookingNote()
        {
            // used to fetch object, do not add code
        }

        [Newtonsoft.Json.JsonConstructor]
        public DobOutBookingNote(long id, long bdId, string bookingNumber, BookingStatus bookingStatus, long cmId, string goodsType, GoodsSize goodsSize, string goodsClass, string licensePlate, long originator, DateTime originateTime, long updater, DateTime updateTime) 
        {
            _id = id;
            _bdId = bdId;
            _bookingNumber = bookingNumber;
            _bookingStatus = bookingStatus;
            _cmId = cmId;
            _goodsType = goodsType;
            _goodsSize = goodsSize;
            _goodsClass = goodsClass;
            _licensePlate = licensePlate;
            _originator = originator;
            _originateTime = originateTime;
            _updater = updater;
            _updateTime = updateTime;
        }

        protected override void InitializeSelf()
        {
            _bookingStatus = 0;
        }

        private long _bdId;
        /// <summary>
        /// 分时预约
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "分时预约")]
        public long BdId
        {
            get { return _bdId; }
            set { _bdId = value; }
        }

        private string _bookingNumber;
        /// <summary>
        /// 预约单号
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "预约单号")]
        public string BookingNumber
        {
            get { return _bookingNumber; }
            set { _bookingNumber = value; }
        }

        private BookingStatus _bookingStatus;
        /// <summary>
        /// 预约单状态
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "预约单状态")]
        public BookingStatus BookingStatus
        {
            get { return _bookingStatus; }
            set { _bookingStatus = value; }
        }

        private long _cmId;
        /// <summary>
        /// 客户
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "客户")]
        public long CmId
        {
            get { return _cmId; }
            set { _cmId = value; }
        }

        private string _goodsType;
        /// <summary>
        /// 货物规格
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "货物规格")]
        public string GoodsType
        {
            get { return _goodsType; }
            set { _goodsType = value; }
        }

        private GoodsSize _goodsSize;
        /// <summary>
        /// 货物尺寸
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "货物尺寸")]
        public GoodsSize GoodsSize
        {
            get { return _goodsSize; }
            set { _goodsSize = value; }
        }

        private string _goodsClass;
        /// <summary>
        /// 货物等级
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "货物等级")]
        public string GoodsClass
        {
            get { return _goodsClass; }
            set { _goodsClass = value; }
        }

        private string _licensePlate;
        /// <summary>
        /// 车牌号
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "车牌号")]
        public string LicensePlate
        {
            get { return _licensePlate; }
            set { _licensePlate = value; }
        }

        private long _originator;
        /// <summary>
        /// 制单人
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "制单人")]
        public long Originator
        {
            get { return _originator; }
            set { _originator = value; }
        }

        private DateTime _originateTime;
        /// <summary>
        /// 制单时间
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "制单时间")]
        public DateTime OriginateTime
        {
            get { return _originateTime; }
            set { _originateTime = value; }
        }

        private long _updater;
        /// <summary>
        /// 更新人
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "更新人")]
        public long Updater
        {
            get { return _updater; }
            set { _updater = value; }
        }

        private DateTime _updateTime;
        /// <summary>
        /// 更新时间
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "更新时间")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

    }
}
