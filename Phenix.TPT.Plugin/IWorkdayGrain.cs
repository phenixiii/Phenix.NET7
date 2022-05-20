using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.TPT.Plugin.Business;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 工作日Grain接口
    /// key: Year
    /// </summary>
    public interface IWorkdayGrain : Phenix.Actor.IGrain, IGrainWithIntegerKey
    {
        /// <summary>
        /// 获取本年度某月工作日(如不存在则返回初始对象)
        /// </summary>
        Task<Workday> GetWorkday(short month);

        /// <summary>
        /// 获取本年度工作日(如不存在则返回初始对象)
        /// </summary>
        Task<IList<Workday>> GetWorkdays();
        
        /// <summary>
        /// 更新工作日(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutWorkday(Workday source);
    }
}
