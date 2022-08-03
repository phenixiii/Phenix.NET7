using System;
using System.Collections.Generic;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车作业
    /// </summary>
    [Serializable]
    public class VehicleOperation
    {
        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected internal VehicleOperation(string machineId, VehicleOperationType operationType, IList<VehicleTask> tasks = null)
        {
            _machineId = machineId;
            _operationType = operationType;
            _tasks = tasks ?? new List<VehicleTask>();
        }

        #region 属性

        private readonly string _machineId;

        /// <summary>
        /// 设备ID
        /// </summary>
        public string MachineId => _machineId;

        private readonly VehicleOperationType _operationType;

        /// <summary>
        /// 作业类型
        /// </summary>
        public VehicleOperationType OperationType => _operationType;

        private readonly IList<VehicleTask> _tasks;

        /// <summary>
        /// 拖车任务
        /// </summary>
        public IList<VehicleTask> Tasks => _tasks;

        /// <summary>
        /// 接货中
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool Receiving
        {
            get
            {
                VehicleTask activityCarryCargoTask = GetActivityCarryCargoTask();
                return activityCarryCargoTask == null || activityCarryCargoTask.TaskPurpose == VehicleTaskPurpose.Receive && activityCarryCargoTask.TaskStatus is not (TaskStatus.Completed or TaskStatus.Aborted);
            }
        }

        /// <summary>
        /// 待送货
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool Deliverable
        {
            get
            {
                VehicleTask activityCarryCargoTask = GetActivityCarryCargoTask();
                return activityCarryCargoTask != null && activityCarryCargoTask.TaskPurpose == VehicleTaskPurpose.Receive && activityCarryCargoTask.TaskStatus == TaskStatus.Completed;
            }
        }

        /// <summary>
        /// 送货中
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool Delivering
        {
            get
            {
                VehicleTask activityCarryCargoTask = GetActivityCarryCargoTask();
                return activityCarryCargoTask != null && activityCarryCargoTask.TaskPurpose == VehicleTaskPurpose.Deliver && activityCarryCargoTask.TaskStatus is not (TaskStatus.Completed or TaskStatus.Aborted);
            }
        }

        /// <summary>
        /// 作业中
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool InOperation => Receiving || Deliverable || Delivering;

        private readonly List<SpaceTimeInfo> _spaceTimes = new List<SpaceTimeInfo>();

        /// <summary>
        /// 时空轨迹
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IList<SpaceTimeInfo> SpaceTimes => _spaceTimes;

        #endregion

        #region 方法

        #region Task

        /// <summary>
        /// 活动中任务
        /// </summary>
        public VehicleTask GetActivityTask()
        {
            if (_tasks.Count > 0)
            {
                foreach (VehicleTask item in _tasks)
                    if (item.TaskStatus is not (TaskStatus.Completed or TaskStatus.Aborted))
                        return item;
                return _tasks[^1];
            }

            return null;
        }

        /// <summary>
        /// 活动中载货任务
        /// </summary>
        public VehicleTask GetActivityCarryCargoTask()
        {
            if (_tasks.Count > 0)
            {
                VehicleTask lastCarryCargoTask = null;
                foreach (VehicleTask item in _tasks)
                    if (item.CarryCargo != null)
                    {
                        if (item.TaskStatus is not (TaskStatus.Completed or TaskStatus.Aborted))
                            return item;
                        lastCarryCargoTask = item;
                    }

                return lastCarryCargoTask;
            }

            return null;
        }

        /// <summary>
        /// 新增任务
        /// </summary>
        /// <param name="destination">目的地</param>
        /// <param name="taskType">任务类型</param>
        /// <param name="taskPurpose">任务目的</param>
        /// <param name="carryCargo">载货</param>
        public VehicleTask NewTask(OperationLocationInfo destination, VehicleTaskType taskType, VehicleTaskPurpose taskPurpose, CarryCargoInfo? carryCargo = null)
        {
            VehicleTask result;
            switch (taskType)
            {
                case VehicleTaskType.Berth:
                    if (!carryCargo.HasValue)
                        throw new ArgumentNullException(nameof(carryCargo));
                    result = new VehicleBerthTask(_machineId, destination, taskPurpose, carryCargo.Value);
                    break;
                case VehicleTaskType.Yard:
                    if (!carryCargo.HasValue)
                        throw new ArgumentNullException(nameof(carryCargo));
                    result = new VehicleYardTask(_machineId, destination, taskPurpose, carryCargo.Value);
                    break;
                default:
                    result = new VehicleTask(_machineId, destination, taskType, taskPurpose);
                    break;
            }

            _tasks.Add(result);
            return result;
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        public bool CompleteTask()
        {
            VehicleTask activityTask = GetActivityTask();
            if (activityTask != null)
                return activityTask.Complete();
            return false;
        }

        /// <summary>
        /// 中止任务
        /// </summary>
        public bool AbortTask()
        {
            VehicleTask activityTask = GetActivityTask();
            if (activityTask != null)
                return activityTask.Abort();
            return false;
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        public bool PauseTask()
        {
            VehicleTask activityTask = GetActivityTask();
            if (activityTask != null)
                return activityTask.Pause();
            return false;
        }

        /// <summary>
        /// 发生故障
        /// </summary>
        public bool Trouble()
        {
            VehicleTask activityTask = GetActivityTask();
            if (activityTask != null)
                return activityTask.Trouble();
            return false;
        }

        #endregion

        #region Event

        /// <summary>
        /// 任务反馈
        /// </summary>
        /// <param name="taskStatus">任务状态</param>
        public void OnTaskAck(TaskStatus taskStatus)
        {
            switch (taskStatus)
            {
                case TaskStatus.Completed:
                    CompleteTask();
                    break;
                case TaskStatus.Aborted:
                    AbortTask();
                    break;
                case TaskStatus.Pausing:
                    PauseTask();
                    break;
                case TaskStatus.Troubling:
                    Trouble();
                    break;
            }
        }

        /// <summary>
        /// 活动中
        /// </summary>
        /// <param name="action">动作</param>
        public void OnActivity(VehicleBerthAction action)
        {
            VehicleBerthTask berthTask = GetActivityCarryCargoTask() as VehicleBerthTask;
            if (berthTask == null)
                throw new InvalidOperationException($"{MachineId}({OperationType})当前无泊位载货任务, 不应该有{action}动作!");

            berthTask.OnActivity(action);
        }

        /// <summary>
        /// 活动中
        /// </summary>
        /// <param name="action">动作</param>
        public void OnActivity(VehicleYardAction action)
        {
            VehicleYardTask yardTask = GetActivityCarryCargoTask() as VehicleYardTask;
            if (yardTask == null)
                throw new InvalidOperationException($"{MachineId}({OperationType})当前无堆场载货任务, 不应该有{action}动作!");

            yardTask.OnActivity(action);
        }

        /// <summary>
        /// 在移动
        /// </summary>
        /// <param name="spaceTime">时空</param>
        public void OnMoving(SpaceTimeInfo spaceTime)
        {
            _spaceTimes.Add(spaceTime);
        }

        #endregion

        #endregion
    }
}