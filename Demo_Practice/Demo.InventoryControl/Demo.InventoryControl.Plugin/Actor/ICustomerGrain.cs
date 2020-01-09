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
        Task LoadLocation(string brand, string cardNumber, string transportNumber, int weight,
            string locationArea, string locationAlley, string locationOrdinal);

        /// <summary>
        /// 卸下货架
        /// </summary>
        Task UnloadLocation(long pickMarks);
    }
}
