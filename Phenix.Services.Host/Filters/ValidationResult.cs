using System;
using Phenix.Core.Data.Validation;

namespace Phenix.Services.Host.Filters
{
    /// <summary>
    /// 数据验证结果
    /// </summary>
    [Serializable]
    public class ValidationResult
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        public ValidationResult(int code, ValidationMessage[] errors)
        {
            _code = code;
            _errors = errors;
        }

        #region 属性

        private readonly int _code;

        /// <summary>
        /// 状态码(见StatusCodes)
        /// </summary>
        public int Code
        {
            get { return _code; }
        }

        private readonly ValidationMessage[] _errors;

        /// <summary>
        /// 错误消息队列
        /// </summary>
        public ValidationMessage[] Errors
        {
            get { return _errors; }
        }

        #endregion
    }
}
