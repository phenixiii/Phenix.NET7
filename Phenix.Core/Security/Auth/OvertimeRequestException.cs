using System;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace Phenix.Core.Security.Auth
{
    /// <summary>
    /// 服务请求超时异常
    /// </summary>
    [Serializable]
    public class OvertimeRequestException : AuthenticationException
    {
        /// <summary>
        /// 服务请求超时异常
        /// </summary>
        public OvertimeRequestException(double timestamp)
            : base(String.Format(AppSettings.GetValue("客户端时钟与服务端相差{0}分钟, 请校准!"), timestamp))
        {
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected OvertimeRequestException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}