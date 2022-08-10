using System;
using Phenix.Core.Data;

namespace Phenix.Core.Event
{
    /// <summary>
    /// 事件
    /// </summary>
    [Serializable]
    public record IntegrationEvent
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public IntegrationEvent()
            : this(DateTime.UtcNow)
        {
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected IntegrationEvent(DateTime occurredTime, string eventId = null, string eventName = null)
        {
            this.OccurredTime = occurredTime;
            this.EventId = eventId ?? Database.Default.Sequence.Value.ToString();
            this.EventName = eventName ?? this.GetType().Name;
        }

        #region 属性

        /// <summary>
        /// 与发布订阅topic的pubsubname保持一致
        /// </summary>
        public const string PubSubName = "pubsub";

        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime OccurredTime { get; }

        /// <summary>
        /// 事件ID
        /// 默认：Database.Default.Sequence.Value
        /// </summary>
        public string EventId { get; }

        /// <summary>
        /// 事件类型
        /// 默认：类名
        /// </summary>
        public string EventName { get; }

        #endregion
    }
}