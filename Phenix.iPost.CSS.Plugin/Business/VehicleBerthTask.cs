using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车泊位任务
    /// </summary>
    public class VehicleBerthTask : VehicleTask
    {
        internal VehicleBerthTask(VehicleOperation owner, OperationLocationProperty destination, VehicleTaskPurpose taskPurpose, CarryCargoProperty carryCargo)
            : base(owner, destination, VehicleTaskType.Berth, taskPurpose, carryCargo)
        {
        }

        #region 属性

        private VehicleBerthAction _action;

        /// <summary>
        /// 动作
        /// </summary>
        public VehicleBerthAction Action
        {
            get { return _action; }
        }

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
                throw new InvalidOperationException($"拖车{Owner.Owner.MachineId}({TaskStatus})更新泊位动作{action}无意义被忽略!");

            _action = action;

            if (action == VehicleBerthAction.Leave)
                Complete();
        }

        #endregion

        #endregion
    }
}