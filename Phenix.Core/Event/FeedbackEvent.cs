using System;

namespace Phenix.Core.Event
{
    /// <summary>
    /// 反馈事件
    /// </summary>
    [Serializable]
    public record FeedbackEvent : IntegrationEvent
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ackEvent">应答事件</param>
        /// <param name="error">处理异常</param>
        public FeedbackEvent(IntegrationEvent ackEvent, Exception error)
            : this(ackEvent, EventLevel.Error, error.Message)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ackEvent">应答事件</param>
        /// <param name="eventLevel">事件级别</param>
        /// <param name="eventMessage">事件消息</param>
        public FeedbackEvent(IntegrationEvent ackEvent, EventLevel eventLevel, string eventMessage)
            : this(ackEvent.EventId, ackEvent.EventName, eventLevel, eventMessage)
        {
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected FeedbackEvent(string ackEventId, string ackEventName, EventLevel eventLevel, string eventMessage)
            : base()
        {
            this.AckEventId = ackEventId;
            this.AckEventName = ackEventName;
            this.EventLevel = eventLevel;
            this.EventMessage = eventMessage;
        }

        #region 属性

        /// <summary>
        /// 应答事件ID
        /// </summary>
        public string AckEventId { get; }

        /// <summary>
        /// 应答事件类型
        /// </summary>
        public string AckEventName { get; }

        /// <summary>
        /// 事件级别
        /// </summary>
        private EventLevel EventLevel { get; }

        /// <summary>
        /// 事件消息
        /// </summary>
        public string EventMessage { get; }

        #endregion
    }
}