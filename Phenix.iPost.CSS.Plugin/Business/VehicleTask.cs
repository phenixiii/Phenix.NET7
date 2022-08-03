using System;
using Phenix.Core.Data;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车任务
    /// </summary>
    [Serializable]
    public class VehicleTask
    {
        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected VehicleTask(string machineId, OperationLocationInfo destination,
            string taskNo, VehicleTaskType taskType, VehicleTaskPurpose taskPurpose, CarryCargoInfo? carryCargo)
        {
            _machineId = machineId;
            _destination = destination;
            _taskNo = taskNo;
            _taskType = taskType;
            _taskPurpose = taskPurpose;
            _carryCargo = carryCargo;
        }

        internal VehicleTask(string machineId, OperationLocationInfo destination,
            VehicleTaskType taskType, VehicleTaskPurpose taskPurpose, CarryCargoInfo? carryCargo = null)
            : this(machineId, destination, Database.Default.Sequence.Value.ToString(), taskType, taskPurpose, carryCargo)
        {
        }

        #region 属性

        private readonly string _machineId;

        /// <summary>
        /// 设备ID
        /// </summary>
        public string MachineId => _machineId;

        private readonly OperationLocationInfo _destination;

        /// <summary>
        /// 目的地
        /// </summary>
        public OperationLocationInfo Destination => _destination;

        private readonly string _taskNo;

        /// <summary>
        /// 任务号
        /// </summary>
        public string TaskNo => _taskNo;

        private readonly VehicleTaskType _taskType;

        /// <summary>
        /// 任务类型
        /// </summary>
        public VehicleTaskType TaskType => _taskType;

        private readonly VehicleTaskPurpose _taskPurpose;

        /// <summary>
        /// 任务目的
        /// </summary>
        public VehicleTaskPurpose TaskPurpose => _taskPurpose;

        private TaskStatus _taskStatus;

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus TaskStatus => _taskStatus;

        private CarryCargoInfo? _carryCargo;

        /// <summary>
        /// 载货
        /// </summary>
        public CarryCargoInfo? CarryCargo => _carryCargo;

        #endregion

        #region 方法

        #region Task

        /// <summary>
        /// 完成任务
        /// </summary>
        public bool Complete()
        {
            if (_taskStatus is TaskStatus.Completed or TaskStatus.Aborted)
                return false;

            _taskStatus = TaskStatus.Completed;
            return true;
        }

        /// <summary>
        /// 中止任务
        /// </summary>
        public bool Abort()
        {
            if (_taskStatus is TaskStatus.Completed or TaskStatus.Aborted)
                return false;

            _taskStatus = TaskStatus.Aborted;
            return true;
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        public bool Pause()
        {
            if (_taskStatus is TaskStatus.Completed or TaskStatus.Aborted)
                return false;

            _taskStatus = TaskStatus.Pausing;
            return true;
        }

        /// <summary>
        /// 发生故障
        /// </summary>
        public bool Trouble()
        {
            if (_taskStatus is TaskStatus.Completed or TaskStatus.Aborted)
                return false;

            _taskStatus = TaskStatus.Troubling;
            return true;
        }

        #endregion

        #endregion
    }
}