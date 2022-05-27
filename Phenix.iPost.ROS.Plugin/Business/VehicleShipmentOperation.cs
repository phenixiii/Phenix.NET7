using System;
using Phenix.iPost.ROS.Plugin.Business.Norms;

namespace Phenix.iPost.ROS.Plugin.Business
{
    /// <summary>
    /// 装船作业
    /// </summary>
    [Serializable]
    public class VehicleShipmentOperation : VehicleOperation
    {
        /// <summary>
        /// 装船作业
        /// </summary>
        /// <param name="owner">主人</param>
        public VehicleShipmentOperation(Vehicle owner)
            : base(owner)
        {
        }

        #region 属性

        private bool _oneByOneLoading;

        /// <summary>
        /// 双箱时有意义一个一个装船
        /// </summary>
        public bool OneByOneLoading
        {
            get { return _oneByOneLoading; }
            set { _oneByOneLoading = value; }
        }

        private VehicleShipmentOperationStatus _status;

        /// <summary>
        /// 装船作业状态
        /// </summary>
        public VehicleShipmentOperationStatus Status
        {
            get { return _status; }
        }

        private VehicleYardOperationStatus _yardReceive1;

        /// <summary>
        /// 堆场收箱1
        /// </summary>
        public VehicleYardOperationStatus YardReceive1
        {
            get { return _yardReceive1; }
        }

        private VehicleYardOperationStatus _yardReceive2;

        /// <summary>
        /// 堆场收箱2
        /// </summary>
        public VehicleYardOperationStatus YardReceive2
        {
            get { return _yardReceive2; }
        }

        private VehicleBerthOperationStatus _berthDeliver1;

        /// <summary>
        /// 泊位送箱1
        /// </summary>
        public VehicleBerthOperationStatus BerthDeliver1
        {
            get { return _berthDeliver1; }
        }

        private VehicleBerthOperationStatus _berthDeliver2;

        /// <summary>
        /// 泊位送箱2
        /// </summary>
        public VehicleBerthOperationStatus BerthDeliver2
        {
            get { return _berthDeliver2; }
        }

        /// <summary>
        /// 收箱中
        /// </summary>
        public override bool Receiving
        {
            get { return _status < VehicleShipmentOperationStatus.BerthDeliver1 && !Deliverable; }
        }

        /// <summary>
        /// 待送箱
        /// </summary>
        public override bool Deliverable
        {
            get
            {
                if (_status is VehicleShipmentOperationStatus.YardReceive1 or VehicleShipmentOperationStatus.YardReceive2)
                {
                    if (ValidTask1And2)
                        return _yardReceive1 == VehicleYardOperationStatus.Leave && _yardReceive2 == VehicleYardOperationStatus.Leave;
                    if (ValidTask1Only)
                        return _yardReceive1 == VehicleYardOperationStatus.Leave;
                    if (ValidTask2Only)
                        return _yardReceive2 == VehicleYardOperationStatus.Leave;
                }

                return false;
            }
        }

        /// <summary>
        /// 送箱中
        /// </summary>
        public override bool Delivering
        {
            get
            {
                if (_status is VehicleShipmentOperationStatus.BerthDeliver1 or VehicleShipmentOperationStatus.BerthDeliver2)
                {
                    if (ValidTask1And2)
                        return _berthDeliver1 != VehicleBerthOperationStatus.Leave || _berthDeliver2 != VehicleBerthOperationStatus.Leave;
                    if (ValidTask1Only)
                        return _berthDeliver1 != VehicleBerthOperationStatus.Leave;
                    if (ValidTask2Only)
                        return _berthDeliver2 != VehicleBerthOperationStatus.Leave;
                }

                return false;
            }
        }

        #endregion

        #region 方法

        #region Task

