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

        private IDictionary<short, Workday> _kernel;

        /// <summary>
        /// 月-工作日
        /// </summary>
        protected IDictionary<short, Workday> Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    IDictionary<short, Workday> result = Workday.FetchKeyValues(Database,
                        p => p.Month,
                        p => p.Year == Year, 
                        OrderBy.Ascending<Workday>(p => p.Month));
                    if (result.Count == 0)
                        for (short i = 1; i <= 12; i++)
                            result.Add(i, Workday.New(Database,
                                Workday.Set(p => p.Year, Year).
                                    Set(p => p.Month, i)));
                    _kernel = result;
                }

                return _kernel;
            }
        }

        #endregion

        #region 方法

        Task<Workday> IWorkdayGrain.GetWorkday(short month)
        {
            if (month < 1 || month > 12)
                throw new ValidationException(String.Format("咱这可没{0}月份唉!", month));

            return Task.FromResult(Kernel[month]);
        }

        Task<IList<Workday>> IWorkdayGrain.GetWorkdays()
        {
            IList<Workday> result = new List<Workday>(Kernel.Values);
            return Task.FromResult(result);
        }

        Task IWorkdayGrain.PutWorkday(Workday source)
        {
            if (source.Month < 1 || source.Month > 12)
                throw new ValidationException(String.Format("咱这可没{0}月份唉!", source.Month));
            if (source.Days < 1 || source.Days > 31)
                throw new ValidationException("躺平和加班一样糟糕，最好对标法定工作日哦!");

            Workday workday = Kernel[source.Month];
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
