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

        Task ICustomerGrain.LoadLocation(string brand, string cardNumber, string transportNumber, int weight, string locationArea, string locationAlley, string locationOrdinal)
        {
            Kernel.LoadLocation(brand, cardNumber, transportNumber, weight, locationArea, locationAlley, locationOrdinal);
            RefreshLocation(AppConfig.FormatLocation(locationArea, locationAlley, locationOrdinal));
            return Task.CompletedTask;
        }

        Task ICustomerGrain.UnloadLocation(long pickMarks)
        {
            foreach (string location in Kernel.UnloadLocation(pickMarks))
                RefreshLocation(location);
            return Task.CompletedTask;
        }

        private void RefreshLocation(string location)
        {
            ILocationGrain locationGrain = ClusterClient.Default.GetGrain<ILocationGrain>(location);
            locationGrain.Refresh();
        }

        #endregion
    }
}