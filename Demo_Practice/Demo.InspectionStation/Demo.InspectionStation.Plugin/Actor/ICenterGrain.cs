using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Demo.InspectionStation.Plugin.Actor
{
    /// <summary>
    /// 中控Grain接口
    /// </summary>
    public interface ICenterGrain : IGrainWithStringKey
    {
        /// <summary>
        /// 监控指定的作业点
        /// </summary>
        /// <param name="operationPoints">作业点</param>
        Task Monitoring(IList<string> operationPoints);

    }
}
