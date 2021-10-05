using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.TPT.Business;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 工作人员Grain接口
    /// key: Worker（PH7_User.US_ID）
    /// </summary>
    public interface IWorkerGrain : Phenix.Actor.IGrain, IGrainWithIntegerKey
    {
        /// <summary>
        /// 获取某年某月项目工作量
        /// </summary>
        Task<IList<ProjectWorkload>> GetProjectWorkloads(short year, short month);

        /// <summary>
        /// 更新项目工作量(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutProjectWorkload(ProjectWorkload source);
    }
}
