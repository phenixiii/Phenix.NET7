using System;
using System.ComponentModel.DataAnnotations;

namespace Phenix.Core.Data.Validation
{
    /// <summary>
    /// 可整除某数
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class DivisibleAttribute : ValidationAttribute
    {
        /// <summary>
        /// 可整除某数
        /// </summary>
        /// <param name="into">某数</param>
        public DivisibleAttribute(int into)
            : base(String.Format(AppSettings.GetValue("无法整除 {0}"), into))
        {
            _into = into;
        }

        #region 属性

        private readonly int _into;

        /// <summary>
        /// 某数
        /// </summary>
        public int Into
        {
            get { return _into; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 检查有效性
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public override bool IsValid(object value)
        {
            return value == null || _into % (int) value == 0;
        }

        #endregion
    }
}
