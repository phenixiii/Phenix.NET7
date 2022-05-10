using System;

namespace Phenix.Core.Event
{
    /// <summary>
    /// 事件包
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
        public virtual string EventName
        {
            get { return this.GetType().Name; }
        }

        #endregion
    }
}
