using System;
using Phenix.iPost.ROS.Plugin.Business.Norms;

namespace Phenix.iPost.ROS.Plugin.Business
{
    /// <summary>
    /// 转堆作业
    /// </summary>
    [Serializable]
    public class VehicleShiftOperation : VehicleOperation
    {
        /// <summary>
        /// 转堆作业
        /// </summary>
        /// <param name="owner">主人</param>
        public VehicleShiftOperation(Vehicle owner)
            : base(owner)
        {
        }

        #region 属性

        private VehicleShiftOperationStatus _status;

        /// <summary>
        /// 转堆作业状态
        /// </summary>
        public VehicleShiftOperationStatus Status
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
            get { return _status < VehicleShiftOperationStatus.YardDeliver1 && !Deliverable; }
        }

        /// <summary>
        /// 待送箱
        /// </summary>
        public override bool Deliverable
        {
            get
            {
                if (_status is VehicleShiftOperationStatus.YardReceive1 or VehicleShiftOperationStatus.YardReceive2)
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
                if (_status is VehicleShiftOperationStatus.YardDeliver1 or VehicleShiftOperationStatus.YardDeliver2)
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
                    if (_yardReceive1 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShiftOperationStatus.YardReceive1;
                    else if (_yardReceive2 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShiftOperationStatus.YardReceive2;
                    else
                        result = false;
                }
                else if (ValidTask1Only)
                {
                    if (_yardReceive1 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShiftOperationStatus.YardReceive1;
                    else
                        result = false;
                }
                else if (ValidTask2Only)
                {
                    if (_yardReceive2 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShiftOperationStatus.YardReceive2;
                    else
                        result = false;
                }
            }
            else if (Deliverable || Delivering)
            {
                if (ValidTask1And2)
                {
                    if (_yardDeliver1 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShiftOperationStatus.YardDeliver1;
                    else if (_yardDeliver2 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShiftOperationStatus.YardDeliver2;
                    else
                        result = false;
                }
                else if (ValidTask1Only)
                {
                    if (_yardDeliver1 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShiftOperationStatus.YardDeliver1;
                    else
                        result = false;
                }
                else if (ValidTask2Only)
                {
                    if (_yardDeliver2 != VehicleYardOperationStatus.Leave)
                        _status = VehicleShiftOperationStatus.YardDeliver2;
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
                case VehicleShiftOperationStatus.YardReceive1 when _yardReceive1 == VehicleYardOperationStatus.Standby:
                    _yardReceive1 = VehicleYardOperationStatus.Issued;
                    return true;
                case VehicleShiftOperationStatus.YardReceive2 when _yardReceive2 == VehicleYardOperationStatus.Standby:
                    _yardReceive2 = VehicleYardOperationStatus.Issued;
                    return true;
                case VehicleShiftOperationStatus.YardDeliver1 when _yardDeliver1 == VehicleYardOperationStatus.Standby:
                    _yardDeliver1 = VehicleYardOperationStatus.Issued;
                    return true;
                case VehicleShiftOperationStatus.YardDeliver2 when _yardDeliver2 == VehicleYardOperationStatus.Standby:
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
            throw new InvalidOperationException($"{status}无意义被忽略!");
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
                case VehicleShiftOperationStatus.YardReceive1:
                    _yardReceive1 = status;
                    break;
                case VehicleShiftOperationStatus.YardReceive2:
                    _yardReceive2 = status;
                    break;
                case VehicleShiftOperationStatus.YardDeliver1:
                    _yardDeliver1 = status;
                    break;
                case VehicleShiftOperationStatus.YardDeliver2:
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