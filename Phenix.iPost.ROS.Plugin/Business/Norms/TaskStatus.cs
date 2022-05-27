using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 任务状态
    /// </summary>
    [Serializable]
    public enum TaskStatus
    {
        /// <summary>
        /// 无任务
        /// </summary>
        None = 0,

        /// <summary>
        /// 执行中
        /// </summary>
        Running = 1,

        /// <summary>
        /// 完成
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 拒绝
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// 取消
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// 中止
        /// </summary>
        Aborted = 5,
    }
}