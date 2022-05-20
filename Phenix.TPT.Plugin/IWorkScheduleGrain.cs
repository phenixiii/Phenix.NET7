using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.TPT.Plugin.Business;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 工作档期Grain接口
    /// key: Manager（PH7_User.US_ID）
    /// </summary>
    public interface IWorkScheduleGrain : Phenix.Actor.IGrain, IGrainWithIntegerKey
    {
        /// <summary>
        /// 获取工作档期(如不存在则返回初始对象)
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        Task<WorkSchedule> FetchWorkSchedule(short year, short month);

        /// <summary>
        /// 获取工作档期(如不存在则返回初始对象)
        /// </summary>
        /// <param name="pastMonths">往期月份数</param>
        /// <param name="newMonths">新生月份数</param>
        Task<IList<WorkSchedule>> FetchWorkSchedules(short pastMonths, short newMonths);

        /// <summary>
        /// 更新工作档期(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutWorkSchedule(WorkSchedule source);
    }
}
