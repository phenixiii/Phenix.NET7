using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 机械
    /// </summary>
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

        private MachineStatusProperty _status;

        /// <summary>
        /// 状态
        /// </summary>
        public MachineStatusProperty Status
        {
            get { return _status; }
        }

        private GridCellProperty _location;

        /// <summary>
        /// 位置
        /// </summary>
        public GridCellProperty Location
        {
            get { return _location; }
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

        #region Event

        /// <summary>
        /// 状态变化
        /// </summary>
        /// <param name="status">状态</param>
        public virtual void OnChangeStatus(MachineStatusProperty status)
        {
            _status = status;
        }

        /// <summary>
        /// 在移动
        /// </summary>
        /// <param name="spaceTime">时空</param>
        public virtual void OnMoving(SpaceTimeProperty spaceTime)
        {
            _location = new GridCellProperty(spaceTime.X, spaceTime.Y);
        }

        /// <summary>
        /// 动力变化
        /// </summary>
        /// <param name="power">动力</param>
        public virtual void OnChangePower(PowerProperty power)
        {
            _power = power;
        }

        #endregion

        #endregion
    }
}
