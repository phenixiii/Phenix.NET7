using Phenix.Core.Data;
using Phenix.iPost.CSS.Plugin.Business.Norms;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车任务
    /// </summary>
    public class VehicleTask
    {
        internal VehicleTask(VehicleOperation owner, OperationLocationProperty destination,
            VehicleTaskType taskType, VehicleTaskPurpose taskPurpose, CarryCargoProperty? carryCargo = null)
        {
            _owner = owner;
            _prevTask = owner.Tasks.Count > 0 ? owner.Tasks[^1] : null;
            _destination = destination;
            _taskType = taskType;
            _taskPurpose = taskPurpose;
            _carryCargo = carryCargo;
        }

        #region 属性
        
        private readonly VehicleOperation _owner;

        /// <summary>
        /// 主人
        /// </summary>
        public VehicleOperation Owner
        {
            get { return _owner; }
        }

        private readonly VehicleTask _prevTask;

        /// <summary>
        /// 上个
        /// </summary>
        public VehicleTask PrevTask
        {
            get { return _prevTask; }
        }

        private readonly OperationLocationProperty _destination;

        /// <summary>
        /// 目的地
        /// </summary>
        public OperationLocationProperty Destination
        {
            get { return _destination; }
        }

        private readonly string _taskNo = Database.Default.Sequence.Value.ToString();

        /// <summary>
        /// 任务号
        /// </summary>
        public string TaskNo
        {
            get { return _taskNo; }
        }

        private VehicleTaskType _taskType;

        /// <summary>
        /// 任务类型
        /// </summary>
        public VehicleTaskType TaskType
        {
            get { return _taskType; }
        }

        private VehicleTaskPurpose _taskPurpose;

        /// <summary>
        /// 任务目的
        /// </summary>
        public VehicleTaskPurpose TaskPurpose
        {
            get { return _taskPurpose; }
        }

        private TaskStatus _taskStatus;

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus TaskStatus
        {
            get { return _taskStatus; }
        }

        private readonly CarryCargoProperty? _carryCargo;

        /// <summary>
        /// 载货
        /// </summary>
        public CarryCargoProperty? CarryCargo
        {
            get { return _carryCargo; }
        }

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