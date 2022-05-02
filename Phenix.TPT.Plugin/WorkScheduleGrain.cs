using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Security;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Mapper.Expressions;
using Phenix.TPT.Business;
using Phenix.TPT.Business.Norm;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 工作档期Grain接口
    /// key: Manager（PH7_User.US_ID）
    /// </summary>
    public class WorkScheduleGrain : StreamGrainBase<string>, IWorkScheduleGrain
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

        private IDictionary<DateTime, WorkSchedule> _kernel;

        /// <summary>
        /// 年月-工作档期
        /// </summary>
        protected IDictionary<DateTime, WorkSchedule> Kernel
        {
            get
            {
                if (_kernel == null)
                    _kernel = WorkSchedule.FetchKeyValues(Database,
                        p => p.YearMonth,
                        p => p.Manager == Manager && p.Year >= DateTime.Now.Year - 1,
                        OrderBy.Ascending<Workday>(p => p.Year).Ascending(p => p.Month));
                return _kernel;
            }
        }

        #endregion

        #region 方法

        private DateTime GetDeadline()
        {
            DateTime now = DateTime.Now;
            return now.Day > 5 ? new DateTime(now.Year, now.Month, 5) : new DateTime(now.Year, now.Month, 5).AddMonths(-1);
        }

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

            return ClusterClient.GetSimpleMessageStreamProvider().GetStream<string>(StreamConfig.ProjectStreamId, receiver.ToString()).OnNextAsync(content, token);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        protected override Task OnReceiving(string content, StreamSequenceToken token)
        {
            DateTime deadline = GetDeadline();
            if (Kernel.TryGetValue(Standards.FormatYearMonth((short) deadline.Year, (short) deadline.Month), out WorkSchedule workSchedule))
                foreach (long receiver in workSchedule.Workers)
                    SendEventForRefreshProjectWorkloads(receiver, content, token);
            return Task.CompletedTask;
        }

        #endregion

        private WorkSchedule FetchWorkSchedule(short year, short month)
        {
            return Kernel.TryGetValue(Standards.FormatYearMonth(year, month), out WorkSchedule workSchedule)
                ? workSchedule
                : WorkSchedule.New(Database,
                    NameValue.Set<WorkSchedule>(p => p.Manager, Manager).
                        Set(p => p.Year, year).
                        Set(p => p.Month, month).
                        Set(p => p.Workers, new long[0]));
        }
        Task<WorkSchedule> IWorkScheduleGrain.FetchWorkSchedule(short year, short month)
        {
            return Task.FromResult(FetchWorkSchedule(year, month));
        }


        Task<IList<WorkSchedule>> IWorkScheduleGrain.FetchWorkSchedules(short pastMonths, short newMonths)
        {
            IList<WorkSchedule> result = new List<WorkSchedule>();
            DateTime deadline = GetDeadline();
            for (int i = -pastMonths; i < newMonths; i++)
            {
                short year = (short) (deadline.Month + i < 1 ? deadline.Year - 1 : deadline.Month + i > 12 ? deadline.Year + 1 : deadline.Year);
                short month = (short) (deadline.Month + i < 1 ? deadline.Month + i + 12 : deadline.Month + i > 12 ? deadline.Month + i - 12 : deadline.Month + i);
                result.Add(FetchWorkSchedule(year, month));
            }

            return Task.FromResult(result);
        }

        async Task IWorkScheduleGrain.PutWorkSchedule(WorkSchedule source)
        {
            bool unlimited = await User.Identity.IsInRole(ProjectRoles.经营管理);
            if (!(unlimited || new DateTime(source.Year, source.Month, 1).AddDays(DateTime.Now.Day - 1) < GetDeadline()))
                throw new ValidationException("不允许修改已归档的工作档期!");
            if (!(unlimited || User.Identity.Id == Manager))
                throw new SecurityException("管好自己的工作档期就行啦!");

            List<long> receivers = new List<long>();
            DateTime key = Standards.FormatYearMonth(source.Year, source.Month);
            if (Kernel.TryGetValue(key, out WorkSchedule workSchedule))
            {
                receivers.AddRange(workSchedule.Workers);
                Database.Execute(dbTransaction =>
                {
                    workSchedule.UpdateSelf(dbTransaction, source);
                    ResetWorkers(dbTransaction, workSchedule);
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
                    Kernel[key] = source;
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
                workSchedule.NewDetail(WorkScheduleWorker.Set(p => p.Worker, worker)).
                    InsertSelf(dbTransaction);
        }

        #endregion
    }
}
