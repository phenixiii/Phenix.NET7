using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Phenix.Algorithm.CombinatorialOptimization;

namespace Demo.InventoryControl.Plugin.Business.CustomerInventory
{
    internal class Area : IGoodsBunch
    {
        public Area(IcCustomer owner, string name)
        {
            _owner = owner;
            _name = name;
        }

        #region 属性

        #region 基本属性

        private readonly IcCustomer _owner;

        public IcCustomer Owner
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

        private readonly Dictionary<string, Alley> _alleyDictionary = new Dictionary<string, Alley>(StringComparer.Ordinal);

        public bool Empty
        {
            get { return _alleyDictionary.Count == 0; }
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

        private Alley FetchAlley(string locationAlley)
        {
            Alley result;
            if (!_alleyDictionary.TryGetValue(locationAlley, out result))
            {
                result = new Alley(this, locationAlley);
                _alleyDictionary.Add(locationAlley, result);
            }

            return result;
        }

        public void Add(IcCustomerInventory inventory)
        {
            FetchAlley(inventory.LocationAlley).Add(inventory);
        }

        public async Task Load(string brand, string cardNumber, string transportNumber, int weight,
            string locationArea, string locationAlley, string locationOrdinal)
        {
            await FetchAlley(locationAlley).Load(brand, cardNumber, transportNumber, weight, locationArea, locationAlley, locationOrdinal);
        }

        public async Task<bool> IsMatch(string brand, string cardNumber, string transportNumber)
        {
            int i = 0;
            _size = 0;
            _value = 0;
            _items.Clear();
            foreach (KeyValuePair<string, Alley> kvp in _alleyDictionary)
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

        public void Unload(DbTransaction transaction, long pickMarks, ref IList<string> locations)
        {
            foreach (Alley item in new List<Alley>(_alleyDictionary.Values))
            {
                item.Unload(transaction, pickMarks, ref locations);
                if (item.Empty)
                    _alleyDictionary.Remove(item.Name);
            }
        }

        #endregion
    }
}