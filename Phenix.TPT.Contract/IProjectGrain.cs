using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;
using Phenix.TPT.Business;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 项目Grain接口
    /// key: ID
    /// </summary>
    public interface IProjectGrain : IEntityGrain<ProjectInfo>, IGrainWithIntegerKey
    {
        #region 项目年度计划

        /// <summary>
        /// 获取所有年度计划
        /// </summary>
        Task<IList<ProjectAnnualPlan>> GetAllProjectAnnualPlan();

        /// <summary>
        /// 获取当前年度计划
        /// </summary>
        Task<ProjectAnnualPlan> GetNowProjectAnnualPlan();

        /// <summary>
        /// 更新年度计划(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutProjectAnnualPlan(ProjectAnnualPlan source);

        /// <summary>
        /// 获取应收总额
        /// </summary>
        Task<decimal> GetTotalReceivables();

        #endregion

        #region 项目月报

        /// <summary>
        /// 获取所有项目月报
        /// </summary>
        Task<IList<ProjectMonthlyReport>> GetAllProjectMonthlyReport();

        /// <summary>
        /// 获取当前项目月报
        /// </summary>
        Task<ProjectMonthlyReport> GetNowProjectMonthlyReport();

        /// <summary>
        /// 更新项目月报(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutProjectMonthlyReport(ProjectMonthlyReport source);

        #endregion

        #region 项目收款

        /// <summary>
        /// 获取所有项目收款
        /// </summary>
        Task<IList<ProjectProceeds>> GetAllProjectProceeds();

        /// <summary>
        /// 新增项目收款
        /// </summary>
        /// <param name="source">数据源</param>
        Task PostProjectProceeds(ProjectProceeds source);

        /// <summary>
        /// 删除项目收款
        /// </summary>
        Task DeleteProjectProceeds(long id);

        /// <summary>
        /// 获取开票总额
        /// </summary>
        Task<decimal> GetTotalInvoiceAmount();

        #endregion

        #region 项目开支

        /// <summary>
        /// 获取所有项目开支
        /// </summary>
        Task<IList<ProjectExpenses>> GetAllProjectExpenses();

        /// <summary>
        /// 新增项目开支
        /// </summary>
        /// <param name="source">数据源</param>
        Task PostProjectExpenses(ProjectExpenses source);

        /// <summary>
        /// 删除项目开支
        /// </summary>
        Task DeleteProjectExpenses(long id);

        /// <summary>
        /// 获取报销总额
        /// </summary>
        Task<decimal> GetTotalReimbursementAmount();

        #endregion

        #region 项目工作量

        #endregion
    }
}