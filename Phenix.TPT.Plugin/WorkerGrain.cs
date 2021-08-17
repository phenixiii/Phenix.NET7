using System.Collections.Generic;
using Phenix.Actor;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 工作人员Grain
    /// key: RootTeamsId
    /// keyExtension: Worker
    /// </summary>
    public class WorkerGrain : GrainBase, IWorkerGrain
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
        /// 工作人员
        /// </summary>
        protected string Worker
        {
            get { return PrimaryKeyExtension; }
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

        #endregion

        #region 方法

        #endregion
    }
}
