using System;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;

namespace Demo.InspectionStation.Plugin.Business
{
    /// <summary>
    /// 作业点
    /// </summary>
    [Serializable]
    public class IsOperationPoint : EntityBase<IsOperationPoint>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private IsOperationPoint()
        {
            //禁止添加代码
        }

        /// <summary>
        /// 初始化
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        public IsOperationPoint(string name,
            OperationPointStatus operationPointStatus,
            int weighbridge, DateTime weighbridgeAliveTime,
            string licensePlate, DateTime licensePlateAliveTime,
            DateTime permitThroughTime)
        {
            _name = name;
            _operationPointStatus = operationPointStatus;
            _weighbridge = weighbridge;
            _weighbridgeAliveTime = weighbridgeAliveTime;
            _licensePlate = licensePlate;
            _licensePlateAliveTime = licensePlateAliveTime;
            _permitThroughTime = permitThroughTime;
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

        private OperationPointStatus _operationPointStatus;

        /// <summary>
        /// 作业点状态
        /// </summary>
        public OperationPointStatus OperationPointStatus
        {
            get
            {
                if ((_operationPointStatus == OperationPointStatus.PermitThrough && PermitThroughTime.AddSeconds(10) < DateTime.Now || _operationPointStatus == OperationPointStatus.Shutdown) && Weighbridge == 0 && String.IsNullOrEmpty(LicensePlate) && IsAlive)
                    _operationPointStatus = OperationPointStatus.Standby;
                if ((_operationPointStatus == OperationPointStatus.Standby || _operationPointStatus == OperationPointStatus.PermitThrough) && !IsAlive)
                    _operationPointStatus = OperationPointStatus.Shutdown;
                if ((_operationPointStatus == OperationPointStatus.Shutdown || _operationPointStatus == OperationPointStatus.Standby) && (Weighbridge != 0 || !String.IsNullOrEmpty(LicensePlate)))
                    _operationPointStatus = OperationPointStatus.CheckIn;
                return _operationPointStatus;
            }
        }

        private int _weighbridge;

        /// <summary>
        /// 磅秤重
        /// </summary>
        public int Weighbridge
        {
            get { return _weighbridge; }
        }

        private DateTime? _weighbridgeAliveTime;

        /// <summary>
        /// 磅秤(读取设备)心跳时间
        /// </summary>
        public DateTime WeighbridgeAliveTime
        {
            get { return _weighbridgeAliveTime ?? DateTime.MinValue; }
        }

        private string _licensePlate;

        /// <summary>
        /// 车牌号
        /// </summary>
        public string LicensePlate
        {
            get { return _licensePlate; }
        }

        private DateTime? _licensePlateAliveTime;

        /// <summary>
        /// 车牌(识别设备)心跳时间
        /// </summary>
        public DateTime LicensePlateAliveTime
        {
            get { return _licensePlateAliveTime ?? DateTime.MinValue; }
        }

        private DateTime? _permitThroughTime;

        /// <summary>
        /// 放行时间
        /// </summary>
        public DateTime PermitThroughTime
        {
            get { return _permitThroughTime ?? DateTime.MinValue; }
        }

        /// <summary>
        /// 是否活着
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsAlive
        {
            get { return WeighbridgeAliveTime.AddSeconds(10) >= DateTime.Now && LicensePlateAliveTime.AddSeconds(10) >= DateTime.Now; }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 设置磅秤重
        /// </summary>
        /// <param name="value">值</param>
        public void SetWeighbridge(int value)
        {
            UpdateSelf(
                SetProperty(p => p.Weighbridge, value), 
                SetProperty(p => p.WeighbridgeAliveTime, DateTime.Now));
        }

        /// <summary>
        /// 磅秤(读取设备)心跳(每10秒至少2次)
        /// </summary>
        public void WeighbridgeAlive()
        {
            UpdateSelf(SetProperty(p => p.WeighbridgeAliveTime, DateTime.Now));
        }

        /// <summary>
        /// 设置车牌号
        /// </summary>
        /// <param name="value">值</param>
        public void SetLicensePlate(string value)
        {
            UpdateSelf(
                SetProperty(p => p.LicensePlate, value), 
                SetProperty(p => p.LicensePlateAliveTime, DateTime.Now));
        }

        /// <summary>
        /// 车牌(识别设备)心跳(每10秒至少2次)
        /// </summary>
        public void LicensePlateAlive()
        {
            UpdateSelf(SetProperty(p => p.LicensePlateAliveTime, DateTime.Now));
        }

        /// <summary>
        /// 放行
        /// </summary>
        public void PermitThrough()
        {
            UpdateSelf(
                SetProperty(p => p.Weighbridge, 0),
                SetProperty(p => p.LicensePlate, 0),
                SetProperty(p => p.PermitThroughTime, DateTime.Now));
            _operationPointStatus = OperationPointStatus.PermitThrough;
        }

        private static void Initialize(Database database)
        {
            database.ExecuteNonQuery(@"
CREATE TABLE IS_Operation_Point (
  IP_ID NUMERIC(15) NOT NULL,
  IP_Name VARCHAR(100) NOT NULL,
  IP_Weighbridge NUMERIC(5) default 0 NOT NULL,
  IP_Weighbridge_Alive_Time DATETIME NULL,
  IP_License_Plate VARCHAR(10) NULL,
  IP_License_Plate_Alive_Time DATETIME NULL,
  IP_Permit_Through_Time DATETIME NULL,
  PRIMARY KEY(IP_ID),
  UNIQUE(IP_Name)
)", false);
        }

        #endregion
    }
}