using System;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace Phenix.Core.Security.Auth
{
    /// <summary>
    /// 用户找不到异常
    /// </summary>
    [Serializable]
    public class UserNotFoundException : AuthenticationException
    {
        /// <summary>
        /// 用户找不到异常
        /// </summary>
        public UserNotFoundException(Exception innerException = null)
            : this(AppSettings.GetValue("您不是注册用户!"), innerException)
        {
        }

        /// <summary>
        /// 用户找不到异常
        /// </summary>
        public UserNotFoundException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected UserNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}
