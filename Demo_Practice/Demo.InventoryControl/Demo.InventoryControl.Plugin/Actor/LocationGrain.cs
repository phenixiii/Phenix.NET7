using System.Threading.Tasks;
using Demo.InventoryControl.Plugin.Business;
using Orleans;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Data.Schema;

namespace Demo.InventoryControl.Plugin.Actor
{
    /// <summary>
    /// 货架Grain
    /// </summary>
    public class LocationGrain : EntityGrainBase<IcLocation>, ILocationGrain
    {
        #region 属性

        private string _area;

        /// <summary>
        /// 库区
        /// </summary>
        public string Area
        {
            get { return _area ?? (_area = AppConfig.ExtractArea(this.GetPrimaryKeyString())); }
        }

        private string _alley;

        /// <summary>
        /// 巷道
        /// </summary>
        public string Alley
        {
            get { return _alley ?? (_alley = AppConfig.ExtractAlley(this.GetPrimaryKeyString())); }
        }

        private string _ordinal;

        /// <summary>
        /// 序号
        /// </summary>
        public string Ordinal
        {
            get { return _ordinal ?? (_ordinal = AppConfig.ExtractOrdinal(this.GetPrimaryKeyString())); }
        }

        /// <summary>
        /// ID(映射表XX_ID字段)
        /// </summary>
        protected override long Id
        {
            get { return Kernel.Id; }
        }

        private IcLocation _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override IcLocation Kernel
        {
            get
            {
                return _kernel ?? (_kernel = IcLocation.FetchRoot(Database.Default,
                           p => p.Area == Area && p.Alley == Alley && p.Ordinal == Ordinal,
                           () => IcLocation.New(Database.Default,
                               NameValue.Set<IcLocation>(p => p.Area, Area),
                               NameValue.Set<IcLocation>(p => p.Alley, Alley),
                               NameValue.Set<IcLocation>(p => p.Ordinal, Ordinal))));
            }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        Task<long> ILocationGrain.GetStackOrdinal()
        {
            return Task.FromResult(Kernel.GetStackOrdinal());
        }

        Task<int> ILocationGrain.GetUnloadValue(string brand, string cardNumber, string transportNumber)
        {
            return Task.FromResult(Kernel.GetUnloadValue(brand, cardNumber, transportNumber));
        }

        Task ILocationGrain.Refresh()
        {
            Kernel.Refresh();
            return Task.CompletedTask;
        }

        #endregion
    }
}
