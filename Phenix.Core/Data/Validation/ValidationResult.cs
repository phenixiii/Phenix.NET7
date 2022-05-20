using System;
using System.Collections.Generic;

namespace Phenix.Core.Data.Validation
{
    /// <summary>
    /// 数据验证结果
    /// </summary>
    public class ValidationResult : System.ComponentModel.DataAnnotations.ValidationResult
    {
        /// <summary>
        /// 数据验证结果
        /// </summary>
        public ValidationResult(string errorMessage = null)
            : this(null, null, errorMessage)
        {
        }

        /// <summary>
        /// 数据验证结果
        /// </summary>
        public ValidationResult(IEnumerable<string> memberNames, object value, string description = null)
            : base(String.Format(AppSettings.GetValue("数据不符合要求{0}{1}"), description != null ? ": " : "!", description), memberNames)
        {
            _value = value;
        }

        /// <summary>
        /// 数据验证结果
        /// </summary>
        protected ValidationResult(ValidationResult validationResult)
            : base(validationResult)
        {
            _value = validationResult._value;
        }

        #region 属性

        private readonly object _value;

        /// <summary>
        /// 值
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        #endregion
    }
}