using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Business;
using Phenix.Mapper.Schema;
using Phenix.TPT.Plugin.Business.Norm;

/* 
   builder:    phenixiii
   build time: 2021-09-17 15:44:27
   mapping to: PT7_PROJECT_INFO 项目资料
   revision record: 
    1，删除不需要显示的属性
    2，删除序列化代码
    3，禁止序列化操作痕迹属性
*/

namespace Phenix.TPT.Plugin.Business
{
    /// <summary>
    /// 项目资料
    /// </summary>
    [Serializable]
    public class ProjectInfoS : ProjectInfoS<ProjectInfoS>
    {
    }

    /// <summary>
    /// 项目资料
    /// </summary>
    [Serializable]
    [Display(Description = @"项目资料")]
    [Sheet("PT7_PROJECT_INFO", PrimaryKeyName = "PI_ID")]
    public abstract class ProjectInfoS<T> : EntityBase<T>
        where T : ProjectInfoS<T>
    {
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

        private string _contNumber;
        /// <summary>
        /// 合同号/临时号
        /// </summary>
        [Display(Description = @"合同号/临时号")]
        [Column("PI_CONT_NUMBER")]
        public string ContNumber
        {
            get { return _contNumber; }
            set { _contNumber = value; }
        }

        private DateTime _contApproveDate;
        /// <summary>
        /// 合同流转日期
        /// </summary>
        [Display(Description = @"合同流转日期")]
        [Column("PI_CONT_APPROVE_DATE")]
        public DateTime ContApproveDate
        {
            get { return _contApproveDate; }
            set { _contApproveDate = value; }
        }

        private string _projectName;
        /// <summary>
        /// 项目名称
        /// </summary>
        [Display(Description = @"项目名称")]
        [Column("PI_PROJECT_NAME")]
        public string ProjectName
        {
            get { return _projectName; }
            set { _projectName = value; }
        }

        private ProjectType _projectType;
        /// <summary>
        /// 项目类型
        /// </summary>
        [Display(Description = @"项目类型")]
        [Column("PI_PROJECT_TYPE_FG")]
        public ProjectType ProjectType
        {
            get { return _projectType; }
            set { _projectType = value; }
        }

        private long _projectManager;
        /// <summary>
        /// 项目经理
        /// </summary>
        [Display(Description = @"项目经理")]
        [Column("PI_PROJECT_MANAGER")]
        public long ProjectManager
        {
            get { return _projectManager; }
            set { _projectManager = value; }
        }

        private long _developManager;
        /// <summary>
        /// 开发经理
        /// </summary>
        [Display(Description = @"开发经理")]
        [Column("PI_DEVELOP_MANAGER")]
        public long DevelopManager
        {
            get { return _developManager; }
            set { _developManager = value; }
        }

        private long _maintenanceManager;
        /// <summary>
        /// 维护经理
        /// </summary>
        [Display(Description = @"维护经理")]
        [Column("PI_MAINTENANCE_MANAGER")]
        public long MaintenanceManager
        {
            get { return _maintenanceManager; }
            set { _maintenanceManager = value; }
        }

        private long _salesManager;
        /// <summary>
        /// 客户经理
        /// </summary>
        [Display(Description = @"客户经理")]
        [Column("PI_SALES_MANAGER")]
        public long SalesManager
        {
            get { return _salesManager; }
            set { _salesManager = value; }
        }

        private string _salesArea;
        /// <summary>
        /// 销售区域
        /// </summary>
        [Display(Description = @"销售区域")]
        [Column("PI_SALES_AREA")]
        public string SalesArea
        {
            get { return _salesArea; }
            set { _salesArea = value; }
        }

        private string _customer;
        /// <summary>
        /// 服务客户
        /// </summary>
        [Display(Description = @"服务客户")]
        [Column("PI_CUSTOMER")]
        public string Customer
        {
            get { return _customer; }
            set { _customer = value; }
        }
        
        private string _productVersion;
        /// <summary>
        /// 产品版本
        /// </summary>
        [Display(Description = @"产品版本")]
        [Column("PI_PRODUCT_VERSION")]
        public string ProductVersion
        {
            get { return _productVersion; }
            set { _productVersion = value; }
        }

        private bool _manageWork;
        /// <summary>
        /// 项目管理工作
        /// </summary>
        [Display(Description = @"项目管理工作")]
        [Column("PI_MANAGE_WORK_FG")]
        public bool ManageWork
        {
            get { return _manageWork; }
            set { _manageWork = value; }
        }

        private bool _investigateWork;
        /// <summary>
        /// 调研分析工作
        /// </summary>
        [Display(Description = @"调研分析工作")]
        [Column("PI_INVESTIGATE_WORK_FG")]
        public bool InvestigateWork
        {
            get { return _investigateWork; }
            set { _investigateWork = value; }
        }

        private bool _developWork;
        /// <summary>
        /// 设计开发工作
        /// </summary>
        [Display(Description = @"设计开发工作")]
        [Column("PI_DEVELOP_WORK_FG")]
        public bool DevelopWork
        {
            get { return _developWork; }
            set { _developWork = value; }
        }

        private bool _testWork;
        /// <summary>
        /// 联调测试工作
        /// </summary>
        [Display(Description = @"联调测试工作")]
        [Column("PI_TEST_WORK_FG")]
        public bool TestWork
        {
            get { return _testWork; }
            set { _testWork = value; }
        }

        private bool _implementWork;
        /// <summary>
        /// 培训实施工作
        /// </summary>
        [Display(Description = @"培训实施工作")]
        [Column("PI_IMPLEMENT_WORK_FG")]
        public bool ImplementWork
        {
            get { return _implementWork; }
            set { _implementWork = value; }
        }

        private bool _maintenanceWork;
        /// <summary>
        /// 质保维保工作
        /// </summary>
        [Display(Description = @"质保维保工作")]
        [Column("PI_MAINTENANCE_WORK_FG")]
        public bool MaintenanceWork
        {
            get { return _maintenanceWork; }
            set { _maintenanceWork = value; }
        }

        [NonSerialized]
        private DateTime _originateTime;
        /// <summary>
        /// 制单时间
        /// </summary>
        [Display(Description = @"制单时间")]
        [Column("PI_ORIGINATE_TIME")]
        [Newtonsoft.Json.JsonIgnore]
        public DateTime OriginateTime
        {
            get { return _originateTime; }
            set { _originateTime = value; }
        }

        [NonSerialized]
        private DateTime _updateTime;
        /// <summary>
        /// 更新时间
        /// </summary>
        [Display(Description = @"更新时间")]
        [Column("PI_UPDATE_TIME")]
        [Newtonsoft.Json.JsonIgnore]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

        private DateTime? _closedDate;
        /// <summary>
        /// 关闭日期
        /// </summary>
        [Display(Description = @"关闭日期")]
        [Column("PI_CLOSED_DATE")]
        public DateTime? ClosedDate
        {
            get { return _closedDate; }
            set { _closedDate = value; }
        }

        private string _currentStatus;
        /// <summary>
        /// 当前状态
        /// </summary>
        [Display(Description = @"当前状态")]
        [Column("PI_CURRENT_STATUS")]
        public string CurrentStatus
        {
            get { return _currentStatus; }
            set { _currentStatus = value; }
        }

        private string _annualMilestone;
        /// <summary>
        /// 年里程碑
        /// </summary>
        [Display(Description = @"年里程碑")]
        [Column("PI_ANNUAL_MILESTONE")]
        public string AnnualMilestone
        {
            get { return _annualMilestone; }
            set { _annualMilestone = value; }
        }
    }
}
