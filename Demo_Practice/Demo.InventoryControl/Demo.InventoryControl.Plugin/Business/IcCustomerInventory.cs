using System.Data.Common;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;

namespace Demo.InventoryControl.Plugin.Business
{
    /// <summary>
    /// 货主库存
    /// </summary>
    public class IcCustomerInventory : EntityBase<IcCustomerInventory>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private IcCustomerInventory()
        {
            //禁止添加代码
        }

        public IcCustomerInventory(long customerId, string brand, string cardNumber, string transportNumber, int weight,
            string locationArea, string locationAlley, string locationOrdinal, long stackOrdinal)
        {
            _id = Database.Sequence.Value;

            _customerId = customerId;
            _brand = brand;
            _cardNumber = cardNumber;
            _transportNumber = transportNumber;
            _weight = weight;
            _locationArea = locationArea;
            _locationAlley = locationAlley;
            _locationOrdinal = locationOrdinal;
            _stackOrdinal = stackOrdinal;
        }

        #region 属性

        #region 基本属性

        private long _id;

        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private long _customerId;

        /// <summary>
        /// 货主
        /// </summary>
        public long CustomerId
        {
            get { return _customerId; }
        }

        private string _brand;

        /// <summary>
        /// 品牌
        /// </summary>
        public string Brand
        {
            get { return _brand; }
        }

        private string _cardNumber;

        /// <summary>
        /// 卡号
        /// </summary>
        public string CardNumber
        {
            get { return _cardNumber; }
        }

        private string _transportNumber;

        /// <summary>
        /// 车皮/箱号
        /// </summary>
        public string TransportNumber
        {
            get { return _transportNumber; }
        }

        private int _weight;

        /// <summary>
        /// 重量
        /// </summary>
        public int Weight
        {
            get { return _weight; }
        }

        private string _locationArea;

        /// <summary>
        /// 货架库区
        /// </summary>
        public string LocationArea
        {
            get { return _locationArea; }
        }

        private string _locationAlley;

        /// <summary>
        /// 货架巷道
        /// </summary>
        public string LocationAlley
        {
            get { return _locationAlley; }
        }

        private string _locationOrdinal;

        /// <summary>
        /// 货架序号
        /// </summary>
        public string LocationOrdinal
        {
            get { return _locationOrdinal; }
        }

        private long _stackOrdinal;

        /// <summary>
        /// 货架LIFO序号
        /// </summary>
        public long StackOrdinal
        {
            get { return _stackOrdinal; }
        }

        /// <summary>
        /// 货架号
        /// </summary>
        public string Location
        {
            get { return AppConfig.FormatLocation(_locationArea, _locationAlley, _locationOrdinal); }
        }

        #endregion

        #region 动态属性

        private CustomerInventoryStatus _customerInventoryStatus;

        /// <summary>
        /// 货主库存状态
        /// </summary>
        public CustomerInventoryStatus CustomerInventoryStatus
        {
            get { return _customerInventoryStatus; }
        }

        private long? _pickMarks;

        /// <summary>
        /// 挑中标记号码
        /// </summary>
        public long? PickMarks
        {
            get { return _pickMarks; }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 标记挑中
        /// </summary>
        /// <param name="transaction">DbTransaction</param>
        /// <param name="pickMarks">挑中标记号码</param>
        public void MarkPicked(DbTransaction transaction, long pickMarks)
        {
            UpdateSelf(transaction,
                SetProperty(p => p.CustomerInventoryStatus, CustomerInventoryStatus.Picked),
                SetProperty(p => p.PickMarks, pickMarks));
        }

        /// <summary>
        /// 卸下货架
        /// </summary>
        /// <param name="pickMarks">挑中标记号码</param>
        /// <param name="transaction">DbTransaction</param>
        public bool Unload(DbTransaction transaction, long pickMarks)
        {
            if (PickMarks == pickMarks)
            {
                UpdateSelf(transaction,
                    SetProperty(p => p.CustomerInventoryStatus, CustomerInventoryStatus.NotStored));
                return true;
            }

            return false;
        }

        private static void Initialize(Database database)
        {
            database.ExecuteNonQuery(@"
CREATE TABLE IC_Customer_Inventory (
  II_ID NUMERIC(15) NOT NULL,
  II_Customer_Id NUMERIC(15) NOT NULL,
  II_Brand VARCHAR(50) NOT NULL,
  II_Card_Number VARCHAR(50) NOT NULL,
  II_Transport_Number VARCHAR(50) NOT NULL,
  II_Weight NUMERIC(5) NOT NULL,
  II_Location_Area VARCHAR(10) NOT NULL,
  II_Location_Alley VARCHAR(10) NOT NULL,
  II_Location_Ordinal VARCHAR(10) NOT NULL,
  II_Stack_Ordinal NUMERIC(15) NOT NULL,
  II_Customer_Inventory_Status NUMERIC(2) default 0 NOT NULL,
  II_Pick_Marks NUMERIC(15) NULL,
  PRIMARY KEY(II_ID)
)", false);
        }

        #endregion
    }
}