using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Phenix.Core.Data
{
    /// <summary>
    /// 规格
    /// </summary>
    public static class Standards
    {
        #region 属性

        /// <summary>
        /// "null"
        /// </summary>
        public const string Null = "null";

        /// <summary>
        /// 换行字串
        /// </summary>
        public const string CrLf = "\r\n";

        /// <summary>
        /// 分隔字串" #-*-# "
        /// </summary>
        public const string Separator = " #-*-# ";

        /// <summary>
        /// 块分隔符
        /// </summary>
        public const char BlockSeparator = '\u0017';

        /// <summary>
        /// 字段分隔符
        /// </summary>
        public const char FieldSeparator = '\u0003';

        /// <summary>
        /// 行分隔符
        /// </summary>
        public const char RowSeparator = '\u0004';

        /// <summary>
        /// 参数分隔符'_'
        /// </summary>
        public const char ParamSeparator = '_';

        /// <summary>
        /// 值分隔符','
        /// </summary>
        public const char ValueSeparator = ',';

        /// <summary>
        /// 标点分隔符'.'
        /// </summary>
        public const char PointSeparator = '.';

        /// <summary>
        /// 隶属分隔符'='
        /// </summary>
        public const char EqualSeparator = '=';

        /// <summary>
        /// 未知值"*"
        /// </summary>
        public const string UnknownValue = "*";

        #region 配置项

        private static string _defaultKeyColumnName;

        /// <summary>
        /// 缺省"主键/外键"的 ColumnName
        /// 默认：^ID$|^*_ID$
        /// </summary>
        public static string DefaultKeyColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultKeyColumnName, "^ID$|^*_ID$"); }
            set { AppSettings.SetLocalProperty(ref _defaultKeyColumnName, value); }
        }

        private static string _defaultOriginatorColumnName;

        /// <summary>
        /// 缺省"制单人"的 ColumnName
        /// 默认：^\w{0,3}_ORIGINATOR$
        /// </summary>
        public static string DefaultOriginatorColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultOriginatorColumnName, @"^\w{0,3}_ORIGINATOR$"); }
            set { AppSettings.SetLocalProperty(ref _defaultOriginatorColumnName, value); }
        }

        private static string _defaultOriginateTimeColumnName;

        /// <summary>
        /// 缺省"制单时间"的 ColumnName
        /// 默认：^\w{0,3}_ORIGINATE_TIME$
        /// </summary>
        public static string DefaultOriginateTimeColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultOriginateTimeColumnName, @"^\w{0,3}_ORIGINATE_TIME$"); }
            set { AppSettings.SetLocalProperty(ref _defaultOriginateTimeColumnName, value); }
        }

        private static string _defaultOriginateTeamsColumnName;

        /// <summary>
        /// 缺省"制单团体"的 ColumnName
        /// 默认：^\w{0,3}_ORIGINATE_TEAMS$
        /// </summary>
        public static string DefaultOriginateTeamsColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultOriginateTeamsColumnName, @"^\w{0,3}_ORIGINATE_TEAMS$"); }
            set { AppSettings.SetLocalProperty(ref _defaultOriginateTeamsColumnName, value); }
        }

        private static string _defaultUpdaterColumnName;

        /// <summary>
        /// 缺省"更新人"的 ColumnName
        /// 默认：^\w{0,3}_UPDATER$
        /// </summary>
        public static string DefaultUpdaterColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultUpdaterColumnName, @"^\w{0,3}_UPDATER$"); }
            set { AppSettings.SetLocalProperty(ref _defaultUpdaterColumnName, value); }
        }

        private static string _defaultUpdateTimeColumnName;

        /// <summary>
        /// 缺省"更新时间"的 ColumnName
        /// 默认：^\w{0,3}_UPDATE_TIME$
        /// </summary>
        public static string DefaultUpdateTimeColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultUpdateTimeColumnName, @"^\w{0,3}_UPDATE_TIME$"); }
            set { AppSettings.SetLocalProperty(ref _defaultUpdateTimeColumnName, value); }
        }

        private static string _defaultTimestampColumnName;
        /// <summary>
        /// 缺省"时间戳"的 ColumnName
        /// 默认：^\w{0,3}_TIMESTAMP$
        /// </summary>
        public static string DefaultTimestampColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultTimestampColumnName, @"^\w{0,3}_TIMESTAMP$"); }
            set { AppSettings.SetLocalProperty(ref _defaultTimestampColumnName, value); }
        }

        private static string _defaultRouteColumnName;

        /// <summary>
        /// 缺省"HASH值路由增删改查数据库"的 ColumnName
        /// 默认：^*_RU$
        /// </summary>
        public static string DefaultRouteColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultRouteColumnName, "^*_RU$"); }
            set { AppSettings.SetLocalProperty(ref _defaultRouteColumnName, value); }
        }

        private static string _defaultWatermarkColumnName;

        /// <summary>
        /// 缺省"仅在insert时被提交"的 ColumnName
        /// 默认：^*_WM$
        /// </summary>
        public static string DefaultWatermarkColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultWatermarkColumnName, "^*_WM$"); }
            set { AppSettings.SetLocalProperty(ref _defaultWatermarkColumnName, value); }
        }

        private static string _defaultEnumColumnName;

        /// <summary>
        /// 缺省"枚举/布尔"的 ColumnName
        /// 默认：^*_FG$
        /// </summary>
        public static string DefaultEnumColumnName
        {
            get { return AppSettings.GetLocalProperty(ref _defaultEnumColumnName, "^*_FG$"); }
            set { AppSettings.SetLocalProperty(ref _defaultEnumColumnName, value); }
        }

        private static string _isNullSign;

        /// <summary>
        /// is null 标识
        /// 默认：null
        /// </summary>
        public static string IsNullSign
        {
            get { return AppSettings.GetLocalProperty(ref _isNullSign, Null); }
            set { AppSettings.SetLocalProperty(ref _isNullSign, value); }
        }

        #endregion

        #endregion

        #region 方法

        #region 保留字段名

        /// <summary>
        /// 是否缺省"主键/外键"的 ColumnName（字段类型需是长整型15位以上精度）
        /// </summary>
        public static bool IsDefaultKeyColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            return Regex.IsMatch(columnName, DefaultKeyColumnName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否缺省"制单人"的 ColumnName（字段类型需是字符串/长整型15位以上精度）
        /// </summary>
        public static bool IsDefaultOriginatorColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            return Regex.IsMatch(columnName, DefaultOriginatorColumnName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否缺省"制单时间"的 ColumnName（字段类型需是DateTime）
        /// </summary>
        public static bool IsDefaultOriginateTimeColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            return Regex.IsMatch(columnName, DefaultOriginateTimeColumnName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否缺省"制单团体"的 ColumnName（字段类型需是字符串/长整型15位以上精度）
        /// </summary>
        public static bool IsDefaultOriginateTeamsColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            return Regex.IsMatch(columnName, DefaultOriginateTeamsColumnName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否缺省"更新人"的 ColumnName（字段类型需是字符串/长整型15位以上精度）
        /// </summary>
        public static bool IsDefaultUpdaterColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            return Regex.IsMatch(columnName, DefaultUpdaterColumnName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否缺省"更新时间"的 ColumnName（字段类型需是DateTime）
        /// </summary>
        public static bool IsDefaultUpdateTimeColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            return Regex.IsMatch(columnName, DefaultUpdateTimeColumnName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否缺省"时间戳"的 ColumnName（字段类型需是长整型15位以上精度）
        /// </summary>
        public static bool IsDefaultTimestampColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            return Regex.IsMatch(columnName, DefaultTimestampColumnName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否缺省"HASH值路由增删改查数据库"的 ColumnName
        /// </summary>
        public static bool IsDefaultRouteColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            columnName = new Regex(DefaultKeyColumnName, RegexOptions.IgnoreCase).Replace(columnName, "");
            columnName = new Regex(DefaultWatermarkColumnName, RegexOptions.IgnoreCase).Replace(columnName, "");
            columnName = new Regex(DefaultEnumColumnName, RegexOptions.IgnoreCase).Replace(columnName, "");
            return Regex.IsMatch(columnName, DefaultRouteColumnName, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否缺省"仅在insert时被提交"的 ColumnName
        /// </summary>
        public static bool IsDefaultWatermarkColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            columnName = new Regex(DefaultKeyColumnName, RegexOptions.IgnoreCase).Replace(columnName, "");
            columnName = new Regex(DefaultRouteColumnName, RegexOptions.IgnoreCase).Replace(columnName, "");
            columnName = new Regex(DefaultEnumColumnName, RegexOptions.IgnoreCase).Replace(columnName, "");
            return Regex.IsMatch(columnName, DefaultWatermarkColumnName, RegexOptions.IgnoreCase) ||
                   IsDefaultOriginatorColumnName(columnName) || IsDefaultOriginateTimeColumnName(columnName) || IsDefaultOriginateTeamsColumnName(columnName) || 
                   IsDefaultRouteColumnName(columnName);
        }

        /// <summary>
        /// 是否缺省"枚举/布尔"的 ColumnName（字段类型需是整型2位/1位精度）
        /// </summary>
        public static bool IsDefaultEnumColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            columnName = new Regex(DefaultWatermarkColumnName, RegexOptions.IgnoreCase).Replace(columnName, "");
            columnName = new Regex(DefaultRouteColumnName, RegexOptions.IgnoreCase).Replace(columnName, "");
            return Regex.IsMatch(columnName, DefaultEnumColumnName, RegexOptions.IgnoreCase);
        }

        #endregion

        #region 代码规范

        /// <summary>
        /// 整理表名
        /// 如果第1-prefixCount位后是"_"则剔去其及之前的字符
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="prefixCount">前缀字符数</param>
        public static string TrimTableName(string tableName, int prefixCount = 3)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            int index = tableName.IndexOf(ParamSeparator);
            return index >= 0 && index <= prefixCount ? tableName.Remove(0, index + 1) : tableName;
        }

        /// <summary>
        /// 取PascalCasing名
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="trim">是否整理</param>
        /// <param name="prefixCount">前缀字符数</param>
        public static string GetPascalCasingByTableName(string tableName, bool trim = false, int prefixCount = 3)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            StringBuilder result = new StringBuilder();
            foreach (string s in (trim ? TrimTableName(tableName, prefixCount) : tableName).Split(new Char[] {ParamSeparator}, StringSplitOptions.RemoveEmptyEntries))
            {
                result.Append(Char.ToUpper(s[0]));
                result.Append(s.Remove(0, 1).ToLower());
            }

            return result.ToString();
        }

        /// <summary>
        /// 取前缀
        /// </summary>
        /// <param name="sheetName">表/视图名</param>
        /// <param name="prefixCount">前缀字符数</param>
        public static string GetPrefixBySheetName(string sheetName, int prefixCount = 3)
        {
            if (sheetName == null)
                throw new ArgumentNullException(nameof(sheetName));

            int index = sheetName.IndexOf(ParamSeparator);
            return index >= 0 && index <= prefixCount ? sheetName.Substring(0, index) : String.Empty;
        }

        /// <summary>
        /// 整理视图名
        /// 如果第1-prefixCount位后是"_"则剔去其及之前的字符
        /// 如果第suffixCount位前是"_"则剔去其及之后的字符
        /// </summary>
        /// <param name="viewName">视图名</param>
        /// <param name="prefixCount">前缀字符数</param>
        /// <param name="suffixCount">后缀字符数</param>
        public static string TrimViewName(string viewName, int prefixCount = 3, int suffixCount = 1)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            int index = viewName.IndexOf(ParamSeparator);
            string result = index >= 0 && index <= prefixCount ? viewName.Remove(0, index + 1) : viewName;
            if (result.Length > suffixCount + 1 && result[result.Length - suffixCount - 1] == ParamSeparator)
                result = result.Remove(result.Length - suffixCount - 1);
            return result;
        }

        /// <summary>
        /// 取PascalCasing名
        /// </summary>
        /// <param name="viewName">视图名</param>
        /// <param name="trim">是否整理</param>
        /// <param name="prefixCount">前缀字符数</param>
        /// <param name="suffixCount">后缀字符数</param>
        public static string GetPascalCasingByViewName(string viewName, bool trim = false, int prefixCount = 3, int suffixCount = 1)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            StringBuilder result = new StringBuilder();
            foreach (string s in (trim ? TrimViewName(viewName, prefixCount, suffixCount) : viewName).Split(new Char[] {ParamSeparator}, StringSplitOptions.RemoveEmptyEntries))
            {
                result.Append(Char.ToUpper(s[0]));
                result.Append(s.Remove(0, 1).ToLower());
            }

            return result.ToString();
        }

        /// <summary>
        /// 整理字段名
        /// 如果第1-prefixCount位后是"_"则剔去其及之前的字符
        /// 剔除DefaultRouteColumnName、DefaultWatermarkColumnName、DefaultEnumColumnName后缀
        /// </summary>
        /// <param name="columnName">字段名</param>
        /// <param name="prefixCount">前缀字符数</param>
        public static string TrimColumnName(string columnName, int prefixCount = 3)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            int index = columnName.IndexOf(ParamSeparator);
            string result = index >= 0 && index <= prefixCount ? columnName.Remove(0, index + 1) : columnName;
            result = new Regex(DefaultRouteColumnName, RegexOptions.IgnoreCase).Replace(result, "");
            result = new Regex(DefaultWatermarkColumnName, RegexOptions.IgnoreCase).Replace(result, "");
            return new Regex(DefaultEnumColumnName, RegexOptions.IgnoreCase).Replace(result, "");
        }

        /// <summary>
        /// 取PascalCasing名
        /// </summary>
        /// <param name="columnName">字段名</param>
        /// <param name="trim">是否整理</param>
        /// <param name="prefixCount">前缀字符数</param>
        public static string GetPascalCasingByColumnName(string columnName, bool trim = true, int prefixCount = 3)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            StringBuilder result = new StringBuilder();
            foreach (string s in (trim ? TrimColumnName(columnName, prefixCount) : columnName).Split(new Char[] {ParamSeparator}, StringSplitOptions.RemoveEmptyEntries))
            {
                result.Append(Char.ToUpper(s[0]));
                result.Append(s.Remove(0, 1).ToLower());
            }

            return result.ToString();
        }

        /// <summary>
        /// 取类属性名
        /// </summary>
        /// <param name="fieldName">类字段名</param>
        public static string GetPropertyNameByFieldName(string fieldName)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            string result = fieldName.TrimStart('_');
            return result.Length >= 2
                ? String.Format("{0}{1}", Char.ToUpper(result[0]), result.Remove(0, 1))
                : result.Length == 1
                    ? result.ToUpper()
                    : result;
        }

        /// <summary>
        /// 取类字段名
        /// </summary>
        /// <param name="propertyName">类属性名</param>
        public static string GetFieldNameByPropertyName(string propertyName)
        {
            return String.Format("_{0}", GetParameterNameByPropertyName(propertyName));
        }

        /// <summary>
        /// 取参数名
        /// </summary>
        /// <param name="propertyName">类属性名</param>
        public static string GetParameterNameByPropertyName(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            return propertyName.Length >= 2
                ? String.Format("{0}{1}", Char.ToLower(propertyName[0]), propertyName.Remove(0, 1))
                : propertyName.Length == 1
                    ? propertyName.ToLower()
                    : propertyName;
        }

        /// <summary>
        /// 取标准属性名(剔除后缀)
        /// </summary>
        /// <param name="binderName">类属性名/类字段名/参数名</param>
        public static string GetStandardPropertyName(string binderName)
        {
            if (binderName == null)
                throw new ArgumentNullException(nameof(binderName));

            string result = binderName.TrimStart('_');
            int index = result.IndexOf(ParamSeparator);
            if (index > 0)
                result = result.Remove(index);
            return result.Length >= 2
                ? String.Format("{0}{1}", Char.ToUpper(result[0]), result.Remove(0, 1))
                : result.Length == 1
                    ? result.ToUpper()
                    : result;
        }

        #endregion
        
        #region CompoundKey

        /// <summary>
        /// 格式化复合键
        /// </summary>
        public static string FormatCompoundKey(object key1, object key2)
        {
            return String.Format("{0}{1}{2}", key1, RowSeparator, key2);
        }

        /// <summary>
        /// 格式化复合键
        /// </summary>
        public static string FormatCompoundKey(object key1, object key2, object key3)
        {
            return String.Format("{0}{1}{2}{3}{4}", key1, RowSeparator, key2, RowSeparator, key3);
        }

        /// <summary>
        /// 格式化复合键
        /// </summary>
        public static string FormatCompoundKey(object key1, object key2, object key3, object key4)
        {
            return String.Format("{0}{1}{2}{3}{4}{5}{6}", key1, RowSeparator, key2, RowSeparator, key3, RowSeparator, key4);
        }


        /// <summary>
        /// 格式化复合键
        /// </summary>
        public static string FormatCompoundKey(object key1, object key2, object key3, object key4, object key5)
        {
            return String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", key1, RowSeparator, key2, RowSeparator, key3, RowSeparator, key4, RowSeparator, key5);
        }

        /// <summary>
        /// 分解复合键
        /// </summary>
        public static string[] SplitCompoundKey(string compoundKey)
        {
            return compoundKey.Split(RowSeparator);
        }

        #endregion

        /// <summary>
        /// 格式化年月
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>格式化年月</returns>
        public static DateTime FormatYearMonth(short year, short month)
        {
            return new DateTime(year, month, 1);
        }

        /// <summary>
        /// 提取本地文本
        /// </summary>
        /// <param name="text">文本(中英文用‘|’分隔)</param>
        /// <param name="cultureInfo">CultureInfo(Name为非'zh-'时返回后半截)</param>
        public static string ExtractCultureText(string text, CultureInfo cultureInfo)
        {
            if (String.IsNullOrEmpty(text))
                return null;
            string[] strings = text.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length == 0)
                return text;
            if (strings.Length == 1 || cultureInfo.Name.IndexOf("zh-", StringComparison.OrdinalIgnoreCase) == 0)
                return strings[0];
            return strings[1];
        }

        #endregion
    }
}