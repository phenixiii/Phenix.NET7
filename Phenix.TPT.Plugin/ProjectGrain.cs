﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.TPT.Business;
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
                return _projectAnnualPlanList ?? (_projectAnnualPlanList =
                    Kernel.FetchDetails(ProjectAnnualPlan.Descending(p => p.Year)));
            }
        }

        private decimal? _totalReceivables;

        /// <summary>
        /// 应收总额
        /// </summary>
        public decimal TotalReceivables
        {
            get
            {
                if (!_totalReceivables.HasValue)
                {
                    decimal totalReceivables = 0;
                    foreach (ProjectAnnualPlan item in ProjectAnnualPlanList)
                        totalReceivables = totalReceivables + item.AnnualReceivables;
                    _totalReceivables = totalReceivables;
                }

                return _totalReceivables.Value;
            }
            private set { _totalReceivables = value; }
        }

        private IList<ProjectMonthlyReport> _projectMonthlyReportList;

        /// <summary>
        /// 项目月报清单
        /// </summary>
        protected IList<ProjectMonthlyReport> ProjectMonthlyReportList
        {
            get
            {
                return _projectMonthlyReportList ?? (_projectMonthlyReportList =
                    Kernel.FetchDetails(ProjectMonthlyReport.Descending(p => p.Year).Descending(p => p.Month)));
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
                return _projectProceedsList ?? (_projectProceedsList =
                    Kernel.FetchDetails(ProjectProceeds.Descending(p => p.InvoiceDate)));
            }
        }

        private decimal? _totalInvoiceAmount;

        /// <summary>
        /// 开票总额
        /// </summary>
        public decimal TotalInvoiceAmount
        {
            get
            {
                if (!_totalInvoiceAmount.HasValue)
                {
                    decimal totalInvoiceAmount = 0;
                    foreach (ProjectProceeds item in ProjectProceedsList)
                        totalInvoiceAmount = totalInvoiceAmount + item.InvoiceAmount;
                    _totalInvoiceAmount = totalInvoiceAmount;
                }

                return _totalInvoiceAmount.Value;
            }
            private set { _totalInvoiceAmount = value; }
        }

        private IList<ProjectExpenses> _projectExpensesList;

        /// <summary>
        /// 项目开支清单
        /// </summary>
        protected IList<ProjectExpenses> ProjectExpensesList
        {
            get
            {
                return _projectExpensesList ?? (_projectExpensesList =
                    Kernel.FetchDetails(ProjectExpenses.Descending(p => p.ReimbursementDate)));
            }
        }

        private decimal? _totalReimbursementAmount;

        /// <summary>
        /// 报销总额
        /// </summary>
        public decimal TotalReimbursementAmount
        {
            get
            {
                if (!_totalReimbursementAmount.HasValue)
                {
                    decimal totalReimbursementAmount = 0;
                    foreach (ProjectExpenses item in ProjectExpensesList)
                        totalReimbursementAmount = totalReimbursementAmount + item.ReimbursementAmount;
                    _totalReimbursementAmount = totalReimbursementAmount;
                }

                return _totalReimbursementAmount.Value;
            }
            private set { _totalReimbursementAmount = value; }
        }

        private IList<ProjectWorkload> _projectWorkloadList;

        /// <summary>
        /// 项目工作量清单
        /// </summary>
        protected IList<ProjectWorkload> ProjectWorkloadList
        {
            get
            {
                return _projectWorkloadList ?? (_projectWorkloadList =
                    Kernel.FetchDetails(ProjectWorkload.Descending(p => p.Year).Descending(p => p.Month)));
            }
        }

        #endregion

        #region 方法

        #region 项目年度计划

        Task<IList<ProjectAnnualPlan>> IProjectGrain.GetAllProjectAnnualPlan()
        {
            return Task.FromResult(ProjectAnnualPlanList);
        }

        Task<ProjectAnnualPlan> IProjectGrain.GetNowProjectAnnualPlan()
        {
            DateTime today = DateTime.Today;
            foreach (ProjectAnnualPlan item in ProjectAnnualPlanList)
                if (item.Year == today.Year)
                    return Task.FromResult(item);
            return Task.FromResult(Kernel.NewDetail(ProjectAnnualPlan.Set(p => p.Year, today.Year)));
        }

        Task IProjectGrain.PutProjectAnnualPlan(ProjectAnnualPlan source)
        {
            decimal totalReceivables = TotalReceivables;
            foreach (ProjectAnnualPlan item in ProjectAnnualPlanList)
                if (item.Year == source.Year)
                {
                    decimal subTotalReceivables = totalReceivables - item.AnnualReceivables;
                    if (source.AnnualReceivables > Kernel.ContAmount - subTotalReceivables)
                        throw new ValidationException(String.Format("本应收款({0})已超过可报数额({1})!", source.AnnualReceivables, Kernel.ContAmount - subTotalReceivables));

                    item.UpdateSelf(source);
                    TotalReceivables = subTotalReceivables + source.AnnualReceivables;
                    return Task.CompletedTask;
                }

            if (source.AnnualReceivables > Kernel.ContAmount - totalReceivables)
                throw new ValidationException(String.Format("本应收款({0})已超过可报数额({1})!", source.AnnualReceivables, Kernel.ContAmount - totalReceivables));

            source.InsertSelf();
            ProjectAnnualPlanList.Add(source);
            TotalReceivables = totalReceivables + source.AnnualReceivables;
            return Task.CompletedTask;
        }

        Task<decimal> IProjectGrain.GetTotalReceivables()
        {
            return Task.FromResult(TotalReceivables);
        }

        #endregion

        #region 项目月报

        Task<IList<ProjectMonthlyReport>> IProjectGrain.GetAllProjectMonthlyReport()
        {
            return Task.FromResult(ProjectMonthlyReportList);
        }

        Task<ProjectMonthlyReport> IProjectGrain.GetNowProjectMonthlyReport()
        {
            DateTime today = DateTime.Today;
            DateTime ultimo = today.AddMonths(-1);
            ProjectMonthlyReport ultimoReport = null;
            foreach (ProjectMonthlyReport item in ProjectMonthlyReportList)
                if (item.Year == today.Year && item.Month == today.Month)
                    return Task.FromResult(item);
                else if (item.Year == ultimo.Year && item.Month == ultimo.Month)
                    ultimoReport = item;
            return Task.FromResult(Kernel.NewDetail(ProjectMonthlyReport.
                Set(p => p.Year, today.Year).
                Set(p => p.Month, today.Month).
                Set( p => p.MonthlyPlan, ultimoReport != null ? ultimoReport.NextMonthlyPlan : "*")));
        }

        Task IProjectGrain.PutProjectMonthlyReport(ProjectMonthlyReport source)
        {
            foreach (ProjectMonthlyReport item in ProjectMonthlyReportList)
                if (item.Year == source.Year && item.Month == source.Month)
                {
                    item.UpdateSelf(source);
                    return Task.CompletedTask;
                }

            source.InsertSelf();
            ProjectMonthlyReportList.Add(source);
            return Task.CompletedTask;
        }

        #endregion

        #region 项目收款

        Task<IList<ProjectProceeds>> IProjectGrain.GetAllProjectProceeds()
        {
            return Task.FromResult(ProjectProceedsList);
        }
        
        Task IProjectGrain.PostProjectProceeds(ProjectProceeds source)
        {
            decimal totalInvoiceAmount = TotalInvoiceAmount;
            if (source.InvoiceAmount > Kernel.ContAmount - totalInvoiceAmount)
                throw new ValidationException(String.Format("本开票金额({0})已超过可开数额({1})!", source.InvoiceAmount, Kernel.ContAmount - totalInvoiceAmount));

            source.InsertSelf();
            ProjectProceedsList.Add(source);
            TotalInvoiceAmount = totalInvoiceAmount + source.InvoiceAmount;
            return Task.CompletedTask;
        }

        Task IProjectGrain.DeleteProjectProceeds(long id)
        {
            ProjectProceeds projectProceeds = null;
            foreach (ProjectProceeds item in ProjectProceedsList)
                if (item.Id == id)
                {
                    projectProceeds = item;
                    break;
                }

            if (projectProceeds == null)
                throw new ValidationException("未找到可删除的开票记录!");

            projectProceeds.DeleteSelf();
            ProjectProceedsList.Remove(projectProceeds);
            TotalInvoiceAmount = TotalInvoiceAmount - projectProceeds.InvoiceAmount;
            return Task.CompletedTask;
        }

        Task<decimal> IProjectGrain.GetTotalInvoiceAmount()
        {
            return Task.FromResult(TotalInvoiceAmount);
        }

        #endregion

        #region 项目开支

        Task<IList<ProjectExpenses>> IProjectGrain.GetAllProjectExpenses()
        {
            return Task.FromResult(ProjectExpensesList);
        }

        Task IProjectGrain.PostProjectExpenses(ProjectExpenses source)
        {
            source.InsertSelf();
            ProjectExpensesList.Add(source);
            TotalReimbursementAmount = TotalReimbursementAmount + source.ReimbursementAmount;
            return Task.CompletedTask;
        }

        Task IProjectGrain.DeleteProjectExpenses(long id)
        {
            ProjectExpenses projectExpenses = null;
            foreach (ProjectExpenses item in ProjectExpensesList)
                if (item.Id == id)
                {
                    projectExpenses = item;
                    break;
                }

            if (projectExpenses == null)
                throw new ValidationException("未找到可删除的报销记录!");

            projectExpenses.DeleteSelf();
            ProjectExpensesList.Remove(projectExpenses);
            TotalReimbursementAmount = TotalReimbursementAmount - projectExpenses.ReimbursementAmount;
            return Task.CompletedTask;
        }

        Task<decimal> IProjectGrain.GetTotalReimbursementAmount()
        {
            return Task.FromResult(TotalReimbursementAmount);
        }

        #endregion

        #region 项目工作量

        #endregion

        #endregion
    }
}