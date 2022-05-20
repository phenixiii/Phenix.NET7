using System;

namespace Phenix.Core.Data.Validation
{
    /// <summary>
    /// 数据验证消息
    /// </summary>
    [Serializable]
    public class ValidationMessage
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="statusCode">状态码(1000以下为保留值)</param>
        /// <param name="hint">提示</param>
        /// <param name="messageType">消息类型</param>
        [Newtonsoft.Json.JsonConstructor]
        public ValidationMessage(string key, int statusCode, string hint, ValidationMessageType messageType = ValidationMessageType.Error)
        {
            _key = key;
            _statusCode = statusCode;
            _hint = hint;
            _messageType = messageType;
        }

        #region 属性

        private readonly string _key;

        /// <summary>
        /// 键值
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        private readonly int _statusCode;

        /// <summary>
        /// 状态码(1000以下为保留值)
        /// </summary>
        public int StatusCode
        {
            get { return _statusCode; }
        }

        private readonly string _hint;

        /// <summary>
        /// 提示
        /// </summary>
        public string Hint
        {
            get { return _hint; }
        }

        private readonly ValidationMessageType _messageType;

        /// <summary>
        /// 消息类型
        /// </summary>
        public ValidationMessageType MessageType
        {
            get { return _messageType; }
        }

        #endregion
    }
}
