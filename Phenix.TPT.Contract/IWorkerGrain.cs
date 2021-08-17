using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.TPT.Business;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 工作人员Grain接口
    /// key: RootTeamsId
    /// keyExtension: Worker
    /// </summary>
    public interface IWorkerGrain : Phenix.Actor.IGrain, IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 获取某年某月项目工作量
        /// </summary>
        Task<IList<ProjectWorkload>> GetProjectWorkloads(short year, short month);

        /// <summary>
        /// 重置项目工作量
        /// </summary>
        Task ResetProjectWorkloads();
    }
}
