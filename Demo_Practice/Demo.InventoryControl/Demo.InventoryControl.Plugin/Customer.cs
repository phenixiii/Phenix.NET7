using System;
using System.Threading.Tasks;
using Demo.InventoryControl.Plugin.Actor;
using Phenix.Actor;

namespace Demo.InventoryControl.Plugin
{
    /// <summary>
    /// 货主
    /// </summary>
    [Serializable]
    public class Customer
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private Customer()
        {
            //禁止添加代码
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">名称</param>
        [Newtonsoft.Json.JsonConstructor]
        public Customer(string name)
        {
            _name = name;
        }

        #region 属性

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        [NonSerialized]
        private ICustomerGrain _grain;

        /// <summary>
        /// ICustomerGrain
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public ICustomerGrain Grain
        {
            get { return _grain ?? (_grain = ClusterClient.Default.GetGrain<ICustomerGrain>(Name)); }
        }

        #endregion

        #region 方法

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
        public Task LoadLocation(string brand, string cardNumber, string transportNumber, int weight, string locationArea, string locationAlley, string locationOrdinal)
        {
            return Grain.LoadLocation(brand, cardNumber, transportNumber, weight, locationArea, locationAlley, locationOrdinal);
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
        public Task<bool> PickInventory(long pickMarks, string brand, string cardNumber, string transportNumber, int minTotalWeight, int maxTotalWeight)
        {
            return Grain.PickInventory(pickMarks, brand, cardNumber, transportNumber, minTotalWeight, maxTotalWeight);
        }

        /// <summary>
        /// 卸下货架
        /// </summary>
        /// <param name="pickMarks">挑中标记号码</param>
        public Task UnloadLocation(long pickMarks)
        {
            return Grain.UnloadLocation(pickMarks);
        }

        #endregion
    }
}