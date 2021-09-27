﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;
using Phenix.TPT.Plugin.Filters;

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

        #region Stream

        private Task SendEventForRefreshProjectWorkloads(string receiver)
        {
            return ClusterClient.GetStreamProvider().GetStream<string>(StreamConfig.RefreshProjectWorkloadsStreamId,
                Standards.FormatCompoundKey(Kernel.OriginateTeams, receiver)).OnNextAsync(receiver);
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
                dynamic old = tag;
                if ((string) old.ProjectManager != Kernel.ProjectManager)
                {
                    SendEventForRefreshProjectWorkloads((string) old.ProjectManager);
                    SendEventForRefreshProjectWorkloads(Kernel.ProjectManager);
                }

                if ((string) old.DevelopManager != Kernel.DevelopManager)
                {
                    SendEventForRefreshProjectWorkloads((string) old.DevelopManager);
                    SendEventForRefreshProjectWorkloads(Kernel.DevelopManager);
                }
            }
            else
            {
                SendEventForRefreshProjectWorkloads(Kernel.ProjectManager);
                SendEventForRefreshProjectWorkloads(Kernel.DevelopManager);
            }
        }

        private async Task<bool> IsMyProject()
        {
            return await User.Identity.IsInRole(Roles.经营管理.ToString()) ||
                   User.Identity.UserName == Kernel.ProjectManager ||
                   User.Identity.UserName == Kernel.DevelopManager ||
                   User.Identity.UserName == Kernel.MaintenanceManager ||
                   User.Identity.UserName == Kernel.SalesManager;
        }

        async Task IProjectGrain.Close(DateTime closedDate)
        {
            if (!await IsMyProject())
                throw new ValidationException("非请毋删!");

            Kernel.UpdateSelf(NameValue.Set<ProjectInfo>(p => p.ClosedDate, closedDate));
        }

        #endregion

        #region 项目年度计划

        Task<IList<ProjectAnnualPlan>> IProjectGrain.GetAllProjectAnnualPlan()
        {
            return Task.FromResult(ProjectAnnualPlanList);
        }

        Task<ProjectAnnualPlan> IProjectGrain.GetProjectAnnualPlan(int year)
        {
            //DateTime today = DateTime.Today;
            //if (today.Year < year)
            //    throw new ValidationException("未来不可期~");
            
            foreach (ProjectAnnualPlan item in ProjectAnnualPlanList)
                if (item.Year == year)
                    return Task.FromResult(item);

            return Task.FromResult(Kernel.NewDetail(ProjectAnnualPlan.Set(p => p.Year, year).
                Set(p => p.AnnualReceivables, Kernel.ContAmount - Kernel.TotalReceivables)));
        }

        async Task IProjectGrain.PutProjectAnnualPlan(ProjectAnnualPlan source)
        {
            if (!await IsMyProject())
                throw new ValidationException("非请毋改!");

            //DateTime today = DateTime.Today;
            //if (today.Year < source.Year)
            //    throw new ValidationException("未来不可期~");

            foreach (ProjectAnnualPlan item in ProjectAnnualPlanList)
                if (item.Year == source.Year)
                {
                    if (source.AnnualReceivables > Kernel.ContAmount - Kernel.TotalReceivables + item.AnnualReceivables)
                        throw new ValidationException(String.Format("本应收款({0})已超过{1}可报数额({2})!",
                            source.AnnualReceivables, Kernel.ProjectName, Kernel.ContAmount - Kernel.TotalReceivables + item.AnnualReceivables));

                    Database.Execute((DbTransaction dbTransaction) =>
                    {
                        item.UpdateSelf(dbTransaction, source);
                        Kernel.UpdateSelf(dbTransaction,
                            NameValue.Set<ProjectInfo>(p => p.AnnualMilestone, source.AnnualMilestone).
                                Set(p => p.TotalReceivables, p => p.TotalReceivables - item.AnnualReceivables + source.AnnualReceivables));
                    });
                    return;
                }

            if (source.AnnualReceivables > Kernel.ContAmount - Kernel.TotalReceivables)
                throw new ValidationException(String.Format("本应收款({0})已超过{1}可报数额({2})!",
                    source.AnnualReceivables, Kernel.ProjectName, Kernel.ContAmount - Kernel.TotalReceivables));

            Database.Execute((DbTransaction dbTransaction) =>
            {
                source.InsertSelf(dbTransaction);
                Kernel.UpdateSelf(dbTransaction,
                    NameValue.Set<ProjectInfo>(p => p.AnnualMilestone, source.AnnualMilestone).
                        Set(p => p.TotalReceivables, p => p.TotalReceivables + source.AnnualReceivables));
            });
            ProjectAnnualPlanList.Add(source);
        }

        #endregion

        #region 项目月报

        Task<IList<ProjectMonthlyReport>> IProjectGrain.GetAllProjectMonthlyReport()
        {
            return Task.FromResult(ProjectMonthlyReportList);
        }

        Task<ProjectMonthlyReport> IProjectGrain.GetProjectMonthlyReport(int year, int month)
        {
            DateTime today = DateTime.Today;
            if (today.Year < year || today.Year == year && today.Month < month)
                throw new ValidationException("未来不可得~");

            DateTime ultimo = new DateTime(year, month, 1).AddMilliseconds(-1);
            ProjectMonthlyReport ultimoReport = null;
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
            if (!await IsMyProject())
                throw new ValidationException("非请毋改!");

            DateTime today = DateTime.Today;
            if (today.Year < source.Year || today.Year == source.Year && today.Month < source.Month)
                throw new ValidationException("未来不可得~");

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
            return Task.FromResult(ProjectProceedsList);
        }

        async Task IProjectGrain.PostProjectProceeds(ProjectProceeds source)
        {
            if (!await IsMyProject())
                throw new ValidationException("非请毋加!");

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
            if (!await IsMyProject())
                throw new ValidationException("非请毋删!");

            ProjectProceeds projectProceeds = null;
            foreach (ProjectProceeds item in ProjectProceedsList)
                if (item.Id == id)
                {
                    projectProceeds = item;
                    break;
                }

            if (projectProceeds == null)
                throw new ArgumentException(String.Format("未找到{0}可删除的开票记录!", Kernel.ProjectName), nameof(id));

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
            return Task.FromResult(ProjectExpensesList);
        }

        Task IProjectGrain.PostProjectExpenses(ProjectExpenses source)
        {
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
            if (!await IsMyProject())
                throw new ValidationException("非请毋删!");

            ProjectExpenses projectExpenses = null;
            foreach (ProjectExpenses item in ProjectExpensesList)
                if (item.Id == id)
                {
                    projectExpenses = item;
                    break;
                }

            if (projectExpenses == null)
                throw new ArgumentException(String.Format("未找到{0}可删除的报销记录!", Kernel.ProjectName), nameof(id));

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