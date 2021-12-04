using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-13 11:11:23
   mapping to: PT7_WORKDAY 工作日
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 工作日
    /// </summary>
    [Serializable]
    public class Workday : Workday<Workday>
    {
    }

    /// <summary>
    /// 工作日
    /// </summary>
    [Serializable]
    [Display(Description = @"工作日")]
    [Sheet("PT7_WORKDAY", PrimaryKeyName = "WD_ID")]
    public abstract class Workday<T> : EntityBase<T>
        where T : Workday<T>
    {
        /// <summary>
        /// initialize self
        /// </summary>
        protected override void InitializeSelf()
        {
        }

        private long _id;
        /// <summary>
        /// 
        /// </summary>
        [Display(Description = @"")]
        [Column("WD_ID")]
        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private short _year;
        /// <summary>
        /// 年
        /// </summary>
        [Display(Description = @"年")]
        [Column("WD_YEAR_WM")]
        public short Year
        {
            get { return _year; }
            set { _year = value; }
        }

        private short _month;
        /// <summary>
        /// 月
        /// </summary>
        [Display(Description = @"月")]
        [Column("WD_MONTH_WM")]
        public short Month
        {
            get { return _month; }
            set { _month = value; }
        }

        private short _days;
        /// <summary>
        /// 法定工作日
        /// </summary>
        [Display(Description = @"法定工作日")]
        [Column("WD_DAYS")]
        public short Days
        {
            get { return _days; }
            set { _days = value; }
        }

    }
}
