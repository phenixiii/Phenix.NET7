using System.Threading.Tasks;
using Orleans;

namespace Demo.InventoryControl.Plugin.Actor
{
    /// <summary>
    /// 货主Grain接口
    /// </summary>
    public interface ICustomerGrain : IGrainWithStringKey
    {
        /// <summary>
        /// 装上货架
        /// </summary>
        /// <param name="brand">品牌</param>
        /// <param name="cardNumber">卡号</param>
        /// <param name="transportNumber">车皮/箱号</param>
        /// <param name="weight">重量</param>
        /// <param name="locationArea">货架库区</param>
        /// <param name="locationAlley">货架巷道</param>
        /// <param name="locationOrdinal">货架序号</param>
        Task LoadLocation(string brand, string cardNumber, string transportNumber, int weight, string locationArea, string locationAlley, string locationOrdinal);

        /// <summary>
        /// 挑选货物
        /// </summary>
        /// <param name="pickMarks">挑中标记号码</param>
        /// <param name="brand">品牌(null代表忽略本筛选条件)</param>
        /// <param name="cardNumber">卡号(null代表忽略本筛选条件)</param>
        /// <param name="transportNumber">车皮/箱号(null代表忽略本筛选条件)</param>
        /// <param name="minTotalWeight">最小总重</param>
        /// <param name="maxTotalWeight">最大总重</param>
        Task<bool> PickInventory(long pickMarks, string brand, string cardNumber, string transportNumber, int minTotalWeight, int maxTotalWeight);

        /// <summary>
        /// 卸下货架
        /// </summary>
        /// <param name="pickMarks">挑中标记号码</param>
        Task UnloadLocation(long pickMarks);
    }
}
