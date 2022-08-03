using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 吊车抓具动作事件
    /// </summary>
    [Serializable]
    public record CraneGrabActionEvent : MachineEvent
    {
        /// <summary>
        /// 吊车抓具动作事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="craneType">吊车类型</param>
        /// <param name="grabAction">抓具动作</param>
        /// <param name="hoistHeight">起升高度cm</param>
        [Newtonsoft.Json.JsonConstructor]
        public CraneGrabActionEvent(string machineId, MachineType machineType,
            CraneType craneType, CraneGrabAction grabAction, int hoistHeight)
            : base(machineId, machineType)
        {
            this.CraneType = craneType;
            this.GrabAction = grabAction;
            this.HoistHeight = hoistHeight;
        }

        #region 属性

        /// <summary>
        /// 吊车类型
        /// </summary>
        public CraneType CraneType { get; }

        /// <summary>
        /// 抓具动作
        /// </summary>
        public CraneGrabAction GrabAction { get; }

        /// <summary>
        /// 起升高度cm
        /// </summary>
        public int HoistHeight { get; }

        #endregion
    }
}