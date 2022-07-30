using System;

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
        {
            Id = Guid.NewGuid().ToString();
            OccurredTime = DateTime.UtcNow;
        }

        #region 属性

        /// <summary>
        /// 与发布订阅topic的pubsubname保持一致
        /// </summary>
        public const string PubSubName = "pubsub";

        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime OccurredTime { get; }

        /// <summary>
        /// 事件类型
        /// 默认：类名
        /// </summary>
        public virtual string EventName => this.GetType().Name;

        #endregion
    }
}
