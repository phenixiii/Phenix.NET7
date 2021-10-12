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
        /// <param name="closedDate">关闭日期</param>
        Task Close(DateTime closedDate);

        #region 项目年度计划

        /// <summary>
        /// 获取项目年度计划(如不存在则返回初始对象)
        /// <param name="year">年</param>
        /// </summary>
        Task<ProjectAnnualPlan> GetProjectAnnualPlan(short year);

        /// <summary>
        /// 更新项目年度计划(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutProjectAnnualPlan(ProjectAnnualPlan source);

        #endregion

        #region 项目月报

        /// <summary>
        /// 获取项目月报(如不存在则返回初始对象)
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        Task<ProjectMonthlyReport> GetProjectMonthlyReport(short year, short month);

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