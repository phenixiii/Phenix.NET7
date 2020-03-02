using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Demo.InventoryControl.Plugin.Actor;
using Demo.InventoryControl.Plugin.Business.CustomerInventory;
using Phenix.Actor;
using Phenix.Algorithm.CombinatorialOptimization;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;

namespace Demo.InventoryControl.Plugin.Business
{
    /// <summary>
    /// 货主
    /// </summary>
    public class IcCustomer : EntityBase<IcCustomer>
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
        {
            _id = Database.Default.Sequence.Value;
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

        private Dictionary<string, Area> _areaDictionary;

        private IDictionary<string, Area> AreaDictionary
        {
            get
            {
                if (_areaDictionary == null)
                {
                    Dictionary<string, Area> result = new Dictionary<string, Area>(StringComparer.Ordinal);
                    foreach (IcCustomerInventory item in FetchDetails<IcCustomerInventory>(p =>
                            p.CustomerId == Id &&
                            p.CustomerInventoryStatus < CustomerInventoryStatus.NotStored,
                        IcCustomerInventory.Ascending(p => p.LocationArea),
                        IcCustomerInventory.Ascending(p => p.LocationAlley),
                        IcCustomerInventory.Ascending(p => p.LocationOrdinal),
                        IcCustomerInventory.Ascending(p => p.StackOrdinal)))
                    {
                        Area area;
                        if (!result.TryGetValue(item.LocationArea, out area))
                        {
                            area = new Area(this, item.LocationArea);
                            result.Add(item.LocationArea, area);
                        }

                        area.Add(item);
                    }

                    _areaDictionary = result;
                }

                return _areaDictionary;
            }
        }

        #endregion

        #endregion

        #region 方法

        private Area FetchArea(string locationArea)
        {
            Area result;
            if (!AreaDictionary.TryGetValue(locationArea, out result))
            {
                result = new Area(this, locationArea);
                AreaDictionary.Add(locationArea, result);
            }

            return result;
        }

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
        public async Task LoadLocation(string brand, string cardNumber, string transportNumber, int weight,
            string locationArea, string locationAlley, string locationOrdinal)
        {
            await FetchArea(locationArea).Load(brand, cardNumber, transportNumber, weight, locationArea, locationAlley, locationOrdinal);
        }

        /// <summary>
        /// 挑选货物
        /// </summary>
        /// <param name="pickMarks">挑中标记号码</param>
        /// <param name="brand">品牌(null代表忽略本筛选条件)</param>
        /// <param name="cardNumber">卡号(null代表忽略本筛选条件)</param>
        /// <param name="transportNumber">车皮/箱号(null代表忽略本筛选条件)</param>
        /// <param name="minTotalWeight">最小总重</param>
        /// <param name="maxTotalWeight">最大总重</param>
        public async Task<bool> PickInventory(long pickMarks, string brand, string cardNumber, string transportNumber, int minTotalWeight, int maxTotalWeight)
        {
            IList<IGoods> matchedAreas = new List<IGoods>();
            foreach (KeyValuePair<string, Area> kvp in AreaDictionary)
                if (await kvp.Value.IsMatch(brand, cardNumber, transportNumber))
                    matchedAreas.Add(kvp.Value);

            PackedBunches packedBunches = BunchKnapsackProblem.Pack(matchedAreas, minTotalWeight, maxTotalWeight - minTotalWeight);
            if (packedBunches != null)
            {
                foreach (string location in SelfSheet.Owner.Database.ExecuteGet(DoMarkPicked, pickMarks, packedBunches.AtomicValue))
                    await ClusterClient.Default.GetGrain<ILocationGrain>(location).Refresh();
                return true;
            }

            return false;
        }

        private IList<string> DoMarkPicked(DbTransaction transaction, long pickMarks, IList<IGoods> inventoryList)
        {
            IList<string> result = new List<string>();
            foreach (IcCustomerInventory item in inventoryList)
                item.MarkPicked(transaction, pickMarks, ref result);
            return result;
        }

        /// <summary>
        /// 卸下货架
        /// </summary>
        /// <param name="pickMarks">挑中标记号码</param>
        /// <returns>受影响的货架号清单</returns>
        public async Task UnloadLocation(long pickMarks)
        {
            SelfSheet.Owner.Database.Execute(DoUnloading, pickMarks);
            foreach (string location in DoUnloaded(pickMarks))
                await ClusterClient.Default.GetGrain<ILocationGrain>(location).Refresh();
        }

        private void DoUnloading(DbTransaction transaction, long pickMarks)
        {
            foreach (KeyValuePair<string, Area> kvp in AreaDictionary)
                kvp.Value.Unloading(transaction, pickMarks);
        }

        private IList<string> DoUnloaded(long pickMarks)
        {
            IList<string> result = new List<string>();
            foreach (Area item in new List<Area>(AreaDictionary.Values))
            {
                item.Unloaded(pickMarks, ref result);
                if (item.Empty)
                    AreaDictionary.Remove(item.Name);
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