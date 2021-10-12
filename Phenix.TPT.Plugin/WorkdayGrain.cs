using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Data.Expressions;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 工作日Grain
    /// key: Year
    /// </summary>
    public class WorkdayGrain : GrainBase, IWorkdayGrain
    {
        #region 属性

        /// <summary>
        /// 年
        /// </summary>
        protected long Year
        {
            get { return PrimaryKeyLong; }
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
                        p => p.Year == Year, 
                        OrderBy.Ascending<Workday>(p => p.Month));
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
                        p => p.Year == Year + 1,
                        OrderBy.Ascending<Workday>(p => p.Month));
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

        Task<Workday> IWorkdayGrain.GetCurrentYearWorkday(short month)
        {
            if (month < 1 || month > 12)
                throw new ValidationException(String.Format("咱这可没{0}月份唉!", month));

            return Task.FromResult(CurrentYearWorkdays[month]);
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
                throw new ValidationException(String.Format("咱这可没{0}月份唉!", source.Month));
            if (source.Days < 1 || source.Days > 31)
                throw new ValidationException("躺平和加班一样糟糕，最好对标法定工作日哦!");

            Workday workday = source.Year == Year
                ? CurrentYearWorkdays[source.Month]
                : source.Year == Year + 1
                    ? NextYearWorkdays[source.Month]
                    : throw new ValidationException(String.Format("提交的工作日仅限于{0}、{1}年的!", Year, Year + 1));

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
