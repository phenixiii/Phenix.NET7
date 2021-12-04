using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;
using Phenix.TPT.Business.Norm;

/* 
   builder:    phenixiii
   build time: 2021-09-17 15:44:27
   mapping to: PT7_PROJECT_INFO 项目资料
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 项目资料
    /// </summary>
    [Serializable]
    public class ProjectInfo : ProjectInfo<ProjectInfo>
    {
    }

    /// <summary>
    /// 项目资料
    /// </summary>
    [Serializable]
    [Display(Description = @"项目资料")]
    [Sheet("PT7_PROJECT_INFO", PrimaryKeyName = "PI_ID")]
    public abstract class ProjectInfo<T> : EntityBase<T>
        where T : ProjectInfo<T>
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

        private decimal _contAmount;
        /// <summary>
        /// 合同金额
        /// </summary>
        [Display(Description = @"合同金额")]
        [Column("PI_CONT_AMOUNT")]
        public decimal ContAmount
        {
            get { return _contAmount; }
            set { _contAmount = value; }
        }

        private decimal _contMargin;
        /// <summary>
        /// 合同毛利（PI-PO）
        /// </summary>
        [Display(Description = @"合同毛利（PI-PO）")]
        [Column("PI_CONT_MARGIN")]
        public decimal ContMargin
        {
            get { return _contMargin; }
            set { _contMargin = value; }
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

        private decimal _productPrice;
        /// <summary>
        /// 产品价格
        /// </summary>
        [Display(Description = @"产品价格")]
        [Column("PI_PRODUCT_PRICE")]
        public decimal ProductPrice
        {
            get { return _productPrice; }
            set { _productPrice = value; }
        }

        private decimal _maintenancePrice;
        /// <summary>
        /// 质保/维保价格
        /// </summary>
        [Display(Description = @"质保/维保价格")]
        [Column("PI_MAINTENANCE_PRICE")]
        public decimal MaintenancePrice
        {
            get { return _maintenancePrice; }
            set { _maintenancePrice = value; }
        }

        private string _contPayClause;
        /// <summary>
        /// 付款条款
        /// </summary>
        [Display(Description = @"付款条款")]
        [Column("PI_CONT_PAY_CLAUSE")]
        public string ContPayClause
        {
            get { return _contPayClause; }
            set { _contPayClause = value; }
        }

        private string _contBreachClause;
        /// <summary>
        /// 违约条款
        /// </summary>
        [Display(Description = @"违约条款")]
        [Column("PI_CONT_BREACH_CLAUSE")]
        public string ContBreachClause
        {
            get { return _contBreachClause; }
            set { _contBreachClause = value; }
        }

        private string _contSealingClause;
        /// <summary>
        /// 封闭性条款
        /// </summary>
        [Display(Description = @"封闭性条款")]
        [Column("PI_CONT_SEALING_CLAUSE")]
        public string ContSealingClause
        {
            get { return _contSealingClause; }
            set { _contSealingClause = value; }
        }

        private string _contDefensiveClause;
        /// <summary>
        /// 防御性条款
        /// </summary>
        [Display(Description = @"防御性条款")]
        [Column("PI_CONT_DEFENSIVE_CLAUSE")]
        public string ContDefensiveClause
        {
            get { return _contDefensiveClause; }
            set { _contDefensiveClause = value; }
        }

        private string _contDurationClause;
        /// <summary>
        /// 里程碑要求
        /// </summary>
        [Display(Description = @"里程碑要求")]
        [Column("PI_CONT_DURATION_CLAUSE")]
        public string ContDurationClause
        {
            get { return _contDurationClause; }
            set { _contDurationClause = value; }
        }

        private string _contAcceptanceClause;
        /// <summary>
        /// 验收条款
        /// </summary>
        [Display(Description = @"验收条款")]
        [Column("PI_CONT_ACCEPTANCE_CLAUSE")]
        public string ContAcceptanceClause
        {
            get { return _contAcceptanceClause; }
            set { _contAcceptanceClause = value; }
        }

        private string _contAcceptanceDocs;
        /// <summary>
        /// 验收文档清单
        /// </summary>
        [Display(Description = @"验收文档清单")]
        [Column("PI_CONT_ACCEPTANCE_DOCS")]
        public string ContAcceptanceDocs
        {
            get { return _contAcceptanceDocs; }
            set { _contAcceptanceDocs = value; }
        }

        private string _contDocsPath;
        /// <summary>
        /// 合同文档路径
        /// </summary>
        [Display(Description = @"合同文档路径")]
        [Column("PI_CONT_DOCS_PATH")]
        public string ContDocsPath
        {
            get { return _contDocsPath; }
            set { _contDocsPath = value; }
        }

        private DateTime? _onlinePlanDate;
        /// <summary>
        /// 计划上线
        /// </summary>
        [Display(Description = @"计划上线")]
        [Column("PI_ONLINE_PLAN_DATE")]
        public DateTime? OnlinePlanDate
        {
            get { return _onlinePlanDate; }
            set { _onlinePlanDate = value; }
        }

        private DateTime? _onlineActualDate;
        /// <summary>
        /// 实际上线
        /// </summary>
        [Display(Description = @"实际上线")]
        [Column("PI_ONLINE_ACTUAL_DATE")]
        public DateTime? OnlineActualDate
        {
            get { return _onlineActualDate; }
            set { _onlineActualDate = value; }
        }

        private DateTime? _acceptDate;
        /// <summary>
        /// 验收时间
        /// </summary>
        [Display(Description = @"验收时间")]
        [Column("PI_ACCEPT_DATE")]
        public DateTime? AcceptDate
        {
            get { return _acceptDate; }
            set { _acceptDate = value; }
        }

        private int? _estimateWorkload;
        /// <summary>
        /// 估算工作量
        /// </summary>
        [Display(Description = @"估算工作量")]
        [Column("PI_ESTIMATE_WORKLOAD")]
        public int? EstimateWorkload
        {
            get { return _estimateWorkload; }
            set { _estimateWorkload = value; }
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

        private long _originator;
        /// <summary>
        /// 制单人
        /// </summary>
        [Display(Description = @"制单人")]
        [Column("PI_ORIGINATOR")]
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
        [Column("PI_ORIGINATE_TIME")]
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
        [Column("PI_ORIGINATE_TEAMS")]
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
        [Column("PI_UPDATER")]
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
        [Column("PI_UPDATE_TIME")]
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

        private decimal _totalReceivables;
        /// <summary>
        /// 应收总额
        /// </summary>
        [Display(Description = @"应收总额")]
        [Column("PI_TOTAL_RECEIVABLES")]
        public decimal TotalReceivables
        {
            get { return _totalReceivables; }
            set { _totalReceivables = value; }
        }

        private decimal _totalInvoiceAmount;
        /// <summary>
        /// 开票总额
        /// </summary>
        [Display(Description = @"开票总额")]
        [Column("PI_TOTAL_INVOICE_AMOUNT")]
        public decimal TotalInvoiceAmount
        {
            get { return _totalInvoiceAmount; }
            set { _totalInvoiceAmount = value; }
        }

        private decimal _totalReimbursementAmount;
        /// <summary>
        /// 报销总额
        /// </summary>
        [Display(Description = @"报销总额")]
        [Column("PI_TOTAL_REIMBURSEMENT_AMOUNT")]
        public decimal TotalReimbursementAmount
        {
            get { return _totalReimbursementAmount; }
            set { _totalReimbursementAmount = value; }
        }

    }
}
