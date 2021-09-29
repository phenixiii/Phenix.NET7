using System;
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
        /// <summary>
        /// 关闭项目
        /// </summary>
        Task Close(DateTime closedDate);

        #region 项目年度计划

        /// <summary>
        /// 获取所有年度计划
        /// </summary>
        Task<IList<ProjectAnnualPlan>> GetAllProjectAnnualPlan();

        /// <summary>
        /// 获取当年年度计划(如不存在则新增)
        /// </summary>
        Task<ProjectAnnualPlan> GetProjectAnnualPlan(int year);

        /// <summary>
        /// 更新年度计划(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutProjectAnnualPlan(ProjectAnnualPlan source);

        #endregion

        #region 项目月报

        /// <summary>
        /// 获取所有项目月报
        /// </summary>
        Task<IList<ProjectMonthlyReport>> GetAllProjectMonthlyReport();

        /// <summary>
        /// 获取当月项目月报(如不存在则新增)
        /// </summary>
        Task<ProjectMonthlyReport> GetProjectMonthlyReport(int year, int month);

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

        #endregion
    }
}