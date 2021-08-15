using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.TPT.Business;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 工作档期Grain接口
    /// key: RootTeamsId
    /// keyExtension: Manager
    /// </summary>
    public interface IWorkScheduleGrain : Phenix.Actor.IGrain, IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 某人某年某月是否有档期
        /// </summary>
        Task<bool> HaveWorkSchedule(string worker, short year, short month);

        /// <summary>
        /// 获取近期工作档期
        /// </summary>
        Task<IDictionary<string, WorkSchedule>> GetImmediateWorkSchedules();

        /// <summary>
        /// 更新工作档期(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutWorkSchedule(WorkSchedule source);
    }
}
