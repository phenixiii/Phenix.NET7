using System;
using System.Collections.Generic;
using Phenix.Core.Data;
using Phenix.iPost.ROS.Plugin.Business.Norms;

namespace Phenix.iPost.ROS.Plugin.Business
{
    /// <summary>
    /// 拖车作业
    /// </summary>
    [Serializable]
    public abstract class VehicleOperation
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="owner">主人</param>
        protected VehicleOperation(Vehicle owner)
        {
            _owner = owner;
        }

        #region 属性

        [NonSerialized]
        private readonly Vehicle _owner;

        /// <summary>
        /// 主人
        /// </summary>
        public Vehicle Owner
        {
            get { return _owner; }
        }

        private readonly string _task1No = Database.Default.Sequence.Value.ToString();

        /// <summary>
        /// 任务1ID
        /// </summary>
        public string Task1No
        {
            get { return _task1No; }
        }

        private TaskStatus _task1Status;

        /// <summary>
        /// 任务1状态
        /// </summary>
        public TaskStatus Task1Status
        {
            get { return _task1Status == TaskStatus.None && _task1No != null ? TaskStatus.Running : _task1Status; }
        }

        private CarryContainerProperty? _task1PlanContainer;

        /// <summary>
        /// 任务1计划载箱
        /// </summary>
        public CarryContainerProperty? Task1PlanContainer
        {
            get { return _task1PlanContainer; }
        }

        private CarryContainerProperty? _task1CarryContainer;

        /// <summary>
        /// 任务1实际载箱
        /// </summary>
        public CarryContainerProperty? Task1CarryContainer
        {
            get { return _task1CarryContainer; }
        }

        private readonly string _task2No = Database.Default.Sequence.Value.ToString();

        /// <summary>
        /// 任务2ID
        /// </summary>
        public string Task2No
        {
            get { return _task2No; }
        }

        private TaskStatus _task2Status;

        /// <summary>
        /// 任务2状态
        /// </summary>
        public TaskStatus Task2Status
        {
            get { return _task2Status == TaskStatus.None && _task2No != null ? TaskStatus.Running : _task2Status; }
        }

        private CarryContainerProperty? _task2PlanContainer;

        /// <summary>
        /// 任务2计划载箱
        /// </summary>
        public CarryContainerProperty? Task2PlanContainer
        {
            get { return _task2PlanContainer; }
        }

        private CarryContainerProperty? _task2CarryContainer;

        /// <summary>
        /// 任务2实际载箱
        /// </summary>
        public CarryContainerProperty? Task2CarryContainer
        {
            get { return _task2CarryContainer; }
        }

        /// <summary>
        /// 收箱中
        /// </summary>
        public abstract bool Receiving { get; }

        private readonly string _receiveTask1No = Database.Default.Sequence.Value.ToString();

        /// <summary>
        /// 收箱任务1ID
        /// </summary>
        public string ReceiveTask1No
        {
            get { return _task1No == null ? null : _receiveTask1No; }
        }

        private DriveDestinationProperty? _receiveTask1Destination;

        /// <summary>
        /// 收箱任务1目的地
        /// </summary>
        public DriveDestinationProperty? ReceiveTask1Destination
        {
            get { return _receiveTask1Destination; }
        }

        private readonly string _receiveTask2No = Database.Default.Sequence.Value.ToString();

        /// <summary>
        /// 收箱任务2ID
        /// </summary>
        public string ReceiveTask2No
        {
            get { return _task2No == null ? null : _receiveTask2No; }
        }

        private DriveDestinationProperty? _receiveTask2Destination;

        /// <summary>
        /// 收箱任务2目的地
        /// </summary>
        public DriveDestinationProperty? ReceiveTask2Destination
        {
            get { return _receiveTask2Destination; }
        }

        /// <summary>
        /// 待送箱
        /// </summary>
        public abstract bool Deliverable { get; }

        /// <summary>
        /// 送箱中
        /// </summary>
        public abstract bool Delivering { get; }

        private string _deliverTask1No = Database.Default.Sequence.Value.ToString();

        /// <summary>
        /// 送箱任务1ID
        /// </summary>
        public string DeliverTask1No
        {
            get { return _task1No == null ? null : _deliverTask1No; }
        }

        private DriveDestinationProperty? _deliverTask1Destination;

        /// <summary>
        /// 送箱任务1目的地
        /// </summary>
        public DriveDestinationProperty? DeliverTask1Destination
        {
            get { return _deliverTask1Destination; }
        }

        private string _deliverTask2No = Database.Default.Sequence.Value.ToString();

        /// <summary>
        /// 送箱任务2ID
        /// </summary>
        public string DeliverTask2No
        {
            get { return _task2No == null ? null : _deliverTask2No; }
        }

        private DriveDestinationProperty? _deliverTask2Destination;

        /// <summary>
        /// 送箱任务2目的地
        /// </summary>
        public DriveDestinationProperty? DeliverTask2Destination
        {
            get { return _deliverTask2Destination; }
        }

        /// <summary>
        /// 执行中
        /// </summary>
        public bool TaskRunning
        {
            get { return Receiving || Deliverable || Delivering; }
        }

