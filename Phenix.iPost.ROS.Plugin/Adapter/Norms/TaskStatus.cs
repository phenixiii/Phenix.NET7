using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 任务状态
    /// </summary>
    [Serializable]
    public enum TaskStatus
    {
        /// <summary>
        /// 执行中
        /// </summary>
        Running,

        /// <summary>
        /// 完成
        /// </summary>
        Completed,

        /// <summary>
        /// 拒绝
        /// </summary>
        Rejected,

        /// <summary>
        /// 取消
        /// </summary>
        Cancelled,

        /// <summary>
        /// 中止
        /// </summary>
        Aborted,
    }
}