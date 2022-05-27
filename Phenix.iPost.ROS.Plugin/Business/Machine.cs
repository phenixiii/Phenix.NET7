using System;
using Phenix.iPost.ROS.Plugin.Business.Norms;

namespace Phenix.iPost.ROS.Plugin.Business
{
    /// <summary>
    /// 机械
    /// </summary>
    [Serializable]
    public abstract class Machine
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="machineId">机械ID</param>
        protected Machine(string machineId)
        {
            _machineId = machineId;
        }

        #region 属性

        private readonly string _machineId;

        /// <summary>
        /// 机械ID
        /// </summary>
        public string MachineId
        {
            get { return _machineId; }
        }

        private GridCellProperty _location;

        /// <summary>
        /// 位置
        /// </summary>
        public GridCellProperty Location
        {
            get { return _location; }
        }

        private MachineStatusProperty _status;

        /// <summary>
        /// 状态
        /// </summary>
        public MachineStatusProperty Status
        {
            get { return _status; }
        }

        private PowerProperty _power;

        /// <summary>
        /// 动力
        /// </summary>
        public PowerProperty Power
        {
            get { return _power; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 移动位置
        /// </summary>
        /// <param name="location">位置</param>
        public virtual void MoveInto(GridCellProperty location)
        {
            _location = location;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="status">状态</param>
        public virtual void ChangeStatus(MachineStatusProperty status)
        {
            _status = status;
        }

        /// <summary>
        /// 更新动力
        /// </summary>
        /// <param name="power">动力</param>
        public virtual void ChangePower(PowerProperty power)
        {
            _power = power;
        }

        #endregion
    }
}
