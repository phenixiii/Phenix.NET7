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
        /// 取LIFO序号
        /// </summary>
        Task<long> GetStackOrdinal();

        /// <summary>
        /// 取卸下价值(0-9)
        /// </summary>
        /// <param name="brand">品牌(null代表忽略本筛选条件)</param>
        /// <param name="cardNumber">卡号(null代表忽略本筛选条件)</param>
        /// <param name="transportNumber">车皮/箱号(null代表忽略本筛选条件)</param>
        Task<int> GetUnloadValue(string brand, string cardNumber, string transportNumber);

        /// <summary>
        /// 刷新状态
        /// </summary>
        Task Refresh();
    }
}
