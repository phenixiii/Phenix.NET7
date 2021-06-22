using System;
using Phenix.Core.Data;

namespace Phenix.Services.Business.Message
{
    /// <summary>
    /// 用户消息资料
    /// </summary>
    [Serializable]
    public class UserMessageInfo
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="sender">发送用户</param>
        /// <param name="receiver">接收用户</param>
        /// <param name="content">消息内容</param>
        public UserMessageInfo(long id, string sender, string receiver, string content)
        {
            _id = id != 0 ? id : Database.Default.Sequence.Value;
            _createTime = DateTime.Now;
            _sender = sender;
            _receiver = receiver;
            _content = content;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="createTime">创建时间</param>
        /// <param name="sendTime">发送时间</param>
        /// <param name="receivedTime">收到时间</param>
        /// <param name="sender">发送用户</param>
        /// <param name="receiver">接收用户</param>
        /// <param name="content">消息内容</param>
        [Newtonsoft.Json.JsonConstructor]
        public UserMessageInfo(long id, DateTime createTime, DateTime? sendTime, DateTime? receivedTime, string sender, string receiver, string content)
            : this(id, sender, receiver, content)
        {
            _createTime = createTime;
            _sendTime = sendTime;
            _receivedTime = receivedTime;
        }

        #region 属性

        private readonly long _id;

        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private DateTime _createTime;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return _createTime; }
        }

        private DateTime? _sendTime;

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime? SendTime
        {
            get { return _sendTime; }
        }

        private DateTime? _receivedTime;

        /// <summary>
        /// 收到时间
        /// </summary>
        public DateTime? ReceivedTime
        {
            get { return _receivedTime; }
        }

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

        private string _content;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content
        {
            get { return _content; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 重新开始
        /// </summary>
        /// <param name="content">消息内容</param>
        public void Renew(string content)
        {
            _createTime = DateTime.Now;
            _sendTime = null;
            _receivedTime = null;
            _content = content;
        }

        #endregion
    }
}