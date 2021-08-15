using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-06 15:36:14
   mapping to: PT7_PROJECT_EXPENSES 项目开支
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 项目开支
    /// </summary>
    [Serializable]
    public class ProjectExpenses : ProjectExpenses<ProjectExpenses>
    {
    }

    /// <summary>
    /// 项目开支
    /// </summary>
    [Serializable]
    [Display(Description = @"项目开支")]
    [Sheet("PT7_PROJECT_EXPENSES", PrimaryKeyName = "PE_ID")]
    public abstract class ProjectExpenses<T> : EntityBase<T>
        where T : ProjectExpenses<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected ProjectExpenses()
        {
            // used to fetch object, do not add code
        }

        /// <summary>
        /// 项目开支
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected ProjectExpenses(string dataSourceKey,
            long id, long piId, decimal reimbursementAmount, DateTime reimbursementDate, string remark, string reimbursementApplicant, string reimbursementVerifier, long originator, DateTime originateTime, long updater, DateTime updateTime) 
            : base(dataSourceKey)
        {
            _id = id;
            _piId = piId;
            _reimbursementAmount = reimbursementAmount;
            _reimbursementDate = reimbursementDate;
            _remark = remark;
            _reimbursementApplicant = reimbursementApplicant;
            _reimbursementVerifier = reimbursementVerifier;
            _originator = originator;
            _originateTime = originateTime;
            _updater = updater;
            _updateTime = updateTime;
        }

        protected override void InitializeSelf()
        {
        }

        private long _id;
        /// <summary>
        /// 
        /// </summary>
        [Display(Description = @"")]
        [Column("PE_ID")]
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
        [Column("PE_PI_ID")]
        public long PiId
        {
            get { return _piId; }
            set { _piId = value; }
        }

        private decimal _reimbursementAmount;
        /// <summary>
        /// 报销金额
        /// </summary>
        [Display(Description = @"报销金额")]
        [Column("PE_REIMBURSEMENT_AMOUNT")]
        public decimal ReimbursementAmount
        {
            get { return _reimbursementAmount; }
            set { _reimbursementAmount = value; }
        }

        private DateTime _reimbursementDate;
        /// <summary>
        /// 报销日期
        /// </summary>
        [Display(Description = @"报销日期")]
        [Column("PE_REIMBURSEMENT_DATE")]
        public DateTime ReimbursementDate
        {
            get { return _reimbursementDate; }
            set { _reimbursementDate = value; }
        }

        private string _remark;
        /// <summary>
        /// 报销说明
        /// </summary>
        [Display(Description = @"报销说明")]
        [Column("PE_REMARK")]
        public string Remark
        {
            get { return _remark; }
            set { _remark = value; }
        }

        private string _reimbursementApplicant;
        /// <summary>
        /// 报销人
        /// </summary>
        [Display(Description = @"报销人")]
        [Column("PE_REIMBURSEMENT_APPLICANT")]
        public string ReimbursementApplicant
        {
            get { return _reimbursementApplicant; }
            set { _reimbursementApplicant = value; }
        }

        private string _reimbursementVerifier;
        /// <summary>
        /// 批准人
        /// </summary>
        [Display(Description = @"批准人")]
        [Column("PE_REIMBURSEMENT_VERIFIER")]
        public string ReimbursementVerifier
        {
            get { return _reimbursementVerifier; }
            set { _reimbursementVerifier = value; }
        }

        private long _originator;
        /// <summary>
        /// 制单人
        /// </summary>
        [Display(Description = @"制单人")]
        [Column("PE_ORIGINATOR")]
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
        [Column("PE_ORIGINATE_TIME")]
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
        [Column("PE_UPDATER")]
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
        [Column("PE_UPDATE_TIME")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

    }
}
