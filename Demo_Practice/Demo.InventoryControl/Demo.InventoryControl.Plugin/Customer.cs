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
        /// IRootEntityGrain
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
        public Task LoadLocation(string brand, string cardNumber, string transportNumber, int weight, string locationArea, string locationAlley, string locationOrdinal)
        {
            return Grain.LoadLocation(brand, cardNumber, transportNumber, weight, locationArea, locationAlley, locationOrdinal);
        }

        /// <summary>
        /// 卸下货架
        /// </summary>
        public Task UnloadLocation(long pickMarks)
        {
            return Grain.UnloadLocation(pickMarks);
        }

        #endregion
    }
}