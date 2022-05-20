using System;
using System.ComponentModel.DataAnnotations;

namespace Phenix.Core.Data.Validation
{
    /// <summary>
    /// 能被某数整除
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class DivisibleByAttribute : ValidationAttribute
    {
        /// <summary>
        /// 能被某数整除
        /// </summary>
        /// <param name="by">某数</param>
        public DivisibleByAttribute(int by)
            : base(String.Format(AppSettings.GetValue("无法被 {0} 整除"), by))
        {
            _by = by;
        }

        #region 属性

        private readonly int _by;

        /// <summary>
        /// 某数
        /// </summary>
        public int By
        {
            get { return _by; }
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
            return value == null || (int) value % _by == 0;
        }

        #endregion
    }
}
