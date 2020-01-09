using System.Threading.Tasks;
using Orleans;

namespace Demo.InventoryControl.Plugin.Actor
{
    /// <summary>
    /// 货架Grain接口
    /// </summary>
    public interface ILocationGrain : IGrainWithStringKey
    {
        /// <summary>
        /// 刷新状态
        /// </summary>
        Task Refresh();
    }
}
