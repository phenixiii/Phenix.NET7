using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-17 08:07:22
   mapping to: PT7_PROJECT_WORKER_V 项目工作人员
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 项目工作人员
    /// </summary>
    [Serializable]
    public class ProjectWorkerV : ProjectWorkerV<ProjectWorkerV>
    {
    }

    /// <summary>
    /// 项目工作人员
    /// </summary>
    [Serializable]
    [Display(Description = @"项目工作人员")]
    [Sheet("PT7_PROJECT_WORKER_V", PrimaryKeyName = "PI_ID")]
    public abstract class ProjectWorkerV<T> : EntityBase<T>
        where T : ProjectWorkerV<T>
    {
        protected override void InitializeSelf()
        {
        }

        private long _id;
        /// <summary>
        /// 
        /// </summary>
        [Display(Description = @"")]
        [Column("PI_ID")]
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
        [Column("WS_YEAR_WM")]
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
        [Column("WS_MONTH_WM")]
        public short Month
        {
            get { return _month; }
            set { _month = value; }
        }

        private long _worker;
        /// <summary>
        /// 工作人员
        /// </summary>
        [Display(Description = @"工作人员")]
        [Column("WW_WORKER")]
        public long Worker
        {
            get { return _worker; }
            set { _worker = value; }
        }

    }
}
