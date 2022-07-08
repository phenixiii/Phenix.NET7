using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车堆场任务
    /// </summary>
    public class VehicleYardTask : VehicleTask
    {
        internal VehicleYardTask(VehicleOperation owner, OperationLocationProperty destination, VehicleTaskPurpose taskPurpose, CarryCargoProperty carryCargo)
            : base(owner, destination, VehicleTaskType.Yard, taskPurpose, carryCargo)
        {
        }

        #region 属性

        private VehicleYardAction _action;

        /// <summary>
        /// 动作
        /// </summary>
        public VehicleYardAction Action
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
        public void OnActivity(VehicleYardAction action)
        {
            if (action == VehicleYardAction.Standby)
                throw new InvalidOperationException($"拖车{Owner.Owner.MachineId}({TaskStatus})更新堆场动作{action}无意义被忽略!");

            _action = action;

            if (action == VehicleYardAction.Leave)
                Complete();
        }

        #endregion

        #endregion
    }
}