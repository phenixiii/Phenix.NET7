using System;

namespace Phenix.Core.Event
{
    /// <summary>
    /// 事件包
    /// </summary>
    public record IntegrationEvent
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        #region 属性

        /// <summary>
        /// ID
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime CreationDate { get; }
        
        #endregion
    }
}
