using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.InspectionStation.Plugin.Business;
using Orleans;

namespace Demo.InspectionStation.Plugin.Actor
{
    /// <summary>
    /// 中控Grain接口
    /// </summary>
    public interface ICenterGrain : IGrainWithStringKey
    {
        /// <summary>
        /// 获取监控的作业点
        /// </summary>
        Task<IDictionary<string, IsOperationPoint>> FetchOperationPoint();

        /// <summary>
        /// 监控指定的作业点
        /// </summary>
        /// <param name="operationPoints">作业点</param>
        Task Listen(IList<string> operationPoints);

    }
}
