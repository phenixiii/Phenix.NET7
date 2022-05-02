using System;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace Phenix.Core.Security.Auth
{
    /// <summary>
    /// 用户账号锁定异常
    /// </summary>
    [Serializable]
    public class UserLockedException : AuthenticationException
    {
        /// <summary>
        /// 用户账号锁定异常
        /// </summary>
        public UserLockedException(int lockedMinutes)
            : base(String.Format(AppSettings.GetValue("您的账号被锁定, {0}分钟之后请再尝试登录!"), lockedMinutes))
        {
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected UserLockedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}