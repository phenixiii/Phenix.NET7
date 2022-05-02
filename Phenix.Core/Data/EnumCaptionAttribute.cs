using System;

namespace Phenix.Core.Data
{
    /// <summary>
    /// 枚举字段标签
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumCaptionAttribute : Attribute
    {
        [Newtonsoft.Json.JsonConstructor]
        private EnumCaptionAttribute(string caption, string key, string tag)
            : this(caption)
        {
            _key = key;
            _tag = tag;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="caption">标签(中英文用‘|’分隔)</param>
        public EnumCaptionAttribute(string caption)
            : base()
        {
            _caption = caption;
        }

        #region 属性

        private readonly string _caption;

        /// <summary>
        /// 标签(中英文用‘|’分隔)
        /// Thread.CurrentThread.CurrentCulture.Name为非'zh-'时返回后半截
        /// </summary>
        public string Caption
        {
            get { return AppRun.SplitCulture(_caption); }
        }

        private string _key;

        /// <summary>
        /// 键
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        private string _tag;

        /// <summary>
        /// 标记
        /// </summary>
        public string Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        #endregion
    }
}