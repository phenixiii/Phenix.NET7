using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 工作档期Grain接口
    /// key: RootTeamsId
    /// keyExtension: Manager
    /// </summary>
    public class WorkScheduleGrain : GrainBase, IWorkScheduleGrain
    {
        #region 属性

        /// <summary>
        /// 所属公司ID
        /// </summary>
        protected long RootTeamsId
        {
            get { return PrimaryKeyLong; }
        }

        /// <summary>
        /// 管理人员
        /// </summary>
        protected string Manager
        {
            get { return PrimaryKeyExtension; }
        }

        private IDictionary<string, WorkSchedule> _immediateWorkSchedules;

        /// <summary>
        /// 近期工作档期
        /// </summary>
        protected IDictionary<string, WorkSchedule> ImmediateWorkSchedules
        {
            get
            {
                if (_immediateWorkSchedules == null)
                {
                    int year = DateTime.Today.Year;
                    int month = DateTime.Today.Month;
                    _immediateWorkSchedules = WorkSchedule.FetchKeyValues(Database,
                        p => p.YearMonth,
                        p => p.OriginateTeams == RootTeamsId && p.Manager == Manager &&
                             (p.Year == year && p.Month >= month || p.Year > year));
                }

                return _immediateWorkSchedules;
            }
        }

        #endregion

        #region 方法

        Task<bool> IWorkScheduleGrain.HaveWorkSchedule(string worker, short year, short month)
        {
            return Task.FromResult(ImmediateWorkSchedules.TryGetValue(WorkSchedule.FormatYearMonth(year, month), out WorkSchedule workSchedule) && workSchedule.Workers.Contains(worker));
        }

        Task<IDictionary<string, WorkSchedule>> IWorkScheduleGrain.GetImmediateWorkSchedules()
        {
            return Task.FromResult(ImmediateWorkSchedules);
        }
        
        Task IWorkScheduleGrain.PutWorkSchedule(WorkSchedule source)
        {
            int year = DateTime.Today.Year;
            if (source.Year < year || source.Year > year + 1)
                throw new ArgumentOutOfRangeException(nameof(source), String.Format("提交的工作档期仅限于{0}年和{1}年的!", year, year + 1));
            if (source.Month <= 1 || source.Month >= 12)
                throw new ArgumentOutOfRangeException(nameof(source), "提交的工作档期月份仅限于1-12之间!");
            if (source.Manager != Manager)
                throw new ArgumentException(String.Format("提交的工作档期管理人员应该是{0}!", Manager), nameof(source));

            string yearMonth = WorkSchedule.FormatYearMonth(source.Year, source.Month);
            if (ImmediateWorkSchedules.TryGetValue(yearMonth, out WorkSchedule workSchedule))
                workSchedule.UpdateSelf(source);
            else
            {
                source.InsertSelf();
                ImmediateWorkSchedules[yearMonth] = source;
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
