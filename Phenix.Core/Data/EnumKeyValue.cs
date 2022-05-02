using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Phenix.Core.Reflection;
using Phenix.Core.SyncCollections;

namespace Phenix.Core.Data
{
    /// <summary>
    /// 枚举键值
    /// </summary>
    [Serializable]
    public sealed class EnumKeyValue : IComparable, IComparable<EnumKeyValue>
    {
        private EnumKeyValue(Enum value, EnumCaptionAttribute enumCaption)
            : this(value,
                enumCaption != null && enumCaption.Key != null ? enumCaption.Key : value.ToString("d"),
                enumCaption != null && enumCaption.Caption != null ? enumCaption.Caption : value.ToString("g"),
                enumCaption != null ? enumCaption.Tag : null)
        {
        }

        [Newtonsoft.Json.JsonConstructor]
        private EnumKeyValue(Enum value, string key, string caption, string tag)
        {
            _value = value;
            _key = key;
            _caption = caption;
            _tag = tag;
        }

        #region 工厂

        private static readonly SynchronizedDictionary<string, IList<EnumKeyValue>> _cache =
            new SynchronizedDictionary<string, IList<EnumKeyValue>>(StringComparer.Ordinal);

        /// <summary>
        /// 根据枚举类型定义构建
        /// </summary>
        public static IList<EnumKeyValue> Fetch<TEnum>()
            where TEnum : struct
        {
            return Fetch(typeof(TEnum));
        }

        /// <summary>
        /// 根据枚举类型定义构建
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        private static IList<EnumKeyValue> Fetch(Type enumType)
        {
            enumType = Utilities.LoadType(enumType); //主要用于IDE环境、typeof(T)

            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));
            if (!enumType.IsEnum)
                throw new ArgumentException(String.Format("类 {0} 应定义为枚举", enumType.FullName), nameof(enumType));

            return _cache.GetValue(enumType.FullName, () =>
            {
                FieldInfo[] fieldInfos = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
                List<EnumKeyValue> value = new List<EnumKeyValue>(fieldInfos.Length);
                foreach (FieldInfo item in fieldInfos)
                    value.Add(new EnumKeyValue((Enum) item.GetValue(enumType), (EnumCaptionAttribute) Attribute.GetCustomAttribute(item, typeof(EnumCaptionAttribute))));
                return value;
            });
        }

        /// <summary>
        /// 根据枚举值构建填充
        /// </summary>
        public static EnumKeyValue Fetch(Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            EnumKeyValue result = Fetch(value.GetType()).FirstOrDefault(p => Equals(p.Value, value));
            if (result != null)
                return result;
            throw new InvalidOperationException(String.Format("未发现 {0} 对应的枚举键值", value));
        }

        #endregion

        #region 属性

        /// <summary>
        /// 值
        /// </summary>
        private readonly Enum _value;

        /// <summary>
        /// 值
        /// </summary>
        public Enum Value
        {
            get { return _value; }
        }

        private string _key;

        /// <summary>
        /// 键
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        private string _caption;

        /// <summary>
        /// 标签
        /// </summary>
        public string Caption
        {
            get { return _caption; }
        }

        private string _tag;

        /// <summary>
        /// 标记
        /// </summary>
        public string Tag
        {
            get { return _tag; }
        }

        /// <summary>
        /// 标记
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public int Flag
        {
            get { return Int32.Parse(_key); }
        }

        #endregion

        #region 方法

        #region Flags 和 Captions 互转

        /// <summary>
        /// 将标签组合替换成标记组合
        /// </summary>
        /// <param name="captions">标签组合</param>
        /// <param name="separator">标签分隔符</param>
        public static string CaptionsToFlags<TEnum>(string captions, char separator = Standards.ValueSeparator)
            where TEnum : struct
        {
            StringBuilder result = new StringBuilder();
            if (captions != null)
            {
                string[] strings = captions.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (EnumKeyValue item in Fetch<TEnum>())
                foreach (string s in strings)
                    if (String.CompareOrdinal(item.Caption, s.Trim()) == 0)
                    {
                        result.Append(item.Key);
                        result.Append(separator);
                        break;
                    }

                if (result.Length > 0)
                    result.Remove(result.Length - 1, 1);
            }

            return result.ToString();
        }

        /// <summary>
        /// 将标记组合替换成标签组合
        /// </summary>
        /// <param name="flags">标记组合</param>
        /// <param name="separator">标签分隔符</param>
        public static string FlagsToCaptions<TEnum>(string flags, char separator = Standards.ValueSeparator)
            where TEnum : struct
        {
            StringBuilder result = new StringBuilder();
            if (flags != null)
            {
                string[] strings = flags.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (EnumKeyValue item in Fetch<TEnum>())
                foreach (string s in strings)
                    if (String.CompareOrdinal(item.Key, s.Trim()) == 0)
                    {
                        result.Append(item.Caption);
                        result.Append(separator);
                        break;
                    }

                if (result.Length > 0)
                    result.Remove(result.Length - 1, 1);
            }

            return result.ToString();
        }

        #endregion

        #region IComparable 成员

        /// <summary>
        /// 比较对象
        /// </summary>
        public int CompareTo(object obj)
        {
            return CompareTo(obj as EnumKeyValue);
        }

        /// <summary>
        /// 比较对象
        /// </summary>
        public int CompareTo(EnumKeyValue other)
        {
            if (object.ReferenceEquals(other, this))
                return 0;
            if (object.ReferenceEquals(other, null))
                return 1;
            return String.CompareOrdinal(_key, other._key);
        }

        /// <summary>
        /// 比较对象
        /// </summary>
        public static int Compare(EnumKeyValue x, EnumKeyValue y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;
            if (object.ReferenceEquals(x, null))
                return -1;
            return x.CompareTo(y);
        }

        /// <summary>
        /// 等于
        /// </summary>
        public static bool operator ==(EnumKeyValue left, EnumKeyValue right)
        {
            return Compare(left, right) == 0;
        }

        /// <summary>
        /// 不等于
        /// </summary>
        public static bool operator !=(EnumKeyValue left, EnumKeyValue right)
        {
            return Compare(left, right) != 0;
        }

        /// <summary>
        /// 小于
        /// </summary>
        public static bool operator <(EnumKeyValue left, EnumKeyValue right)
        {
            return Compare(left, right) < 0;
        }

        /// <summary>
        /// 大于
        /// </summary>
        public static bool operator >(EnumKeyValue left, EnumKeyValue right)
        {
            return Compare(left, right) > 0;
        }

        #endregion

        /// <summary>
        /// 比较对象
        /// </summary>
        /// <param name="obj">对象</param>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, this))
                return true;
            EnumKeyValue other = obj as EnumKeyValue;
            if (object.ReferenceEquals(other, null))
                return false;
            return String.CompareOrdinal(_key, other._key) == 0;
        }

        /// <summary>
        /// 取哈希值(注意字符串在32位和64位系统有不同的算法得到不同的结果) 
        /// </summary>
        public override int GetHashCode()
        {
            return _value.GetType().FullName.GetHashCode() ^ _value.ToString().GetHashCode();
        }

        /// <summary>
        /// 字符串表示
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0}.{1}", _value.GetType().FullName, _value.ToString());
        }

        #endregion
    }
}