using System;
using Demo.IDOS.Plugin.Rule;
using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2020-08-13 15:41:31
   mapping to: dgo_in_gate_operation
*/

namespace Demo.IDOS.Plugin.Business.DepotGateOperation
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
            long dpId, string gateName, string licensePlate, string bookingNumber, OperationType operationType, DateTime operationStartTime, DateTime? operationFinishTime, string goodsNumber, string goodsType, GoodsSize goodsSize) 
            : base(dataSourceKey, id)
        {
            _dpId = dpId;
            _gateName = gateName;
            _licensePlate = licensePlate;
            _bookingNumber = bookingNumber;
            _operationType = operationType;
            _operationStartTime = operationStartTime;
            _operationFinishTime = operationFinishTime;
            _goodsNumber = goodsNumber;
            _goodsType = goodsType;
            _goodsSize = goodsSize;
        }

        protected override void InitializeSelf()
        {
            _bookingNumber = "*";
            _operationType = 0;
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

        private DateTime _operationStartTime;
        /// <summary>
        /// 作业开始时间
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"作业开始时间")]
        public DateTime OperationStartTime
        {
            get { return _operationStartTime; }
            set { _operationStartTime = value; }
        }

        private DateTime? _operationFinishTime;
        /// <summary>
        /// 作业结束时间
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"作业结束时间")]
        public DateTime? OperationFinishTime
        {
            get { return _operationFinishTime; }
            set { _operationFinishTime = value; }
        }

        private string _goodsNumber;
        /// <summary>
        /// 货物编号
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"货物编号")]
        public string GoodsNumber
        {
            get { return _goodsNumber; }
            set { _goodsNumber = value; }
        }

        private string _goodsType;
        /// <summary>
        /// 货物规格
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"货物规格")]
        public string GoodsType
        {
            get { return _goodsType; }
            set { _goodsType = value; }
        }

        private GoodsSize _goodsSize;
        /// <summary>
        /// 货物尺寸
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = @"货物尺寸")]
        public GoodsSize GoodsSize
        {
            get { return _goodsSize; }
            set { _goodsSize = value; }
        }

    }
}
