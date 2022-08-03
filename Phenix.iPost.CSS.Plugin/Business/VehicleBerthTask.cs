using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车泊位任务
    /// </summary>
    [Serializable]
    public class VehicleBerthTask : VehicleTask
    {
        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected VehicleBerthTask(string machineId, OperationLocationInfo destination,
            string taskNo, VehicleTaskType taskType, VehicleTaskPurpose taskPurpose, CarryCargoInfo? carryCargo,
            VehicleBerthAction action)
            : base(machineId, destination, taskNo, taskType, taskPurpose, carryCargo)
        {
            _action = action;
        }

        internal VehicleBerthTask(string machineId, OperationLocationInfo destination,
            VehicleTaskPurpose taskPurpose, CarryCargoInfo carryCargo)
            : base(machineId, destination, VehicleTaskType.Berth, taskPurpose, carryCargo)
        {
        }

        #region 属性

        private VehicleBerthAction _action;

        /// <summary>
        /// 动作
        /// </summary>
        public VehicleBerthAction Action => _action;

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 活动中
        /// </summary>
        /// <param name="action">动作</param>
        public void OnActivity(VehicleBerthAction action)
        {
            if (action == VehicleBerthAction.Standby)
                throw new InvalidOperationException($"拖车{MachineId}({TaskStatus})更新泊位动作{action}无意义被忽略!");

            _action = action;

            if (action == VehicleBerthAction.Leave)
                Complete();
        }

        #endregion

        #endregion
    }
}