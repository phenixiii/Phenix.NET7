using System;
using Phenix.iPost.ROS.Plugin.Business.Norms;

namespace Phenix.iPost.ROS.Plugin.Business
{
    /// <summary>
    /// 卸船作业
    /// </summary>
    [Serializable]
    public class VehicleDischargeOperation : VehicleOperation
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="owner">主人</param>
        public VehicleDischargeOperation(Vehicle owner)
            : base(owner)
        {
        }

        #region 属性

        private VehicleDischargeOperationStatus _status;

        /// <summary>
        /// 卸船作业状态
        /// </summary>
        public VehicleDischargeOperationStatus Status
        {
            get { return _status; }
        }

        private VehicleBerthOperationStatus _berthReceive1;

        /// <summary>
        /// 泊位收箱1
        /// </summary>
        public VehicleBerthOperationStatus BerthReceive1
        {
            get { return _berthReceive1; }
        }

        private VehicleBerthOperationStatus _berthReceive2;

        /// <summary>
        /// 泊位收箱2
        /// </summary>
        public VehicleBerthOperationStatus BerthReceive2
        {
            get { return _berthReceive2; }
        }

        private VehicleYardOperationStatus _yardDeliver1;

        /// <summary>
        /// 堆场送箱1
        /// </summary>
        public VehicleYardOperationStatus YardDeliver1
        {
            get { return _yardDeliver1; }
        }

        private VehicleYardOperationStatus _yardDeliver2;

        /// <summary>
        /// 堆场送箱2
        /// </summary>
        public VehicleYardOperationStatus YardDeliver2
        {
            get { return _yardDeliver2; }
        }

        /// <summary>
        /// 收箱中
        /// </summary>
        public override bool Receiving
        {
            get { return _status < VehicleDischargeOperationStatus.YardDeliver1 && !Deliverable; }
        }

        /// <summary>
        /// 待送箱
        /// </summary>
        public override bool Deliverable
        {
            get
            {
                if (_status is VehicleDischargeOperationStatus.BerthReceive1 or VehicleDischargeOperationStatus.BerthReceive2)
                {
                    if (ValidTask1And2)
                        return _berthReceive1 == VehicleBerthOperationStatus.Leave && _berthReceive2 == VehicleBerthOperationStatus.Leave;
                    if (ValidTask1Only)
                        return _berthReceive1 == VehicleBerthOperationStatus.Leave;
                    if (ValidTask2Only)
                        return _berthReceive2 == VehicleBerthOperationStatus.Leave;
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
                if (_status is VehicleDischargeOperationStatus.YardDeliver1 or VehicleDischargeOperationStatus.YardDeliver2)
                {
                    if (ValidTask1And2)
                        return _yardDeliver1 != VehicleYardOperationStatus.Leave || _yardDeliver2 != VehicleYardOperationStatus.Leave;
                    if (ValidTask1Only)
                        return _yardDeliver1 != VehicleYardOperationStatus.Leave;
                    if (ValidTask2Only)
                        return _yardDeliver2 != VehicleYardOperationStatus.Leave;
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
                    if (_berthReceive1 != VehicleBerthOperationStatus.Leave)
                        _status = VehicleDischargeOperationStatus.BerthReceive1;
                    else if (_berthReceive2 != VehicleBerthOperationStatus.Leave)
                        _status = VehicleDischargeOperationStatus.BerthReceive2;
                    else
                        result = false;
                }
                else if (ValidTask1Only)
                {
                    if (_berthReceive1 != VehicleBerthOperationStatus.Leave)
                        _status = VehicleDischargeOperationStatus.BerthReceive1;
                    else
                        result = false;
                }
                else if (ValidTask2Only)
                {
                    if (_berthReceive2 != VehicleBerthOperationStatus.Leave)
                        _status = VehicleDischargeOperationStatus.BerthReceive2;
                    else
                        result = false;
                }
            }
            else if (Deliverable || Delivering)
            {
                if (ValidTask1And2)
                {
                    if (_yardDeliver1 != VehicleYardOperationStatus.Leave)
                        _status = VehicleDischargeOperationStatus.YardDeliver1;
                    else if (_yardDeliver2 != VehicleYardOperationStatus.Leave)
                        _status = VehicleDischargeOperationStatus.YardDeliver2;
                    else
                        result = false;
                }
                else if (ValidTask1Only)
                {
                    if (_yardDeliver1 != VehicleYardOperationStatus.Leave)
                        _status = VehicleDischargeOperationStatus.YardDeliver1;
                    else
                        result = false;
                }
                else if (ValidTask2Only)
                {
                    if (_yardDeliver2 != VehicleYardOperationStatus.Leave)
                        _status = VehicleDischargeOperationStatus.YardDeliver2;
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
                case VehicleDischargeOperationStatus.BerthReceive1 when _berthReceive1 == VehicleBerthOperationStatus.Standby:
                    _berthReceive1 = VehicleBerthOperationStatus.Issued;
                    return true;
                case VehicleDischargeOperationStatus.BerthReceive2 when _berthReceive2 == VehicleBerthOperationStatus.Standby:
                    _berthReceive2 = VehicleBerthOperationStatus.Issued;
                    return true;
                case VehicleDischargeOperationStatus.YardDeliver1 when _yardDeliver1 == VehicleYardOperationStatus.Standby:
                    _yardDeliver1 = VehicleYardOperationStatus.Issued;
                    return true;
                case VehicleDischargeOperationStatus.YardDeliver2 when _yardDeliver2 == VehicleYardOperationStatus.Standby:
                    _yardDeliver2 = VehicleYardOperationStatus.Issued;
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
                case VehicleDischargeOperationStatus.BerthReceive1:
                    _berthReceive1 = status;
                    break;
                case VehicleDischargeOperationStatus.BerthReceive2:
                    _berthReceive2 = status;
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
                case VehicleDischargeOperationStatus.YardDeliver1:
                    _yardDeliver1 = status;
                    break;
                case VehicleDischargeOperationStatus.YardDeliver2:
                    _yardDeliver2 = status;
                    break;
                default:
                    throw new InvalidOperationException($"{status}不合时宜({_status})被忽略!");
            }
        }

        #endregion

        #endregion
    }
}