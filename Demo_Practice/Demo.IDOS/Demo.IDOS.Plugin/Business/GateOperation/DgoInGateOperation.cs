using System;
using Demo.IDOS.Plugin.Rule;
using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2020-08-13 15:41:31
   mapping to: dgo_in_gate_operation
*/

namespace Demo.IDOS.Plugin.Business.GateOperation
{
    /// <summary>
    /// 进场道口作业
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = "进场道口作业")]
    public class DgoInGateOperation : EntityBase<DgoInGateOperation>
    {
        private DgoInGateOperation()
        {
            // used to fetch object, do not add code
        }

        [Newtonsoft.Json.JsonConstructor]
        public DgoInGateOperation(string dataSourceKey, long id,
            long dpId, string gateName, string licensePlate, string bookingNumber, OperationType operationType, DateTime operationTime) 
            : base(dataSourceKey, id)
        {
            _dpId = dpId;
            _gateName = gateName;
            _licensePlate = licensePlate;
            _bookingNumber = bookingNumber;
            _operationType = operationType;
            _operationTime = operationTime;
        }

        protected override void InitializeSelf()
        {
        }

        private long _dpId;
        /// <summary>
        /// 仓库
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"仓库")]
        public long DpId
        {
            get { return _dpId; }
            set { _dpId = value; }
        }

        private string _gateName;
        /// <summary>
        /// 道口名称
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"道口名称")]
        public string GateName
        {
            get { return _gateName; }
            set { _gateName = value; }
        }

        private string _licensePlate;
        /// <summary>
        /// 车牌号
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"车牌号")]
        public string LicensePlate
        {
            get { return _licensePlate; }
            set { _licensePlate = value; }
        }

        private string _bookingNumber;
        /// <summary>
        /// 预约单号
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"预约单号")]
        public string BookingNumber
        {
            get { return _bookingNumber; }
            set { _bookingNumber = value; }
        }

        private OperationType _operationType;
        /// <summary>
        /// 作业类型
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"作业类型")]
        public OperationType OperationType
        {
            get { return _operationType; }
            set { _operationType = value; }
        }

        private DateTime _operationTime;
        /// <summary>
        /// 作业时间
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"作业时间")]
        public DateTime OperationTime
        {
            get { return _operationTime; }
            set { _operationTime = value; }
        }

    }
}
