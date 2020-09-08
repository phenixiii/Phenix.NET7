using System.Threading.Tasks;
using Demo.InventoryControl.Plugin.Business;
using Phenix.Actor;

namespace Demo.InventoryControl.Plugin.Actor
{
    /// <summary>
    /// 货主Grain
    /// </summary>
    public class CustomerGrain : EntityGrainBase<IcCustomer>, ICustomerGrain
    {
        #region 属性

        /// <summary>
        /// ID(映射表ID字段)
        /// </summary>
        protected override long Id
        {
            get { return Kernel.Id; }
        }

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override IcCustomer Kernel
        {
            get
            {
                return base.Kernel ?? (base.Kernel = IcCustomer.FetchRoot(Database,
                           p => p.Name == Name,
                           () => IcCustomer.New(Database,
                               IcCustomer.Set(p => p.Name, Name))));
            }
        }

        #endregion

        #region 方法

        async Task ICustomerGrain.LoadLocation(string brand, string cardNumber, string transportNumber, int weight, string locationArea, string locationAlley, string locationOrdinal)
        {
            await Kernel.LoadLocation(brand, cardNumber, transportNumber, weight, locationArea, locationAlley, locationOrdinal);
        }

        async Task<bool> ICustomerGrain.PickInventory(long pickMarks, string brand, string cardNumber, string transportNumber, int minTotalWeight, int maxTotalWeight)
        {
            return await Kernel.PickInventory(pickMarks, brand, cardNumber, transportNumber, minTotalWeight, maxTotalWeight);
        }

        async Task ICustomerGrain.UnloadLocation(long pickMarks)
        {
            await Kernel.UnloadLocation(pickMarks);
        }

        #endregion
    }
}