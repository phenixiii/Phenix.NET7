﻿using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车堆场任务
    /// </summary>
    [Serializable]
    public class VehicleYardTask : VehicleTask
    {
        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected VehicleYardTask(string machineId, OperationLocationInfo destination,
            string taskNo, VehicleTaskType taskType, VehicleTaskPurpose taskPurpose, CarryCargoInfo? carryCargo,
            VehicleYardAction action)
            : base(machineId, destination, taskNo, taskType, taskPurpose, carryCargo)
        {
            _action = action;
        }

        internal VehicleYardTask(string machineId, OperationLocationInfo destination, VehicleTaskPurpose taskPurpose, CarryCargoInfo carryCargo)
            :base(machineId, destination, VehicleTaskType.Yard, taskPurpose, carryCargo)
        {
        }

        #region 属性

        private VehicleYardAction _action;

        /// <summary>
        /// 动作
        /// </summary>
        public VehicleYardAction Action => _action;

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 活动中
        /// </summary>
        /// <param name="action">动作</param>
        public void OnActivity(VehicleYardAction action)
        {
            if (action == VehicleYardAction.Standby)
                throw new InvalidOperationException($"拖车{MachineId}({TaskStatus})更新堆场动作{action}无意义被忽略!");

            _action = action;

            if (action == VehicleYardAction.Leave)
                Complete();
        }

        #endregion

        #endregion
    }
}