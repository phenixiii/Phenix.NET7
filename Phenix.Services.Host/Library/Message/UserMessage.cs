using System;

namespace Phenix.Services.Host.Library.Message
{
    /// <summary>
    /// 用户消息
    /// </summary>
    [Serializable]
    public class UserMessage
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sender">发送用户</param>
        /// <param name="receiver">接收用户</param>
        /// <param name="content">消息内容</param>
        public UserMessage(string sender, string receiver, string content)
        {
            _sender = sender;
            _receiver = receiver;
            _content = content;
            _createTime = DateTime.Now;
        }

        #region 属性
        
        private readonly string _sender;

        /// <summary>
        /// 发送用户
        /// </summary>
        public string Sender
        {
            get { return _sender; }
        }

        private readonly string _receiver;

        /// <summary>
        /// 接收用户
        /// </summary>
        public string Receiver
        {
            get { return _receiver; }
        }

        private readonly string _content;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content
        {
            get { return _content; }
        }

        private readonly DateTime _createTime;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return _createTime; }
        }
        
        private DateTime _receivedTime;

        /// <summary>
        /// 收到时间
        /// </summary>
        public DateTime ReceivedTime
        {
            get { return _receivedTime; }
            set { _receivedTime = value; }
        }

        #endregion
    }
}