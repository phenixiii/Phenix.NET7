using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-17 15:15:24
   mapping to: PT7_PROJECT_WORKLOAD 项目工作量
   revision record: 
    1，添加TotalWorkload属性用于统计工作量
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 项目工作量
    /// </summary>
    [Serializable]
    public class ProjectWorkload : ProjectWorkload<ProjectWorkload>
    {
        /// <summary>
        /// 人天合计
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public int TotalWorkload
        {
            get { return ManageWorkload + InvestigateWorkload + DevelopWorkload + TestWorkload + ImplementWorkload + MaintenanceWorkload; }
        }
    }

    /// <summary>
    /// 项目工作量
    /// </summary>
    [Serializable]
    [Display(Description = @"项目工作量")]
    [Sheet("PT7_PROJECT_WORKLOAD", PrimaryKeyName = "PW_ID")]
    public abstract class ProjectWorkload<T> : EntityBase<T>
        where T : ProjectWorkload<T>
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
        [Column("PW_ID")]
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
        [Column("PW_YEAR_WM")]
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
        [Column("PW_MONTH_WM")]
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
        [Column("PW_WORKER_WM")]
        public long Worker
        {
            get { return _worker; }
            set { _worker = value; }
        }

        private long _piId;
        /// <summary>
        /// 项目资料
        /// </summary>
        [Display(Description = @"项目资料")]
        [Column("PW_PI_ID_WM")]
        public long PiId
        {
            get { return _piId; }
            set { _piId = value; }
        }

        private short _manageWorkload;
        /// <summary>
        /// 项目管理人天
        /// </summary>
        [Display(Description = @"项目管理人天")]
        [Column("PW_MANAGE_WORKLOAD")]
        public short ManageWorkload
        {
            get { return _manageWorkload; }
            set { _manageWorkload = value; }
        }

        private short _investigateWorkload;
        /// <summary>
        /// 调研分析人天
        /// </summary>
        [Display(Description = @"调研分析人天")]
        [Column("PW_INVESTIGATE_WORKLOAD")]
        public short InvestigateWorkload
        {
            get { return _investigateWorkload; }
            set { _investigateWorkload = value; }
        }

        private short _developWorkload;
        /// <summary>
        /// 设计开发人天
        /// </summary>
        [Display(Description = @"设计开发人天")]
        [Column("PW_DEVELOP_WORKLOAD")]
        public short DevelopWorkload
        {
            get { return _developWorkload; }
            set { _developWorkload = value; }
        }

        private short _testWorkload;
        /// <summary>
        /// 联调测试人天
        /// </summary>
        [Display(Description = @"联调测试人天")]
        [Column("PW_TEST_WORKLOAD")]
        public short TestWorkload
        {
            get { return _testWorkload; }
            set { _testWorkload = value; }
        }

        private short _implementWorkload;
        /// <summary>
        /// 培训实施人天
        /// </summary>
        [Display(Description = @"培训实施人天")]
        [Column("PW_IMPLEMENT_WORKLOAD")]
        public short ImplementWorkload
        {
            get { return _implementWorkload; }
            set { _implementWorkload = value; }
        }

        private short _maintenanceWorkload;
        /// <summary>
        /// 质保维保人天
        /// </summary>
        [Display(Description = @"质保维保人天")]
        [Column("PW_MAINTENANCE_WORKLOAD")]
        public short MaintenanceWorkload
        {
            get { return _maintenanceWorkload; }
            set { _maintenanceWorkload = value; }
        }

        private long _originator;
        /// <summary>
        /// 制单人
        /// </summary>
        [Display(Description = @"制单人")]
        [Column("PW_ORIGINATOR")]
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
        [Column("PW_ORIGINATE_TIME")]
        public DateTime OriginateTime
        {
            get { return _originateTime; }
            set { _originateTime = value; }
        }

        private long _originateTeams;
        /// <summary>
        /// 制单团体
        /// </summary>
        [Display(Description = @"制单团体")]
        [Column("PW_ORIGINATE_TEAMS")]
        public long OriginateTeams
        {
            get { return _originateTeams; }
            set { _originateTeams = value; }
        }

        private long _updater;
        /// <summary>
        /// 更新人
        /// </summary>
        [Display(Description = @"更新人")]
        [Column("PW_UPDATER")]
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
        [Column("PW_UPDATE_TIME")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

    }
}
