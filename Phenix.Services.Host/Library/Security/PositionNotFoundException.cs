using System;
using System.Runtime.Serialization;

namespace Phenix.Services.Host.Library.Security
{
    /// <summary>
    /// 岗位资料找不到异常
    /// </summary>
    [Serializable]
    public class PositionNotFoundException : System.ComponentModel.DataAnnotations.ValidationException
    {
        /// <summary>
        /// 岗位资料找不到异常
        /// </summary>
        public PositionNotFoundException(Exception innerException = null)
            : this("岗位资料不存在!", innerException)
        {
        }

        /// <summary>
        /// 岗位资料找不到异常
        /// </summary>
        public PositionNotFoundException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected PositionNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}
