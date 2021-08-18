using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 工作日Grain
    /// key: RootTeamsId
    /// keyExtension: Year
    /// </summary>
    public class WorkdayGrain : GrainBase, IWorkdayGrain
    {
        #region 属性

        /// <summary>
        /// 所属公司ID
        /// </summary>
        protected long RootTeamsId
        {
            get { return PrimaryKeyLong; }
        }

        private short? _year;

        /// <summary>
        /// 年
        /// </summary>
        protected short Year
        {
            get
            {
                if (!_year.HasValue)
                    _year = Int16.Parse(PrimaryKeyExtension);
                return _year.Value;
            }
        }

        private IDictionary<short, Workday> _currentYearWorkdays;

        /// <summary>
        /// 本年度工作日
        /// </summary>
        protected IDictionary<short, Workday> CurrentYearWorkdays
        {
            get
            {
                if (_currentYearWorkdays == null)
                {
                    IDictionary<short, Workday> currentYearWorkdays = Workday.FetchKeyValues(Database,
                        p => p.Month,
                        p => p.OriginateTeams == RootTeamsId && p.Year == Year);
                    if (currentYearWorkdays.Count == 0)
                        for (short i = 1; i <= 12; i++)
                            currentYearWorkdays.Add(i, Workday.New(Database,
                                Workday.Set(p => p.Year, Year).
                                    Set(p => p.Month, i)));
                    _currentYearWorkdays = currentYearWorkdays;
                }

                return _currentYearWorkdays;
            }
        }

        private IDictionary<short, Workday> _nextYearWorkdays;

        /// <summary>
        /// 次年度工作日
        /// </summary>
        protected IDictionary<short, Workday> NextYearWorkdays
        {
            get
            {
                if (_nextYearWorkdays == null)
                {
                    IDictionary<short, Workday> nextYearWorkdays = Workday.FetchKeyValues(Database,
                        p => p.Month,
                        p => p.OriginateTeams == RootTeamsId && p.Year == Year + 1);
                    if (nextYearWorkdays.Count == 0)
                        for (short i = 1; i <= 12; i++)
                            nextYearWorkdays.Add(i, Workday.New(Database,
                                Workday.Set(p => p.Year, Year + 1).
                                    Set(p => p.Month, i)));
                    _nextYearWorkdays = nextYearWorkdays;
                }

                return _nextYearWorkdays;
            }
        }

        #endregion

        #region 方法

        Task<short> IWorkdayGrain.GetWorkdays(short year, short month)
        {
            if (year < Year || year > Year + 1)
                throw new ValidationException(String.Format("查询的工作日仅限于{0}年和{1}年的!", year, year + 1));
            if (month < 1 || month > 12)
                throw new ValidationException("查询的工作日月份仅限于1-12之间!");

            Workday workday = CurrentYearWorkdays[month];
            if (workday.Days == 0)
                throw new InvalidOperationException(String.Format("{0}年{1}月的工作日还未设置哦!", Year, month));

            return Task.FromResult(workday.Days);
        }

        Task<IList<Workday>> IWorkdayGrain.GetCurrentYearWorkdays()
        {
            IList<Workday> result = new List<Workday>(CurrentYearWorkdays.Values);
            return Task.FromResult(result);
        }

        Task<IList<Workday>> IWorkdayGrain.GetNextYearWorkdays()
        {
            IList<Workday> result = new List<Workday>(NextYearWorkdays.Values);
            return Task.FromResult(result);
        }

        Task IWorkdayGrain.PutWorkday(Workday source)
        {
            if (source.Month < 1 || source.Month > 12)
                throw new ValidationException("提交的工作日月份仅限于1-12之间!");
            if (source.Days < 15 || source.Days > 25)
                throw new ValidationException("提交的工作日天数仅限于15-25之间!");

            Workday workday = source.Year == Year
                ? CurrentYearWorkdays[source.Month]
                : source.Year == Year + 1
                    ? NextYearWorkdays[source.Month]
                    : throw new ValidationException(String.Format("提交的工作日仅限于{0}年和{1}年的!", Year, Year + 1));

            if (workday.Days == 0)
            {
                workday.Apply(Workday.Set(p => p.Days, source.Days));
                workday.InsertSelf();
            }
            else
                workday.UpdateSelf(source);

            return Task.CompletedTask;
        }

        #endregion
    }
}
