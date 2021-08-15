using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-09 17:34:10
   mapping to: PT7_PROJECT_WORKLOAD 项目工作量
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 项目工作量
    /// </summary>
    [Serializable]
    public class ProjectWorkload : ProjectWorkload<ProjectWorkload>
    {
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
        /// for CreateInstance
        /// </summary>
        protected ProjectWorkload()
        {
            // used to fetch object, do not add code
        }

        /// <summary>
        /// 项目工作量
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected ProjectWorkload(string dataSourceKey,
            long id, long piId, short year, short month, string worker, short manageWorkloadRole, short investigateWorkloadRole, short developWorkloadRole, short testWorkloadRole, short implementWorkloadRole, short maintenanceWorkloadRole, long originator, DateTime originateTime, long updater, DateTime updateTime) 
            : base(dataSourceKey)
        {
            _id = id;
            _piId = piId;
            _year = year;
            _month = month;
            _worker = worker;
            _manageWorkloadRole = manageWorkloadRole;
            _investigateWorkloadRole = investigateWorkloadRole;
            _developWorkloadRole = developWorkloadRole;
            _testWorkloadRole = testWorkloadRole;
            _implementWorkloadRole = implementWorkloadRole;
            _maintenanceWorkloadRole = maintenanceWorkloadRole;
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
        [Column("PW_ID")]
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
        [Column("PW_PI_ID_WM")]
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

        private string _worker;
        /// <summary>
        /// 工作人员
        /// </summary>
        [Display(Description = @"工作人员")]
        [Column("PW_WORKER_WM")]
        public string Worker
        {
            get { return _worker; }
            set { _worker = value; }
        }

        private short _manageWorkloadRole;
        /// <summary>
        /// 项目管理人天
        /// </summary>
        [Display(Description = @"项目管理人天")]
        [Column("PW_MANAGE_WORKLOAD_ROLE")]
        public short ManageWorkloadRole
        {
            get { return _manageWorkloadRole; }
            set { _manageWorkloadRole = value; }
        }

        private short _investigateWorkloadRole;
        /// <summary>
        /// 调研分析人天
        /// </summary>
        [Display(Description = @"调研分析人天")]
        [Column("PW_INVESTIGATE_WORKLOAD_ROLE")]
        public short InvestigateWorkloadRole
        {
            get { return _investigateWorkloadRole; }
            set { _investigateWorkloadRole = value; }
        }

        private short _developWorkloadRole;
        /// <summary>
        /// 设计开发人天
        /// </summary>
        [Display(Description = @"设计开发人天")]
        [Column("PW_DEVELOP_WORKLOAD_ROLE")]
        public short DevelopWorkloadRole
        {
            get { return _developWorkloadRole; }
            set { _developWorkloadRole = value; }
        }

        private short _testWorkloadRole;
        /// <summary>
        /// 联调测试人天
        /// </summary>
        [Display(Description = @"联调测试人天")]
        [Column("PW_TEST_WORKLOAD_ROLE")]
        public short TestWorkloadRole
        {
            get { return _testWorkloadRole; }
            set { _testWorkloadRole = value; }
        }

        private short _implementWorkloadRole;
        /// <summary>
        /// 培训实施人天
        /// </summary>
        [Display(Description = @"培训实施人天")]
        [Column("PW_IMPLEMENT_WORKLOAD_ROLE")]
        public short ImplementWorkloadRole
        {
            get { return _implementWorkloadRole; }
            set { _implementWorkloadRole = value; }
        }

        private short _maintenanceWorkloadRole;
        /// <summary>
        /// 质保维保人天
        /// </summary>
        [Display(Description = @"质保维保人天")]
        [Column("PW_MAINTENANCE_WORKLOAD_ROLE")]
        public short MaintenanceWorkloadRole
        {
            get { return _maintenanceWorkloadRole; }
            set { _maintenanceWorkloadRole = value; }
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
