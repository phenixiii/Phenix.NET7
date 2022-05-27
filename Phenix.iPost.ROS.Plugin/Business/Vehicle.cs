using System;
using Phenix.iPost.ROS.Plugin.Business.Norms;

namespace Phenix.iPost.ROS.Plugin.Business
{
    /// <summary>
    /// 拖车
    /// </summary>
    [Serializable]
    public class Vehicle : Machine
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="machineId">机械ID</param>
        public Vehicle(string machineId)
            : base(machineId)
        {
        }

        #region 属性

        private VehicleOperation _operation;

        /// <summary>
        /// 作业
        /// </summary>
        public VehicleOperation Operation
        {
            get { return _operation; }
        }

        #endregion

        #region 方法

        #region Task

        /// <summary>
        /// 更新收箱任务
        /// </summary>
        /// <param name="taskType">任务类型</param>
        /// <param name="planContainer">计划载箱</param>
        /// <param name="destination">目的地</param>
        public void PutReceiveTask(VehicleTaskType taskType, CarryContainerProperty planContainer, DriveDestinationProperty destination)
        {
            if (_operation == null || !_operation.TaskRunning)
            {
                switch (taskType)
                {
                    case VehicleTaskType.DischargeOperation:
                        _operation = new VehicleDischargeOperation(this);
                        break;
                    case VehicleTaskType.ShipmentOperation:
                        _operation = new VehicleShipmentOperation(this);
                        break;
                    case VehicleTaskType.ShiftOperation:
                        _operation = new VehicleShiftOperation(this);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (_operation.GetType() == typeof(VehicleDischargeOperation) && taskType != VehicleTaskType.DischargeOperation ||
                     _operation.GetType() == typeof(VehicleShipmentOperation) && taskType != VehicleTaskType.ShipmentOperation ||
                     _operation.GetType() == typeof(VehicleShiftOperation) && taskType != VehicleTaskType.ShiftOperation)
                throw new InvalidOperationException($"{MachineId}({_operation.GetType().Name})更新收箱任务{taskType}({planContainer},{destination})错乱需人工干预!");

            _operation.PutReceiveTask(planContainer, destination);
        }

        /// <summary>
        /// 更新送箱任务
        /// </summary>
        /// <param name="taskType">任务类型</param>
        /// <param name="carryContainer">实际载箱</param>
        /// <param name="destination">目的地</param>
        public void PutDeliverTask(VehicleTaskType taskType, CarryContainerProperty carryContainer, DriveDestinationProperty destination)
        {
            if (_operation == null)
                throw new InvalidOperationException($"{MachineId}需先有收箱任务才能更新送箱任务{taskType}({carryContainer},{destination})!");

            if (_operation.GetType() == typeof(VehicleDischargeOperation) && taskType != VehicleTaskType.DischargeOperation ||
                _operation.GetType() == typeof(VehicleShipmentOperation) && taskType != VehicleTaskType.ShipmentOperation ||
                _operation.GetType() == typeof(VehicleShiftOperation) && taskType != VehicleTaskType.ShiftOperation)
                throw new InvalidOperationException($"{MachineId}({_operation.GetType().Name})更新送箱任务{taskType}({carryContainer},{destination})错乱需人工干预!");

            _operation.PutDeliverTask(carryContainer, destination);
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="position">载箱位置</param>
        public bool DeleteTask(CarryContainerPosition position)
        {
            return _operation != null && _operation.DeleteTask(position);
        }

        /// <summary>
        /// 下一任务
        /// </summary>
        public bool NextTask()
        {
            return _operation != null && _operation.NextTask();
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        public bool RunTask()
        {
            return _operation != null && _operation.RunTask();
        }

        #endregion

        #region Event

        /// <summary>
        /// 在移动
        /// </summary>
        public void OnMoving(SpaceTimeProperty spaceTime)
        {
            if (_operation != null)
                _operation.OnMoving(spaceTime);
            MoveInto(new GridCellProperty(spaceTime.X, spaceTime.Y));
        }

        /// <summary>
        /// 泊位作业中
        /// </summary>
        /// <param name="status">泊位作业状态</param>
        public void OnBerthOperation(VehicleBerthOperationStatus status)
        {
            if (_operation == null)
                throw new InvalidOperationException($"{MachineId}需先有任务才能有泊位作业({status})!");

            _operation.OnBerthOperation(status);
        }

        /// <summary>
        /// 堆场作业中
        /// </summary>
        /// <param name="status">堆场作业状态</param>
        public void OnYardOperation(VehicleYardOperationStatus status)
        {
            if (_operation == null)
                throw new InvalidOperationException($"{MachineId}需先有任务才能有堆场作业({status})!");

            _operation.OnYardOperation(status);
        }

        #endregion

        #endregion
    }
}