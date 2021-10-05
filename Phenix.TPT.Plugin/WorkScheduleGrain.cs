using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.TPT.Business;
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
            get { return StreamConfig.RefreshProjectWorkloadsStreamId; }
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

        private IDictionary<DateTime, WorkSchedule> _immediateWorkSchedules;

        /// <summary>
        /// 近期工作档期
        /// </summary>
        protected IDictionary<DateTime, WorkSchedule> ImmediateWorkSchedules
        {
            get
            {
                if (_immediateWorkSchedules == null)
                {
                    int year = DateTime.Today.Year;
                    int month = DateTime.Today.Month;
                    _immediateWorkSchedules = WorkSchedule.FetchKeyValues(Database,
                        p => p.YearMonth,
                        p => p.Manager == Manager &&
                             (p.Year == year && p.Month >= month || p.Year > year));
                }

                return _immediateWorkSchedules;
            }
        }

        #endregion

        #region 方法

        #region Stream

        private Task SendEventForRefreshProjectWorkloads(long receiver, string content)
        {
            if (receiver == Manager)
                return Task.CompletedTask;
            if (content == Manager.ToString())
                return Task.CompletedTask;

            return ClusterClient.GetStreamProvider().GetStream<string>(StreamConfig.RefreshProjectWorkloadsStreamId, receiver.ToString()).OnNextAsync(content);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        protected override Task OnReceiving(string content, StreamSequenceToken token)
        {
            List<long> workers = new List<long>();
            foreach (KeyValuePair<DateTime, WorkSchedule> kvp in ImmediateWorkSchedules)
            foreach (long worker in kvp.Value.Workers)
                if (!workers.Contains(worker))
                    workers.Add(worker);
            foreach (long worker in workers)
                SendEventForRefreshProjectWorkloads(worker, content);
            return Task.CompletedTask;
        }

        #endregion

        Task<bool> IWorkScheduleGrain.HaveWorkSchedule(long worker, short year, short month)
        {
            if (worker == Manager)
                return Task.FromResult(true);
            if (ImmediateWorkSchedules.TryGetValue(Standards.FormatYearMonth(year, month), out WorkSchedule workSchedule))
                return Task.FromResult(workSchedule.Workers.Contains(worker));

            throw new ValidationException(String.Format("查询不到{0}年{1}月{2}的工作档期记录!", year, month, worker));
        }

        Task<IList<WorkSchedule>> IWorkScheduleGrain.GetImmediateWorkSchedules()
        {
            IList<WorkSchedule> result = new List<WorkSchedule>(ImmediateWorkSchedules.Values);
            return Task.FromResult(result);
        }

        Task IWorkScheduleGrain.PutWorkSchedule(WorkSchedule source)
        {
            int year = DateTime.Today.Year;
            if (source.Year < year || source.Year > year + 1)
                throw new ValidationException(String.Format("提交的工作档期仅限于{0}年和{1}年的!", year, year + 1));
            if (source.Month < 1 || source.Month > 12)
                throw new ValidationException("提交的工作档期月份仅限于1-12之间!");
            if (source.Manager != Manager)
                throw new ValidationException(String.Format("提交的工作档期管理人员应该是{0}!", Manager));

            List<long> workers = new List<long>();
            DateTime yearMonth = Standards.FormatYearMonth(source.Year, source.Month);
            if (ImmediateWorkSchedules.TryGetValue(yearMonth, out WorkSchedule workSchedule))
                Database.Execute((DbTransaction dbTransaction) =>
                {
                    workers.AddRange(workSchedule.Workers);
                    workSchedule.UpdateSelf(dbTransaction, source);
                    ResetWorkers(dbTransaction, workSchedule);
                });
            else
                Database.Execute((DbTransaction dbTransaction) =>
                {
                    source.InsertSelf(dbTransaction);
                    ResetWorkers(dbTransaction, source);
                    ImmediateWorkSchedules[yearMonth] = source;
                });

            foreach (long worker in source.Workers)
                if (!workers.Contains(worker))
                    workers.Add(worker);
            foreach (long worker in workers)
                SendEventForRefreshProjectWorkloads(worker, Manager.ToString());
            return Task.CompletedTask;
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
