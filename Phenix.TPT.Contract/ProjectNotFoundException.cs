using System;
using System.Runtime.Serialization;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 项目资料找不到异常
    /// </summary>
    [Serializable]
    public class ProjectNotFoundException : System.ComponentModel.DataAnnotations.ValidationException
    {
        /// <summary>
        /// 项目资料找不到异常
        /// </summary>
        public ProjectNotFoundException(Exception innerException = null)
            : this("项目资料不存在!", innerException)
        {
        }

        /// <summary>
        /// 项目资料找不到异常
        /// </summary>
        public ProjectNotFoundException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected ProjectNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}