        /// <summary>
        /// 任务都有效
        /// </summary>
        public bool ValidTask1And2
        {
            get { return Task1Status == TaskStatus.Running && Task2Status == TaskStatus.Running; }
        }

        /// <summary>
        /// 仅任务1有效
        /// </summary>
        public bool ValidTask1Only
        {
            get { return Task1Status == TaskStatus.Running && Task2Status != TaskStatus.Running; }
        }

        /// <summary>
        /// 仅任务2有效
        /// </summary>
        public bool ValidTask2Only
        {
            get { return Task1Status != TaskStatus.Running && Task2Status == TaskStatus.Running; }
        }

        private List<SpaceTimeProperty> _spaceTimes = new List<SpaceTimeProperty>();

        /// <summary>
        /// 时空轨迹
        /// </summary>
        public IList<SpaceTimeProperty> SpaceTimes
        {
            get { return _spaceTimes; }
        }

        #endregion

        #region 方法

        #region Task

        /// <summary>
        /// 更新收箱任务
        /// </summary>
        /// <param name="planContainer">计划载箱</param>
        /// <param name="destination">目的地</param>
        public virtual void PutReceiveTask(CarryContainerProperty planContainer, DriveDestinationProperty destination)
        {
            if (_task1PlanContainer != null && _task1PlanContainer.Value.Position == planContainer.Position)
            {
                _task1PlanContainer = planContainer;
                _receiveTask1Destination = destination;
            }
            else if (_task2PlanContainer != null && _task2PlanContainer.Value.Position == planContainer.Position)
            {
                _task2PlanContainer = planContainer;
                _receiveTask2Destination = destination;
            }
            else if (_task1PlanContainer == null)
            {
                _task1PlanContainer = planContainer;
                _receiveTask1Destination = destination;
            }
            else if (_task2PlanContainer == null)
            {
                _task2PlanContainer = planContainer;
                _receiveTask2Destination = destination;
            }
            else
                throw new InvalidOperationException(@$"{_owner.MachineId}的收箱任务已满没法添加!
而且这些任务的载箱位置({_task1PlanContainer.Value.Position}/{_task2PlanContainer.Value.Position})都对应不上要更新进来的({planContainer.Position})!");
        }

        /// <summary>
        /// 更新送箱任务
        /// </summary>
        /// <param name="carryContainer">实际载箱</param>
        /// <param name="destination">目的地</param>
        public virtual void PutDeliverTask(CarryContainerProperty carryContainer, DriveDestinationProperty destination)
        {
            if (_task1CarryContainer != null && _task1CarryContainer.Value.Position == carryContainer.Position)
            {
                _task1CarryContainer = carryContainer;
                _deliverTask1Destination = destination;
            }
            else if (_task2CarryContainer != null && _task2CarryContainer.Value.Position == carryContainer.Position)
            {
                _task2CarryContainer = carryContainer;
                _deliverTask2Destination = destination;
            }
            else if (_task1CarryContainer == null)
            {
                _task1CarryContainer = carryContainer;
                _deliverTask1Destination = destination;
            }
            else if (_task2CarryContainer == null)
            {
                _task2CarryContainer = carryContainer;
                _deliverTask2Destination = destination;
            }
            else
                throw new InvalidOperationException(@$"{_owner.MachineId}的送箱任务已满没法添加!
而且这些任务的载箱位置({_task1CarryContainer.Value.Position}/{_task2CarryContainer.Value.Position})都对应不上要更新进来的({carryContainer.Position})!");
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="position">载箱位置</param>
        public virtual bool DeleteTask(CarryContainerPosition position)
        {
            bool result = true;

            if (_task1CarryContainer != null && _task1CarryContainer.Value.Position == position)
                _task1Status = TaskStatus.Aborted;
            else if (_task2CarryContainer != null && _task2CarryContainer.Value.Position == position)
                _task2Status = TaskStatus.Aborted;
            else if (_task1PlanContainer != null && _task1PlanContainer.Value.Position == position)
                _task1Status = TaskStatus.Cancelled;
            else if (_task2PlanContainer != null && _task2PlanContainer.Value.Position == position)
                _task2Status = TaskStatus.Cancelled;
            else
                result = false;

            return result;
        }

        /// <summary>
        /// 下一任务
        /// </summary>
        public abstract bool NextTask();

        /// <summary>
        /// 启动任务
        /// </summary>
        public abstract bool RunTask();

        #endregion

        #region Event

        /// <summary>
        /// 在移动
        /// </summary>
        public virtual void OnMoving(SpaceTimeProperty spaceTime)
        {
            _spaceTimes.Add(spaceTime);
        }

        /// <summary>
        /// 泊位作业中
        /// </summary>
        /// <param name="status">泊位作业状态</param>
        public abstract void OnBerthOperation(VehicleBerthOperationStatus status);

        /// <summary>
        /// 堆场作业中
        /// </summary>
        /// <param name="status">堆场作业状态</param>
        public abstract void OnYardOperation(VehicleYardOperationStatus status);

        #endregion

        #endregion
    }
}