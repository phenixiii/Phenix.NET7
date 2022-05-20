using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Business;
using Phenix.Mapper.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-09 17:34:10
   mapping to: PT7_PROJECT_ANNUAL_PLAN 项目年度计划
*/

namespace Phenix.TPT.Plugin.Business
{
    /// <summary>
    /// 项目年度计划
    /// </summary>
    [Serializable]
    public class ProjectAnnualPlan : ProjectAnnualPlan<ProjectAnnualPlan>
    {
    }

    /// <summary>
    /// 项目年度计划
    /// </summary>
    [Serializable]
    [Display(Description = @"项目年度计划")]
    [Sheet("PT7_PROJECT_ANNUAL_PLAN", PrimaryKeyName = "PA_ID")]
    public abstract class ProjectAnnualPlan<T> : EntityBase<T>
        where T : ProjectAnnualPlan<T>
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
        [Column("PA_ID")]
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
        [Column("PA_PI_ID_WM")]
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
        [Column("PA_YEAR_WM")]
        public short Year
        {
            get { return _year; }
            set { _year = value; }
        }

        private string _annualMilestone;
        /// <summary>
        /// 年里程碑
        /// </summary>
        [Display(Description = @"年里程碑")]
        [Column("PA_ANNUAL_MILESTONE")]
        public string AnnualMilestone
        {
            get { return _annualMilestone; }
            set { _annualMilestone = value; }
        }

        private decimal _annualReceivables;
        /// <summary>
        /// 年应收款
        /// </summary>
        [Display(Description = @"年应收款")]
        [Column("PA_ANNUAL_RECEIVABLES")]
        public decimal AnnualReceivables
        {
            get { return _annualReceivables; }
            set { _annualReceivables = value; }
        }

        private string _annualPlan;
        /// <summary>
        /// 年度计划
        /// </summary>
        [Display(Description = @"年度计划")]
        [Column("PA_ANNUAL_PLAN")]
        public string AnnualPlan
        {
            get { return _annualPlan; }
            set { _annualPlan = value; }
        }

        private string _annualAchieve;
        /// <summary>
        /// 实绩完成
        /// </summary>
        [Display(Description = @"实绩完成")]
        [Column("PM_ANNUAL_ACHIEVE")]
        public string AnnualAchieve
        {
            get { return _annualAchieve; }
            set { _annualAchieve = value; }
        }

        private long _originator;
        /// <summary>
        /// 制单人
        /// </summary>
        [Display(Description = @"制单人")]
        [Column("PA_ORIGINATOR")]
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
        [Column("PA_ORIGINATE_TIME")]
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
        [Column("PA_UPDATER")]
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
        [Column("PA_UPDATE_TIME")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

    }
}
