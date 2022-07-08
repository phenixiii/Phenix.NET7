using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车
    /// </summary>
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

        #region Operation

        /// <summary>
        /// 新增作业
        /// </summary>
        /// <param name="operationType">拖车作业类型</param>
        public VehicleOperation NewOperation(VehicleOperationType operationType)
        {
            if (_operation != null && _operation.InOperation)
                throw new InvalidOperationException($"{MachineId}({_operation.GetActivityTask().TaskStatus})当前无法新增作业需人工干预!");

            VehicleOperation result = new VehicleOperation(this, operationType);
            _operation = result;
            return result;
        }

        #endregion

        #region Event

        /// <summary>
        /// 任务反馈
        /// </summary>
        /// <param name="taskStatus">任务状态</param>
        public void OnTaskAck(TaskStatus taskStatus)
        {
            if (_operation != null)
                _operation.OnTaskAck(taskStatus);
        }

        /// <summary>
        /// 活动中
        /// </summary>
        /// <param name="action">动作</param>
        public void OnActivity(VehicleBerthAction action)
        {
            if (_operation != null)
                _operation.OnActivity( action);
        }

        /// <summary>
        /// 活动中
        /// </summary>
        /// <param name="action">动作</param>
        public void OnActivity(VehicleYardAction action)
        {
            if (_operation != null)
                _operation.OnActivity( action);
        }

        /// <summary>
        /// 在移动
        /// </summary>
        /// <param name="spaceTime">时空</param>
        public override void OnMoving(SpaceTimeProperty spaceTime)
        {
            base.OnMoving(spaceTime);

            if (_operation != null)
                _operation.OnMoving(spaceTime);
        }
        
        #endregion

        #endregion
    }
}