namespace Phenix.Core.Plugin
{
    /// <summary>
    /// 插件接口
    /// </summary>
    public interface IPlugin
    {
        #region 属性
        
        /// <summary>
        /// 插件状态
        /// </summary>
        PluginState State { get; }
        
        #endregion

        #region 方法

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns>确定启动</returns>
        bool Start();

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns>确定停止</returns>
        bool Suspend();

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>按需返回</returns>
        object ReceiveMessage(object message);

        #endregion
    }
}