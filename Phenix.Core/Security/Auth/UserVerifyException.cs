using System;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace Phenix.Core.Security.Auth
{
    /// <summary>
    /// 用户身份验证异常
    /// </summary>
    [Serializable]
    public class UserVerifyException : AuthenticationException
    {
        /// <summary>
        /// 用户身份验证异常
        /// </summary>
        public UserVerifyException(Exception innerException = null)
            : this(AppSettings.GetValue("未通过身份验证, 请重新登录!"), innerException)
        {
        }

        /// <summary>
        /// 用户身份验证异常
        /// </summary>
        public UserVerifyException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected UserVerifyException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}