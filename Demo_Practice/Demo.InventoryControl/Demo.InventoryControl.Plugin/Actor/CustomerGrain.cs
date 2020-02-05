using System.Threading.Tasks;
using Demo.InventoryControl.Plugin.Business;
using Orleans;
using Phenix.Actor;
using Phenix.Core.Data.Model;

namespace Demo.InventoryControl.Plugin.Actor
{
    /// <summary>
    /// 货主Grain
    /// </summary>
    public class CustomerGrain : RootEntityGrainBase<IcCustomer>, ICustomerGrain
    {
        #region 属性

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name ?? (_name = this.GetPrimaryKeyString()); }
        }

        /// <summary>
        /// ID(映射表XX_ID字段)
        /// </summary>
        protected override long Id
        {
            get { return Kernel.Id; }
        }

        private IcCustomer _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override IcCustomer Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    _kernel = RootEntityBase<IcCustomer>.Fetch(p => p.Name == Name);
                    if (_kernel == null)
                    {
                        _kernel = new IcCustomer(Name);
                        _kernel.InsertSelf();
                    }
                }

                return _kernel;
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