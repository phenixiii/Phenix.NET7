using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.TPT.Plugin.Business;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目工作量Grain接口
    /// key: Worker（PH7_User.US_ID）
    /// keyExtension: Standards.FormatYearMonth(year, month)
    /// </summary>
    public interface IProjectWorkloadGrain : Phenix.Actor.IGrain, IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 获取项目工作量(如不存在则返回初始对象)
        /// </summary>
        Task<IList<ProjectWorkload>> GetProjectWorkloads();

        /// <summary>
        /// 更新项目工作量(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PutProjectWorkload(ProjectWorkload source);
    }
}
