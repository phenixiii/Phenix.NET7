using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-09 17:34:10
   mapping to: PT7_PROJECT_MONTHLY_REPORT 项目月报
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 项目月报
    /// </summary>
    [Serializable]
    public class ProjectMonthlyReport : ProjectMonthlyReport<ProjectMonthlyReport>
    {
    }

    /// <summary>
    /// 项目月报
    /// </summary>
    [Serializable]
    [Display(Description = @"项目月报")]
    [Sheet("PT7_PROJECT_MONTHLY_REPORT", PrimaryKeyName = "PM_ID")]
    public abstract class ProjectMonthlyReport<T> : EntityBase<T>
        where T : ProjectMonthlyReport<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected ProjectMonthlyReport()
        {
            // used to fetch object, do not add code
        }

        /// <summary>
        /// 项目月报
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected ProjectMonthlyReport(string dataSourceKey,
            long id, long piId, short year, short month, string status, string monthlyPlan, string monthlyAchieve, string nextMonthlyPlan, string riskCaution, string demandCoordination, long originator, DateTime originateTime, long updater, DateTime updateTime) 
            : base(dataSourceKey)
        {
            _id = id;
            _piId = piId;
            _year = year;
            _month = month;
            _status = status;
            _monthlyPlan = monthlyPlan;
            _monthlyAchieve = monthlyAchieve;
            _nextMonthlyPlan = nextMonthlyPlan;
            _riskCaution = riskCaution;
            _demandCoordination = demandCoordination;
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
        [Column("PM_ID")]
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
        [Column("PM_PI_ID_WM")]
        public long PiId
        {
            get { return _piId; }
            set { _piId = value; }
        }

        private short _year;
        /// <summary>
        /// 年
        /// </summary>
        [Display(Description = @"年")]
        [Column("PM_YEAR_WM")]
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
        [Column("PM_MONTH_WM")]
        public short Month
        {
            get { return _month; }
            set { _month = value; }
        }

        private string _status;
        /// <summary>
        /// 状态
        /// </summary>
        [Display(Description = @"状态")]
        [Column("PM_STATUS")]
        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private string _monthlyPlan;
        /// <summary>
        /// 月度计划
        /// </summary>
        [Display(Description = @"月度计划")]
        [Column("PM_MONTHLY_PLAN")]
        public string MonthlyPlan
        {
            get { return _monthlyPlan; }
            set { _monthlyPlan = value; }
        }

        private string _monthlyAchieve;
        /// <summary>
        /// 实绩完成
        /// </summary>
        [Display(Description = @"实绩完成")]
        [Column("PM_MONTHLY_ACHIEVE")]
        public string MonthlyAchieve
        {
            get { return _monthlyAchieve; }
            set { _monthlyAchieve = value; }
        }

        private string _nextMonthlyPlan;
        /// <summary>
        /// 下月计划
        /// </summary>
        [Display(Description = @"下月计划")]
        [Column("PM_NEXT_MONTHLY_PLAN")]
        public string NextMonthlyPlan
        {
            get { return _nextMonthlyPlan; }
            set { _nextMonthlyPlan = value; }
        }

        private string _riskCaution;
        /// <summary>
        /// 风险警示
        /// </summary>
        [Display(Description = @"风险警示")]
        [Column("PM_RISK_CAUTION")]
        public string RiskCaution
        {
            get { return _riskCaution; }
            set { _riskCaution = value; }
        }

        private string _demandCoordination;
        /// <summary>
        /// 协调需求
        /// </summary>
        [Display(Description = @"协调需求")]
        [Column("PM_DEMAND_COORDINATION")]
        public string DemandCoordination
        {
            get { return _demandCoordination; }
            set { _demandCoordination = value; }
        }

        private long _originator;
        /// <summary>
        /// 制单人
        /// </summary>
        [Display(Description = @"制单人")]
        [Column("PM_ORIGINATOR")]
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
        [Column("PM_ORIGINATE_TIME")]
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
        [Column("PM_UPDATER")]
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
        [Column("PM_UPDATE_TIME")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

    }
}
