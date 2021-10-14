using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Security;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.TPT.Business;
using Phenix.TPT.Business.Norm;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目Grain
    /// key: ID
    /// </summary>
    public class ProjectGrain : EntityGrainBase<ProjectInfo>, IProjectGrain
    {
        #region 属性

        private IList<ProjectAnnualPlan> _projectAnnualPlanList;

        /// <summary>
        /// 项目年度计划清单
        /// </summary>
        protected IList<ProjectAnnualPlan> ProjectAnnualPlanList
        {
            get
            {
                return _projectAnnualPlanList ??= Kernel.FetchDetails(ProjectAnnualPlan.Descending(p => p.Year));
            }
        }

        private IList<ProjectMonthlyReport> _projectMonthlyReportList;

        /// <summary>
        /// 项目月报清单
        /// </summary>
        protected IList<ProjectMonthlyReport> ProjectMonthlyReportList
        {
            get
            {
                return _projectMonthlyReportList ??= Kernel.FetchDetails(ProjectMonthlyReport.Descending(p => p.Year).Descending(p => p.Month));
            }
        }

        private IList<ProjectProceeds> _projectProceedsList;

        /// <summary>
        /// 项目收款清单
        /// </summary>
        protected IList<ProjectProceeds> ProjectProceedsList
        {
            get
            {
                return _projectProceedsList ??= Kernel.FetchDetails(ProjectProceeds.Descending(p => p.InvoiceDate));
            }
        }

        private IList<ProjectExpenses> _projectExpensesList;

        /// <summary>
        /// 项目开支清单
        /// </summary>
        protected IList<ProjectExpenses> ProjectExpensesList
        {
            get
            {
                return _projectExpensesList ??= Kernel.FetchDetails(ProjectExpenses.Descending(p => p.ReimbursementDate));
            }
        }

        #endregion

        #region 方法

        private async Task<bool> IsMyProject()
        {
            return await User.Identity.IsInRole(ProjectRoles.经营管理) ||
                   Kernel == null && await User.Identity.IsInRole(ProjectRoles.项目管理) ||
                   Kernel != null && (
                       User.Identity.Id == Kernel.ProjectManager ||
                       User.Identity.Id == Kernel.DevelopManager ||
                       User.Identity.Id == Kernel.MaintenanceManager ||
                       User.Identity.Id == Kernel.SalesManager);
        }

        #region Stream

        private Task SendEventForRefreshProjectWorkloads(long receiver)
        {
            //通知receiver刷新项目工作量
            return ClusterClient.GetStreamProvider().GetStream<string>(StreamConfig.RefreshProjectWorkloadsStreamId, receiver.ToString()).OnNextAsync(receiver.ToString());
        }

        #endregion

        #region Kernel

        /// <summary>
        /// 操作根实体对象之前
        /// </summary>
        /// <param name="executeAction">执行动作</param>
        /// <param name="tag">标记</param>
        protected override void OnKernelOperating(ExecuteAction executeAction, out object tag)
        {
            tag = executeAction == ExecuteAction.Update
                ? new
                {
                    ProjectManager = Kernel.ProjectManager,
                    DevelopManager = Kernel.DevelopManager
                }
                : null;
        }

        /// <summary>
        /// 操作根实体对象之后
        /// </summary>
        /// <param name="executeAction">执行动作</param>
        /// <param name="tag">标记</param>
        protected override void OnKernelOperated(ExecuteAction executeAction, object tag)
        {
            if (executeAction == ExecuteAction.Update)
            {
                //项目负责人发生变更后需要通知到他们的Grain
                dynamic old = tag;
                if ((long) old.ProjectManager != Kernel.ProjectManager)
                {
                    SendEventForRefreshProjectWorkloads((long) old.ProjectManager);
                    SendEventForRefreshProjectWorkloads(Kernel.ProjectManager);
                }

                if ((long) old.DevelopManager != Kernel.DevelopManager)
                {
                    SendEventForRefreshProjectWorkloads((long) old.DevelopManager);
                    SendEventForRefreshProjectWorkloads(Kernel.DevelopManager);
                }
            }
            else
            {
                //（未开发但以防未来实现）项目删除后需要通知到他们的Grain
                SendEventForRefreshProjectWorkloads(Kernel.ProjectManager);
                SendEventForRefreshProjectWorkloads(Kernel.DevelopManager);
            }
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="autoNew">不存在则新增</param>
        protected override async Task<ProjectInfo> FetchKernel(bool autoNew = false)
        {
            if (Kernel != null)
                return Kernel;

            ProjectInfo result = await base.FetchKernel(autoNew);
            if (result != null)
            {
                result.Apply(NameValue.Set<ProjectInfo>(p => p.ContApproveDate, DateTime.Today).
                    Set(p => p.ProjectType, ProjectType.技术服务));
                if (await User.Identity.IsInRole(ProjectRoles.项目管理))
                    result.Apply(NameValue.Set<ProjectInfo>(p => p.ProjectManager, User.Identity.Id));
            }

            return result;
        }

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="source">数据源</param>
        protected override async Task PutKernel(ProjectInfo source)
        {
            if (!await IsMyProject())
                throw new SecurityException("非请毋动!");

            await base.PutKernel(source);
        }

        async Task IProjectGrain.Close(DateTime closedDate)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            if (!await IsMyProject())
                throw new SecurityException("非请毋删!");

            Kernel.UpdateSelf(NameValue.Set<ProjectInfo>(p => p.ClosedDate, closedDate));
        }

        #endregion

        #region 项目年度计划

        Task<ProjectAnnualPlan> IProjectGrain.GetProjectAnnualPlan(short year)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();
            
            foreach (ProjectAnnualPlan item in ProjectAnnualPlanList)
                if (item.Year == year)
                    return Task.FromResult(item);

            return Task.FromResult(Kernel.NewDetail(ProjectAnnualPlan.Set(p => p.Year, year).
                Set(p => p.AnnualReceivables, Kernel.ContAmount - Kernel.TotalReceivables)));
        }

        async Task IProjectGrain.PutProjectAnnualPlan(ProjectAnnualPlan source)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            if (!await IsMyProject())
                throw new SecurityException("非请毋动!");

            DateTime today = DateTime.Today;

            foreach (ProjectAnnualPlan item in ProjectAnnualPlanList)
                if (item.Year == source.Year)
                {
                    if (source.AnnualReceivables > Kernel.ContAmount - Kernel.TotalReceivables + item.AnnualReceivables)
                        throw new ValidationException(String.Format("本应收款({0})已超过{1}可报数额({2})!",
                            source.AnnualReceivables, Kernel.ProjectName, Kernel.ContAmount - Kernel.TotalReceivables + item.AnnualReceivables));

                    Database.Execute((DbTransaction dbTransaction) =>
                    {
                        item.UpdateSelf(dbTransaction, source);
                        if (today.Year == source.Year)
                            Kernel.UpdateSelf(dbTransaction,
                                NameValue.Set<ProjectInfo>(p => p.TotalReceivables, p => p.TotalReceivables - item.AnnualReceivables + source.AnnualReceivables).
                                    Set(p => p.AnnualMilestone, source.AnnualMilestone));
                        else
                            Kernel.UpdateSelf(dbTransaction,
                                NameValue.Set<ProjectInfo>(p => p.TotalReceivables, p => p.TotalReceivables - item.AnnualReceivables + source.AnnualReceivables));
                    });
                    return;
                }

            if (source.AnnualReceivables > Kernel.ContAmount - Kernel.TotalReceivables)
                throw new ValidationException(String.Format("本应收款({0})已超过{1}可报数额({2})!",
                    source.AnnualReceivables, Kernel.ProjectName, Kernel.ContAmount - Kernel.TotalReceivables));

            Database.Execute((DbTransaction dbTransaction) =>
            {
                source.InsertSelf(dbTransaction);
                if (today.Year == source.Year)
                    Kernel.UpdateSelf(dbTransaction,
                        NameValue.Set<ProjectInfo>(p => p.TotalReceivables, p => p.TotalReceivables + source.AnnualReceivables).
                            Set(p => p.AnnualMilestone, source.AnnualMilestone));
                else
                    Kernel.UpdateSelf(dbTransaction,
                        NameValue.Set<ProjectInfo>(p => p.TotalReceivables, p => p.TotalReceivables + source.AnnualReceivables));

            });
            ProjectAnnualPlanList.Add(source);
        }

        #endregion

        #region 项目月报

        Task<ProjectMonthlyReport> IProjectGrain.GetProjectMonthlyReport(short year, short month)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            ProjectMonthlyReport ultimoReport = null;
            DateTime ultimo = new DateTime(year, month, 1).AddMilliseconds(-1);
            foreach (ProjectMonthlyReport item in ProjectMonthlyReportList)
                if (item.Year == year && item.Month == month)
                    return Task.FromResult(item);
                else if (item.Year == ultimo.Year && item.Month == ultimo.Month)
                    ultimoReport = item;

            return Task.FromResult(Kernel.NewDetail(ProjectMonthlyReport.
                Set(p => p.Year, year).
                Set(p => p.Month, month).
                Set(p => p.Status, ultimoReport != null ? ultimoReport.Status : null).
                Set(p => p.MonthlyPlan, ultimoReport != null ? ultimoReport.NextMonthlyPlan : "*")));
        }

        async Task IProjectGrain.PutProjectMonthlyReport(ProjectMonthlyReport source)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            if (!await IsMyProject())
                throw new SecurityException("非请毋动!");

            if (source.Month < 1 || source.Month > 12)
                throw new ValidationException(String.Format("咱这可没{0}月份唉!", source.Month));

            DateTime today = DateTime.Today;
            if (today.Year < source.Year || today.Year == source.Year && today.Month < source.Month)
                throw new ValidationException("未来不可期~");

            foreach (ProjectMonthlyReport item in ProjectMonthlyReportList)
                if (item.Year == source.Year && item.Month == source.Month)
                {
                    Database.Execute((DbTransaction dbTransaction) =>
                    {
                        item.UpdateSelf(dbTransaction, source);
                        if (today.Year == source.Year && today.Month == source.Month)
                            Kernel.UpdateSelf(dbTransaction,
                                NameValue.Set<ProjectInfo>(p => p.CurrentStatus, source.Status));
                    });
                    return;
                }

            Database.Execute((DbTransaction dbTransaction) =>
            {
                source.InsertSelf(dbTransaction);
                if (today.Year == source.Year && today.Month == source.Month)
                    Kernel.UpdateSelf(dbTransaction,
                        NameValue.Set<ProjectInfo>(p => p.CurrentStatus, source.Status));
            });
            ProjectMonthlyReportList.Add(source);
        }

        #endregion

        #region 项目收款

        Task<IList<ProjectProceeds>> IProjectGrain.GetAllProjectProceeds()
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            return Task.FromResult(ProjectProceedsList);
        }

        async Task IProjectGrain.PostProjectProceeds(ProjectProceeds source)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            if (!await IsMyProject())
                throw new SecurityException("非请毋加!");

            if (source.InvoiceAmount > Kernel.ContAmount - Kernel.TotalInvoiceAmount)
                throw new ValidationException(String.Format("本开票金额({0})已超过{1}可开数额({2})!",
                    source.InvoiceAmount, Kernel.ProjectName, Kernel.ContAmount - Kernel.TotalInvoiceAmount));

            Database.Execute((DbTransaction dbTransaction) =>
            {
                source.InsertSelf(dbTransaction);
                Kernel.UpdateSelf(dbTransaction,
                    NameValue.Set<ProjectInfo>(p => p.TotalInvoiceAmount, p => p.TotalInvoiceAmount + source.InvoiceAmount));
            });
            ProjectProceedsList.Add(source);
        }

        async Task IProjectGrain.DeleteProjectProceeds(long id)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            if (!await IsMyProject())
                throw new SecurityException("非请毋删!");

            ProjectProceeds projectProceeds = null;
            foreach (ProjectProceeds item in ProjectProceedsList)
                if (item.Id == id)
                {
                    projectProceeds = item;
                    break;
                }

            if (projectProceeds == null)
                throw new ValidationException(String.Format("未找到 {0} 可删除的开票记录!", Kernel.ProjectName));

            Database.Execute((DbTransaction dbTransaction) =>
            {
                projectProceeds.DeleteSelf(dbTransaction);
                Kernel.UpdateSelf(dbTransaction,
                    NameValue.Set<ProjectInfo>(p => p.TotalInvoiceAmount, p => p.TotalInvoiceAmount - projectProceeds.InvoiceAmount));
            });
            ProjectProceedsList.Remove(projectProceeds);
        }

        #endregion

        #region 项目开支

        Task<IList<ProjectExpenses>> IProjectGrain.GetAllProjectExpenses()
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            return Task.FromResult(ProjectExpensesList);
        }

        Task IProjectGrain.PostProjectExpenses(ProjectExpenses source)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            Database.Execute((DbTransaction dbTransaction) =>
            {
                source.InsertSelf(dbTransaction);
                Kernel.UpdateSelf(dbTransaction,
                    NameValue.Set<ProjectInfo>(p => p.TotalReimbursementAmount, p => p.TotalReimbursementAmount + source.ReimbursementAmount));
            });
            ProjectExpensesList.Add(source);
            return Task.CompletedTask;
        }

        async Task IProjectGrain.DeleteProjectExpenses(long id)
        {
            if (Kernel == null)
                throw new ProjectNotFoundException();

            if (!await IsMyProject())
                throw new SecurityException("非请毋删!");

            ProjectExpenses projectExpenses = null;
            foreach (ProjectExpenses item in ProjectExpensesList)
                if (item.Id == id)
                {
                    projectExpenses = item;
                    break;
                }

            if (projectExpenses == null)
                throw new ValidationException(String.Format("未找到 {0} 可删除的报销记录!", Kernel.ProjectName));

            Database.Execute((DbTransaction dbTransaction) =>
            {
                projectExpenses.DeleteSelf(dbTransaction);
                Kernel.UpdateSelf(dbTransaction,
                    NameValue.Set<ProjectInfo>(p => p.TotalReimbursementAmount, p => p.TotalReimbursementAmount - projectExpenses.ReimbursementAmount));
            });
            ProjectExpensesList.Remove(projectExpenses);
        }

        #endregion

        #endregion
    }
}