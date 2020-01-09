using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;

namespace Demo.InventoryControl.Plugin.Business
{
    /// <summary>
    /// 货主
    /// </summary>
    public class IcCustomer : RootEntityBase<IcCustomer>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private IcCustomer()
        {
            //禁止添加代码
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="name">名称</param>
        public IcCustomer(string name)
            : base(Database.Sequence.Value)
        {
            _name = name;
        }

        #region 属性

        #region 基本属性

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        #endregion

        #region 动态属性

        private IList<IcCustomerInventory> _inventoryList;

        /// <summary>
        /// 货主库存
        /// </summary>
        public IList<IcCustomerInventory> InventoryList
        {
            get
            {
                return _inventoryList ?? (_inventoryList = IcCustomerInventory.Select(p =>
                               p.CustomerId == Id &&
                               p.CustomerInventoryStatus < CustomerInventoryStatus.NotStored,
                           IcCustomerInventory.Ascending(p => p.LocationArea),
                           IcCustomerInventory.Ascending(p => p.LocationAlley),
                           IcCustomerInventory.Ascending(p => p.LocationOrdinal),
                           IcCustomerInventory.Ascending(p => p.StackOrdinal)));
            }
        }

        private Dictionary<string, List<IcCustomerInventory>> _locationInventoryList;

        /// <summary>
        /// 货架-货主库存
        /// </summary>
        public IDictionary<string, List<IcCustomerInventory>> LocationInventoryList
        {
            get
            {
                if (_locationInventoryList == null)
                {
                    _locationInventoryList = new Dictionary<string, List<IcCustomerInventory>>(StringComparer.Ordinal);
                    foreach (IcCustomerInventory item in InventoryList)
                    {
                        List<IcCustomerInventory> value;
                        if (!_locationInventoryList.TryGetValue(item.Location, out value))
                        {
                            value = new List<IcCustomerInventory>();
                            _locationInventoryList.Add(item.Location, value);
                        }

                        value.Add(item);
                    }
                }

                return _locationInventoryList;
            }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 装上货架
        /// </summary>
        public void LoadLocation(string brand, string cardNumber, string transportNumber, int weight,
            string locationArea, string locationAlley, string locationOrdinal)
        {
            string location = AppConfig.FormatLocation(locationArea, locationAlley, locationOrdinal);

            List<IcCustomerInventory> value;
            if (!LocationInventoryList.TryGetValue(location, out value))
            {
                value = new List<IcCustomerInventory>();
                LocationInventoryList.Add(location, value);
            }

            IcCustomerInventory customerInventory = new IcCustomerInventory(Id, brand, cardNumber, transportNumber, weight,
                locationArea, locationAlley, locationOrdinal, value.Count > 0 ? value.Last().StackOrdinal + 1 : 1);
            customerInventory.InsertSelf();
            value.Add(customerInventory);
            InventoryList.Add(customerInventory);
        }
        
        /// <summary>
        /// 卸下货架
        /// </summary>
        /// <param name="pickMarks">挑中标记号码</param>
        /// <returns>受影响的货架号清单</returns>
        public IList<string> UnloadLocation(long pickMarks)
        {
            return Database.ExecuteGet(DoUnloadLocation, pickMarks);
        }

        private IList<string> DoUnloadLocation(DbTransaction transaction, long pickMarks)
        {
            IList<string> result = new List<string>();
            for (int i = InventoryList.Count - 1; i >= 0; i--)
            {
                IcCustomerInventory item = InventoryList[i];
                if (item.Unload(transaction, pickMarks))
                {
                    InventoryList.Remove(item);
                    if (LocationInventoryList.TryGetValue(item.Location, out List<IcCustomerInventory> value))
                        value.Remove(item);
                    result.Add(item.Location);
                }
            }

            return result;
        }

        private static void Initialize(Database database)
        {
            database.ExecuteNonQuery(@"
CREATE TABLE IC_Customer (
  IC_ID NUMERIC(15) NOT NULL,
  IC_Name VARCHAR(100) NOT NULL,
  PRIMARY KEY(IC_ID),
  UNIQUE(IC_Name)
)", false);
        }

        #endregion
    }
}