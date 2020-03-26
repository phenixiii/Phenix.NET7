using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Phenix.Algorithm.CombinatorialOptimization;
using Phenix.Core.SyncCollections;

namespace Demo.InventoryControl.Plugin.Business.CustomerInventory
{
    internal class Alley : IGoodsBunch
    {
        public Alley(Area owner, string name)
        {
            _owner = owner;
            _name = name;
        }

        #region 属性

        #region 基本属性


        private readonly Area _owner;

        public Area Owner
        {
            get { return _owner; }
        }

        private readonly string _name;

        public string Name
        {
            get { return _name; }
        }

        #endregion


        #region 动态属性


        private readonly SynchronizedDictionary<string, Location> _locationDictionary = new SynchronizedDictionary<string, Location>(StringComparer.Ordinal);

        public bool Empty
        {
            get { return _locationDictionary.Count == 0; }
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

        private Location FetchLocation(string locationOrdinal)
        {
            return _locationDictionary.GetValue(locationOrdinal, () => new Location(this, locationOrdinal));
        }

        public void Add(IcCustomerInventory inventory)
        {
            FetchLocation(inventory.LocationOrdinal).Add(inventory);
        }

        public async Task Load(string brand, string cardNumber, string transportNumber, int weight,
            string locationArea, string locationAlley, string locationOrdinal)
        {
            await FetchLocation(locationOrdinal).Load(brand, cardNumber, transportNumber, weight, locationArea, locationAlley, locationOrdinal);
        }

        public async Task<bool> IsMatch(string brand, string cardNumber, string transportNumber)
        {
            int i = 0;
            _size = 0;
            _value = 0;
            _items.Clear();
            foreach (KeyValuePair<string, Location> kvp in _locationDictionary)
                if (await kvp.Value.IsMatch(brand, cardNumber, transportNumber))
                {
                    i = i + 1;
                    _size = _size + ((IGoods) kvp.Value).Size;
                    _value = _value + ((IGoods) kvp.Value).Value * ((IGoods) kvp.Value).Value;
                    _items.Add(kvp.Value);
                }

            if (_value > 0)
                _value = (int) Math.Round(Math.Sqrt((double) _value / i));
            return _items.Count > 0;
        }

        public void Unloading(DbTransaction transaction, long pickMarks)
        {
            foreach (KeyValuePair<string, Location> kvp in _locationDictionary)
                kvp.Value.Unloading(transaction, pickMarks);
        }

        public void Unloaded(long pickMarks, ref IList<string> locations)
        {
            foreach (Location item in new List<Location>(_locationDictionary.Values))
                if (item.Unloaded(pickMarks))
                {
                    if (item.Empty)
                        _locationDictionary.Remove(item.Ordinal);
                    locations.Add(item.Name);
                }
        }

        #endregion
    }
}