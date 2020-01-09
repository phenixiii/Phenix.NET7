using System.Collections.Generic;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;

namespace Demo.InventoryControl.Plugin.Business
{
    /// <summary>
    /// 货架
    /// </summary>
    public class IcLocation : RootEntityBase<IcLocation>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private IcLocation()
        {
            //禁止添加代码
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="area">库区</param>
        /// <param name="alley">巷道</param>
        /// <param name="ordinal">序号</param>
        public IcLocation(string area, string alley, string ordinal)
            : base(Database.Sequence.Value)
        {
            _area = area;
            _alley = alley;
            _ordinal = ordinal;
        }

        #region 属性

        #region 基本属性

        private string _area;

        /// <summary>
        /// 库区
        /// </summary>
        public string Area
        {
            get { return _area; }
        }

        private string _alley;

        /// <summary>
        /// 巷道
        /// </summary>
        public string Alley
        {
            get { return _alley; }
        }

        private string _ordinal;

        /// <summary>
        /// 序号
        /// </summary>
        public string Ordinal
        {
            get { return _ordinal; }
        }

        #endregion

        #region 动态属性

        private IList<IcCustomerInventory> _inventoryList;

        /// <summary>
        /// 货架库存
        /// </summary>
        public IList<IcCustomerInventory> InventoryList
        {
            get
            {
                return _inventoryList ?? (_inventoryList = IcCustomerInventory.Select(p =>
                               p.LocationArea == Area && p.LocationAlley == Alley && p.LocationOrdinal == Ordinal &&
                               p.CustomerInventoryStatus < CustomerInventoryStatus.NotStored,
                           IcCustomerInventory.Ascending(p => p.StackOrdinal)));
            }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 刷新状态
        /// </summary>
        public void Refresh()
        {
            _inventoryList = null;
        }

        private static void Initialize(Database database)
        {
            database.ExecuteNonQuery(@"
CREATE TABLE IC_Location (
  IL_ID NUMERIC(15) NOT NULL,
  IL_Area VARCHAR(10) NOT NULL,
  IL_Alley VARCHAR(10) NOT NULL,
  IL_Ordinal VARCHAR(10) NOT NULL,
  PRIMARY KEY(IL_ID),
  UNIQUE(IL_Area, IL_Alley, IL_Ordinal)
)", false);
        }

        #endregion
    }
}