using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.TPT.Business;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 工作档期Grain接口
    /// key: Manager（PH7_User.US_ID）
    /// </summary>
    public interface IWorkScheduleGrain : Phenix.Actor.IGrain, IGrainWithIntegerKey
    {
        /// <summary>
        /// 是否有档期
        /// </summary>
        Task<bool> HaveWorkSchedule(long worker, short year, short month);

        /// <summary>
        /// 获取近期工作档期
        /// </summary>
        Task<IList<WorkSchedule>> GetImmediateWorkSchedules();

        /// <summary>
        /// 更新工作档期(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutWorkSchedule(WorkSchedule source);
    }
}
