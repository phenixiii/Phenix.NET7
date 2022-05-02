using System;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace Phenix.Core.Security.Auth
{
    /// <summary>
    /// 口令复杂度验证异常
    /// </summary>
    [Serializable]
    public class PasswordComplexityException : AuthenticationException
    {
        /// <summary>
        /// 口令复杂度验证异常
        /// </summary>
        public PasswordComplexityException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 口令复杂度验证异常
        /// </summary>
        public PasswordComplexityException(int lengthMinimize, int complexityMinimize)
            : base(String.Format(AppSettings.GetValue("口令过于简单, 长度需大于等于{0}个字符且至少包含数字、大小写字母、特殊字符之{1}种"), lengthMinimize, complexityMinimize))
        {
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected PasswordComplexityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}