using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-06 15:36:14
   mapping to: PT7_PROJECT_PROCEEDS 项目收款
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 项目收款
    /// </summary>
    [Serializable]
    public class ProjectProceeds : ProjectProceeds<ProjectProceeds>
    {
    }

    /// <summary>
    /// 项目收款
    /// </summary>
    [Serializable]
    [Display(Description = @"项目收款")]
    [Sheet("PT7_PROJECT_PROCEEDS", PrimaryKeyName = "PP_ID")]
    public abstract class ProjectProceeds<T> : EntityBase<T>
        where T : ProjectProceeds<T>
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
        [Column("PP_ID")]
        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private long _piId;
        /// <summary>
        /// 项目资料
        /// </summary>
        [Display(Description = @"项目资料")]
        [Column("PP_PI_ID")]
        public long PiId
        {
            get { return _piId; }
            set { _piId = value; }
        }

        private decimal _invoiceAmount;
        /// <summary>
        /// 开票金额
        /// </summary>
        [Display(Description = @"开票金额")]
        [Column("PP_INVOICE_AMOUNT")]
        public decimal InvoiceAmount
        {
            get { return _invoiceAmount; }
            set { _invoiceAmount = value; }
        }

        private DateTime _invoiceDate;
        /// <summary>
        /// 开票日期
        /// </summary>
        [Display(Description = @"开票日期")]
        [Column("PP_INVOICE_DATE")]
        public DateTime InvoiceDate
        {
            get { return _invoiceDate; }
            set { _invoiceDate = value; }
        }

        private string _remark;
        /// <summary>
        /// 开票说明
        /// </summary>
        [Display(Description = @"开票说明")]
        [Column("PP_REMARK")]
        public string Remark
        {
            get { return _remark; }
            set { _remark = value; }
        }

        private long _originator;
        /// <summary>
        /// 制单人
        /// </summary>
        [Display(Description = @"制单人")]
        [Column("PP_ORIGINATOR")]
        public long Originator
        {
            get { return _originator; }
            set { _originator = value; }
        }

        private DateTime _originateTime;
        /// <summary>
        /// 制单时间
        /// </summary>
        [Display(Description = @"制单时间")]
        [Column("PP_ORIGINATE_TIME")]
        public DateTime OriginateTime
        {
            get { return _originateTime; }
            set { _originateTime = value; }
        }

        private long _updater;
        /// <summary>
        /// 更新人
        /// </summary>
        [Display(Description = @"更新人")]
        [Column("PP_UPDATER")]
        public long Updater
        {
            get { return _updater; }
            set { _updater = value; }
        }

        private DateTime _updateTime;
        /// <summary>
        /// 更新时间
        /// </summary>
        [Display(Description = @"更新时间")]
        [Column("PP_UPDATE_TIME")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

    }
}
