using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 机械状态
    /// </summary>
    [Serializable]
    public enum MachineStatus
    {
        /// <summary>
        /// 未在线
        /// 无法执行指令
        /// </summary>
        NotOnline,

        /// <summary>
        /// 被控中
        /// 无法执行指令
        /// </summary>
        Controlling,

        /// <summary>
        /// 维修中
        /// 无法执行指令
        /// </summary>
        Maintenance,

        /// <summary>
        /// 充电中/加油中
        /// 无法执行指令
        /// </summary>
        Fueling,

        /// <summary>
        /// 自动模式
        /// 可以执行指令不管是司机还是终端
        /// </summary>
        Automatic,
    }
}
