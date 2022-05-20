namespace Phenix.Core.Data.Validation
{
    /// <summary>
    /// 数据验证消息类型
    /// </summary>
    public enum ValidationMessageType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// 错误
        /// </summary>
        Error = 1,

        /// <summary>
        /// 终止
        /// </summary>
        Stop = 2,

        /// <summary>
        /// 问题
        /// </summary>
        Question = 3,

        /// <summary>
        /// 提醒
        /// </summary>
        Exclamation = 4,

        /// <summary>
        /// 警告
        /// </summary>
        Warning = 5,

        /// <summary>
        /// 脚注
        /// </summary>
        Asterisk = 6,

        /// <summary>
        /// 消息
        /// </summary>
        Information = 7,
    }
}