        /// <summary>
        /// 下一任务
        /// </summary>
        public override bool NextTask()
        {
            bool result = true;

            if (Receiving)
            {
                if (ValidTask1And2)
                {
                    if (_yardReceive1 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShipmentOperationStatus.YardReceive1;
                    else if (_yardReceive2 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShipmentOperationStatus.YardReceive2;
                    else
                        result = false;
                }
                else if (ValidTask1Only)
                {
                    if (_yardReceive1 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShipmentOperationStatus.YardReceive1;
                    else
                        result = false;
                }
                else if (ValidTask2Only)
                {
                    if (_yardReceive2 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShipmentOperationStatus.YardReceive2;
                    else
                        result = false;
                }
            }
            else if (Deliverable || Delivering)
            {
                if (ValidTask1And2)
                {
                    if (_berthDeliver1 != VehicleBerthOperationStatus.Leave)
                        _status = VehicleShipmentOperationStatus.BerthDeliver1;
                    else if (_berthDeliver2 != VehicleBerthOperationStatus.Leave)
                        _status = VehicleShipmentOperationStatus.BerthDeliver2;
                    else
                        result = false;
                }
                else if (ValidTask1Only)
                {
                    if (_berthDeliver1 != VehicleBerthOperationStatus.Leave)
                        _status = VehicleShipmentOperationStatus.BerthDeliver1;
                    else
                        result = false;
                }
                else if (ValidTask2Only)
                {
                    if (_berthDeliver2 != VehicleBerthOperationStatus.Leave)
                        _status = VehicleShipmentOperationStatus.BerthDeliver2;
                    else
                        result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        public override bool RunTask()
        {
            switch (_status)
            {
                case VehicleShipmentOperationStatus.YardReceive1 when _yardReceive1 == VehicleYardOperationStatus.Standby:
                    _yardReceive1 = VehicleYardOperationStatus.Issued;
                    return true;
                case VehicleShipmentOperationStatus.YardReceive2 when _yardReceive2 == VehicleYardOperationStatus.Standby:
                    _yardReceive2 = VehicleYardOperationStatus.Issued;
                    return true;
                case VehicleShipmentOperationStatus.BerthDeliver1 when _berthDeliver1 == VehicleBerthOperationStatus.Standby:
                    _berthDeliver1 = VehicleBerthOperationStatus.Issued;
                    return true;
                case VehicleShipmentOperationStatus.BerthDeliver2 when _berthDeliver2 == VehicleBerthOperationStatus.Standby:
                    _berthDeliver2 = VehicleBerthOperationStatus.Issued;
                    return true;
            }

            return false;
        }

        #endregion

        #region Event

        /// <summary>
        /// 泊位作业中
        /// </summary>
        /// <param name="status">泊位作业状态</param>
        public override void OnBerthOperation(VehicleBerthOperationStatus status)
        {
            if (status == VehicleBerthOperationStatus.Standby)
                throw new InvalidOperationException($"{status}无意义被忽略!");

            switch (_status)
            {
                case VehicleShipmentOperationStatus.BerthDeliver1:
                    _berthDeliver1 = status;
                    break;
                case VehicleShipmentOperationStatus.BerthDeliver2:
                    _berthDeliver2 = status;
                    break;
                default:
                    throw new InvalidOperationException($"{status}不合时宜({_status})被忽略!");
            }
        }

        /// <summary>
        /// 堆场作业中
        /// </summary>
        /// <param name="status">堆场作业状态</param>
        public override void OnYardOperation(VehicleYardOperationStatus status)
        {
            if (status == VehicleYardOperationStatus.Standby)
                throw new InvalidOperationException($"{status}无意义被忽略!");

            switch (_status)
            {
                case VehicleShipmentOperationStatus.YardReceive1:
                    _yardReceive1 = status;
                    break;
                case VehicleShipmentOperationStatus.YardReceive2:
                    _yardReceive2 = status;
                    break;
                default:
                    throw new InvalidOperationException($"{status}不合时宜({_status})被忽略!");
            }
        }

        #endregion

        #endregion
    }
}