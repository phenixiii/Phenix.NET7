using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Demo.InventoryControl.Plugin.Actor;
using Phenix.Actor;
using Phenix.Algorithm.CombinatorialOptimization;

namespace Demo.InventoryControl.Plugin.Business.CustomerInventory
{
    internal class Location : IGoodsBunch
    {
        public Location(Alley owner, string ordinal)
        {
            _owner = owner;
            _ordinal = ordinal;
        }

        #region 属性

        #region 基本属性

        private readonly Alley _owner;

        public Alley Owner
        {
            get { return _owner; }
        }

        private readonly string _ordinal;

        public string Ordinal
        {
            get { return _ordinal; }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return AppConfig.FormatLocation(_owner.Owner.Name, _owner.Name, _ordinal); }
        }

        #endregion

        #region 动态属性


        private readonly List<IcCustomerInventory> _inventoryList = new List<IcCustomerInventory>();

        public bool Empty
        {
            get { return _inventoryList.Count == 0; }
        }

        private int _size;

        int IGoods.Size
        {
            get { return _size; }
        }

        private int _value;

        int IGoods.Value
        {
            get { return _value; }
        }

        private readonly IList<IGoods> _items = new List<IGoods>();

        IList<IGoods> IGoodsBunch.Items
        {
            get { return _items; }
        }

        #endregion

        #endregion

        #region 方法

        public void Add(IcCustomerInventory inventory)
        {
            _inventoryList.Add(inventory);
        }

        public async Task Load(string brand, string cardNumber, string transportNumber, int weight,
            string locationArea, string locationAlley, string locationOrdinal)
        {
            string location = AppConfig.FormatLocation(locationArea, locationAlley, locationOrdinal);
            ILocationGrain locationGrain = ClusterClient.Default.GetGrain<ILocationGrain>(location);
            IcCustomerInventory inventory = new IcCustomerInventory(Owner.Owner.Owner.Id, brand, cardNumber, transportNumber, weight,
                locationArea, locationAlley, locationOrdinal, await locationGrain.GetStackOrdinal());
            inventory.InsertSelf();
            await locationGrain.Refresh();
            Add(inventory);
        }

        public async Task<bool> IsMatch(string brand, string cardNumber, string transportNumber)
        {
            int i = 0;
            _size = 0;
            _value = 0;
            _items.Clear();
            foreach (IcCustomerInventory item in _inventoryList)
            {
                i = i + 1;
                if (item.IsMatch(brand, cardNumber, transportNumber))
                {
                    _size = _size + ((IGoods) item).Size;
                    if (_items.Count == 0)
                        _value = await ClusterClient.Default.GetGrain<ILocationGrain>(Name).GetUnloadValue(brand, cardNumber, transportNumber);
                    _items.Add(item);
                    item.ResetValue((int) Math.Round(Math.Sqrt((_value * _value + (double) i * i * 81 / (_inventoryList.Count * _inventoryList.Count)) / 2)));
                }
            }

            return _items.Count > 0;
        }

        public bool Unload(DbTransaction transaction, long pickMarks)
        {
            bool result = false;
            for (int i = _inventoryList.Count - 1; i >= 0; i--)
            {
                IcCustomerInventory item = _inventoryList[i];
                if (item.Unload(transaction, pickMarks))
                {
                    _inventoryList.Remove(item);
                    result = true;
                }
            }

            return result;
        }

        #endregion
    }
}