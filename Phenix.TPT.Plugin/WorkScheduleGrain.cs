using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Security;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.TPT.Business;
using Phenix.TPT.Business.Norm;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 工作档期Grain接口
    /// key: Manager（PH7_User.US_ID）
    /// keyExtension: Standards.FormatYearMonth(year, month)
    /// </summary>
    public class WorkScheduleGrain : StreamEntityGrainBase<WorkSchedule, string>, IWorkScheduleGrain
    {
        #region 属性

        #region Stream

        /// <summary>
        /// StreamId
        /// </summary>
        protected override Guid StreamId
        {
            get { return StreamConfig.ProjectStreamId; }
        }

        /// <summary>
        /// (自己作为Observer)侦听的一组StreamNamespace
        /// </summary>
        protected override string[] ListenStreamNamespaces
        {
            get { return new string[] {Manager.ToString()}; }
        }

        #endregion

        /// <summary>
        /// 管理人员
        /// </summary>
        protected long Manager
        {
            get { return PrimaryKeyLong; }
        }

        private DateTime? _yearMonth;

        /// <summary>
        /// 年月
        /// </summary>
        protected DateTime YearMonth
        {
            get { return _yearMonth ??= DateTime.Parse(PrimaryKeyExtension); }
        }

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override WorkSchedule Kernel
        {
            get
            {
                return base.Kernel ??= WorkSchedule.FetchRoot(Database,
                    p => p.Manager == Manager && p.Year == YearMonth.Year && p.Month == YearMonth.Month);
            }
        }

        #endregion

        #region 方法

        #region Stream

        /// <summary>
        /// 发送消息刷新项目工作量
        /// </summary>
        /// <param name="receiver">侦听者</param>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        private Task SendEventForRefreshProjectWorkloads(long receiver, string content, StreamSequenceToken token = null)
        {
            if (receiver == Manager)
                return Task.CompletedTask;
            if (content == Manager.ToString())
                return Task.CompletedTask;

            return ClusterClient.GetStreamProvider().GetStream<string>(StreamConfig.ProjectStreamId, receiver.ToString()).OnNextAsync(content, token);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        protected override Task OnReceiving(string content, StreamSequenceToken token)
        {
            foreach (long receiver in Kernel.Workers)
                SendEventForRefreshProjectWorkloads(receiver, content, token);
            return Task.CompletedTask;
        }

        #endregion

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="autoNew">不存在则新增</param>
        protected override async Task<WorkSchedule> FetchKernel(bool autoNew = false)
        {
            if (!(await User.Identity.IsInRole(ProjectRoles.经营管理, ProjectRoles.项目管理)))
                throw new SecurityException("仅允许管理层调配员工、项目负责人组织团队!");

            return Kernel ?? (autoNew
                ? WorkSchedule.New(Database,
                    NameValue.Set<WorkSchedule>(p => p.Manager, Manager).
                        Set(p => p.Year, YearMonth.Year).
                        Set(p => p.Month, YearMonth.Month).
                        Set(p => p.Workers, new List<long>()))
                : null);
        }

        private DateTime GetDeadline()
        {
            DateTime now = DateTime.Now;
            return now.Day > 5 ? new DateTime(now.Year, now.Month, 5) : new DateTime(now.Year, now.Month, 5).AddMonths(-1);
        }

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="source">数据源</param>
        protected override async Task PutKernel(WorkSchedule source)
        {
            bool unlimited = await User.Identity.IsInRole(ProjectRoles.经营管理);
            if (!(unlimited || new DateTime(source.Year, source.Month, 1).AddDays(DateTime.Now.Day - 1) < GetDeadline()))
                throw new ValidationException("不允许修改已归档的工作量!");
            if (!(unlimited || User.Identity.Id == Manager))
                throw new SecurityException("管好自己的工作档期就行啦!");

            List<long> receivers = new List<long>();
            if (Kernel != null)
            {
                if (Kernel.Workers != null)
                    receivers.AddRange(Kernel.Workers);
                Database.Execute(dbTransaction =>
                {
                    Kernel.UpdateSelf(dbTransaction, source);
                    ResetWorkers(dbTransaction, Kernel);
                });
                foreach (long item in source.Workers)
                    if (!receivers.Contains(item))
                        receivers.Add(item);
            }
            else
            {
                Database.Execute(dbTransaction =>
                {
                    source.InsertSelf(dbTransaction);
                    ResetWorkers(dbTransaction, source);
                    Kernel = source;
                });
                receivers.AddRange(source.Workers);
            }

            //播报
            foreach (long item in receivers)
                await SendEventForRefreshProjectWorkloads(item, Manager.ToString());
        }

        private void ResetWorkers(DbTransaction dbTransaction, WorkSchedule workSchedule)
        {
            workSchedule.DeleteDetails<WorkScheduleWorker>(dbTransaction);
            foreach (long worker in workSchedule.Workers)
                workSchedule.NewDetail(WorkScheduleWorker.Set(p => p.Worker, worker)).InsertSelf(dbTransaction);
        }

        #endregion
    }
}
