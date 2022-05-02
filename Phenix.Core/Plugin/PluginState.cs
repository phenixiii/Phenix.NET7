using System;

namespace Phenix.Core.Plugin
{
    /// <summary>
    /// 插件状态
    /// </summary>
    [Serializable]
    public enum PluginState
    {
        /// <summary>
        /// 构建
        /// </summary>
        Created,

        /// <summary>
        /// 初始化
        /// </summary>
        Initialized,

        /// <summary>
        /// 终止化
        /// </summary>
        Finalizing,

        /// <summary>
        /// 启动
        /// </summary>
        Started,

        /// <summary>
        /// 停止
        /// </summary>
        Suspended
    }
}