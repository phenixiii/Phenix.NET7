using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.TPT.Business;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 工作日Grain接口
    /// key: RootTeamsId
    /// keyExtension: Year
    /// </summary>
    public interface IWorkdayGrain : Phenix.Actor.IGrain, IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 获取某年某月工作日
        /// </summary>
        Task<short> GetWorkdays(short year, short month);

        /// <summary>
        /// 获取本年度工作日
        /// </summary>
        Task<IList<Workday>> GetCurrentYearWorkdays();

        /// <summary>
        /// 获取次年度工作日
        /// </summary>
        Task<IList<Workday>> GetNextYearWorkdays();

        /// <summary>
        /// 更新工作日(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutWorkday(Workday source);
    }
}
