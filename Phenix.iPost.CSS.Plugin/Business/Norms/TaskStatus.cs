using System;

namespace Phenix.iPost.CSS.Plugin.Business.Norms
{
    /// <summary>
    /// 任务状态
    /// </summary>
    [Serializable]
    public enum TaskStatus
    {
        /// <summary>
        /// 计划中
        /// </summary>
        Planning,

        /// <summary>
        /// 已取消
        /// </summary>
        Canceled,

        /// <summary>
        /// 已启动
        /// </summary>
        Activated,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 已中止
        /// </summary>
        Aborted,

        /// <summary>
        /// 暂停中
        /// </summary>
        Pausing,

        /// <summary>
        /// 故障中
        /// </summary>
        Troubling,
    }
}