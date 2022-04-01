using System;
using System.Runtime.Serialization;

namespace Phenix.Actor
{
    /// <summary>
    /// 数据流延迟异常
    /// </summary>
    [Serializable]
    public class StreamDelayException : InvalidOperationException
    {
        /// <summary>
        /// 数据流延迟异常
        /// </summary>
        public StreamDelayException(Exception innerException = null)
            : this("数据流延迟需做补偿", innerException)
        {
        }

        /// <summary>
        /// 数据流延迟异常
        /// </summary>
        public StreamDelayException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected StreamDelayException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}