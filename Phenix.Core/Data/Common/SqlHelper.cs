using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Phenix.Core.Data.Common
{
    /// <summary>
    /// SQL助手
    /// </summary>
    public static class SqlHelper
    {
        /// <summary>
        /// 清理脚本前后多余的字符
        /// </summary>
        /// <param name="script">脚本</param>
        public static string ClearSpare(string script)
        {
            if (String.IsNullOrEmpty(script))
                return script;
            string result = script.Trim();
            while (result.Length >= 2 && result[0] == '(' && result[^1] == ')')
                result = result.Substring(1, result.Length - 2).Trim();
            return result;
        }

        /// <summary>
        /// 清理脚本里的引号、注释
        /// </summary>
        /// <param name="script">脚本</param>
        public static string ClearComment(string script)
        {
            if (String.IsNullOrEmpty(script))
                return script;
            StringBuilder result = new StringBuilder();
            int i = 0;
            while (i >= 0)
            {
                i = script.IndexOf("--", 0, StringComparison.Ordinal);
                if (i >= 0)
                {
                    if (i > 0)
                        result.Append(script.Substring(0, i));
                    script = script.Remove(0, i + 2);
                    int j = script.IndexOf('\n', 0);
                    if (j >= 0)
                    {
                        result.Append(String.Empty.PadRight(j + 1, ' '));
                        script = script.Remove(0, j + 1);
                    }

                    continue;
                }

                i = script.IndexOf("/*", 0, StringComparison.Ordinal);
                if (i >= 0)
                {
                    if (i > 0)
                        result.Append(script.Substring(0, i));
                    script = script.Remove(0, i + 2);
                    int j = script.IndexOf("*/", 0, StringComparison.Ordinal);
                    if (j >= 0)
                    {
                        result.Append(String.Empty.PadRight(j + 2, ' '));
                        script = script.Remove(0, j + 2);
                    }

                    continue;
                }

                result.Append(script);
            }

            return new Regex(@"[\x00-\x1F]+").Replace(result.ToString().Replace("`", ""), " ");
        }

        /// <summary>
        /// fetchScript是否是联接语句
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static bool FetchScriptIsUnion(string fetchScript)
        {
            if (String.IsNullOrEmpty(fetchScript))
                return false;
            string text = ClearComment(fetchScript);
            return
                (text.IndexOf(" UNION ", StringComparison.OrdinalIgnoreCase) != -1) ||
                (text.IndexOf(" INTERSECT ", StringComparison.OrdinalIgnoreCase) != -1) ||
                (text.IndexOf(" MINUS ", StringComparison.OrdinalIgnoreCase) != -1);
        }

        /// <summary>
        /// fetchScript是否是分组语句
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static bool FetchScriptIsGroup(string fetchScript)
        {
            if (String.IsNullOrEmpty(fetchScript))
                return false;
            string text = ClearComment(fetchScript);
            return
                (text.IndexOf(" DISTINCT ", StringComparison.OrdinalIgnoreCase) != -1) ||
                (text.IndexOf(" UNIQUE ", StringComparison.OrdinalIgnoreCase) != -1) ||
                (text.IndexOf(" GROUP BY ", StringComparison.OrdinalIgnoreCase) != -1) ||
                (text.IndexOf(" CONNECT BY ", StringComparison.OrdinalIgnoreCase) != -1);
        }

        /// <summary>
        /// fetchScript是否是复合语句
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static bool FetchScriptIsComplex(string fetchScript)
        {
            return FetchScriptIsUnion(fetchScript) || FetchScriptIsGroup(fetchScript);
        }

        /// <summary>
        /// 检索from语句在fetch脚本中的位置
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static int FindFromIndex(string fetchScript)
        {
            if (String.IsNullOrEmpty(fetchScript))
                return -1;
            int haveBrace = 0;
            for (int i = 0; i < fetchScript.Length; i++)
                if (fetchScript[i] == '(')
                    haveBrace = haveBrace + 1;
                else if (fetchScript[i] == ')')
                    haveBrace = haveBrace - 1;
                else if (haveBrace == 0)
                    if (fetchScript.IndexOf(" FROM ", i, fetchScript.Length - i > 6 ? 6 : 0, StringComparison.OrdinalIgnoreCase) == i)
                        return i + 1;
            return -1;
        }

        /// <summary>
        /// 检索where语句在fetch脚本中的位置
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static int FindWhereIndex(string fetchScript)
        {
            if (String.IsNullOrEmpty(fetchScript))
                return -1;
            int haveBrace = 0;
            for (int i = 0; i < fetchScript.Length; i++)
                if (fetchScript[i] == '(')
                    haveBrace = haveBrace + 1;
                else if (fetchScript[i] == ')')
                    haveBrace = haveBrace - 1;
                else if (haveBrace == 0)
                    if (fetchScript.IndexOf(" WHERE ", i, fetchScript.Length - i > 7 ? 7 : 0, StringComparison.OrdinalIgnoreCase) == i ||
                        fetchScript.IndexOf(" WHERE(", i, fetchScript.Length - i > 7 ? 7 : 0, StringComparison.OrdinalIgnoreCase) == i)
                        return i + 1;
            return -1;
        }

        /// <summary>
        /// 检索orderby语句在fetch脚本中的位置
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static int FindOrderIndex(string fetchScript)
        {
            if (String.IsNullOrEmpty(fetchScript))
                return -1;
            int haveBrace = 0;
            for (int i = 0; i < fetchScript.Length; i++)
                if (fetchScript[i] == '(')
                    haveBrace = haveBrace + 1;
                else if (fetchScript[i] == ')')
                    haveBrace = haveBrace - 1;
                else if (haveBrace == 0)
                    if (fetchScript.IndexOf(" order by ", i, fetchScript.Length - i > 10 ? 10 : 0, StringComparison.OrdinalIgnoreCase) == i)
                        return i + 1;
            return -1;
        }

        /// <summary>
        /// 检索groupby语句在fetch脚本中的位置
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static int FindGroupIndex(string fetchScript)
        {
            if (String.IsNullOrEmpty(fetchScript))
                return -1;
            int haveBrace = 0;
            for (int i = 0; i < fetchScript.Length; i++)
                if (fetchScript[i] == '(')
                    haveBrace = haveBrace + 1;
                else if (fetchScript[i] == ')')
                    haveBrace = haveBrace - 1;
                else if (haveBrace == 0)
                    if (fetchScript.IndexOf(" GROUP BY ", i, fetchScript.Length - i > 10 ? 10 : 0, StringComparison.OrdinalIgnoreCase) == i)
                        return i + 1;
            return -1;
        }

        /// <summary>
        /// 检索connectby语句在fetch脚本中的位置
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static int FindConnectIndex(string fetchScript)
        {
            if (String.IsNullOrEmpty(fetchScript))
                return -1;
            int haveBrace = 0;
            for (int i = 0; i < fetchScript.Length; i++)
                if (fetchScript[i] == '(')
                    haveBrace = haveBrace + 1;
                else if (fetchScript[i] == ')')
                    haveBrace = haveBrace - 1;
                else if (haveBrace == 0)
                    if (fetchScript.IndexOf(" CONNECT BY ", i, fetchScript.Length - i > 12 ? 12 : 0, StringComparison.OrdinalIgnoreCase) == i)
                        return i + 1;
            return -1;
        }

        /// <summary>
        /// 取fetchScript中字段体
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static string GetColumnBody(string fetchScript)
        {
            string result = ClearSpare(ClearComment(fetchScript));
            int index = FindFromIndex(result);
            if (index == -1)
                return null;
            result = result.Remove(index).Trim();
            if (result.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
                result = result.Substring(7).TrimStart();
            if (result.StartsWith("DISTINCT ", StringComparison.OrdinalIgnoreCase))
                result = result.Substring(9).TrimStart();
            else if (result.StartsWith("UNIQUE ", StringComparison.OrdinalIgnoreCase))
                result = result.Substring(7).TrimStart();
            else if (result.StartsWith("ALL ", StringComparison.OrdinalIgnoreCase))
                result = result.Substring(4).TrimStart();
            return result;
        }

        /// <summary>
        /// 取fetchScript中排序体
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        public static string GetOrderBody(string fetchScript)
        {
            string result = ClearSpare(ClearComment(fetchScript));
            int index = FindOrderIndex(result);
            if (index == -1)
                return null;
            return result.Substring(index + 10).Trim();
        }

        private static List<string> ExtractSourceBody(string sourceBodyText)
        {
            List<string> result = new List<string>();
            if (String.IsNullOrEmpty(sourceBodyText))
                return result;
            string text = sourceBodyText + ',';
            int haveBrace = 0;
            int startIndex = 0;
            for (int i = 0; i < text.Length; i++)
                if (text[i] == '(')
                    haveBrace = haveBrace + 1;
                else if (text[i] == ')')
                    haveBrace = haveBrace - 1;
                else if (haveBrace == 0 && text[i] == ',')
                {
                    string s = ClearSpare(text.Substring(startIndex, i - startIndex));
                    if (!String.IsNullOrEmpty(s))
                        result.Add(s);
                    startIndex = i + 1;
                }

            return result;
        }

        private static void ExtractSourceBody(string fetchScript, ref Dictionary<string, List<string>> semiSourceBody)
        {
            foreach (KeyValuePair<string, List<string>> kvp in GetSourceBody(fetchScript))
                if (semiSourceBody.TryGetValue(kvp.Key, out List<string> tableAliases))
                {
                    foreach (string s in kvp.Value)
                        if (!tableAliases.Exists(item => String.Compare(s, item, StringComparison.OrdinalIgnoreCase) == 0))
                            tableAliases.Add(s);
                }
                else
                    semiSourceBody[kvp.Key] = kvp.Value;
        }

        private static Dictionary<string, List<string>> ExtractSourceBody(string sourceBodyText, Dictionary<string, List<string>> semiSourceBody)
        {
            foreach (string s in ExtractSourceBody(sourceBodyText))
            {
                string tableAlias = null;
                string tableName = s;
                int index = tableName.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                    tableName = ClearSpare(tableName.Remove(index));
                index = tableName.LastIndexOf(' ');
                if (index != -1)
                {
                    tableAlias = ClearSpare(tableName.Substring(index));
                    tableName = ClearSpare(tableName.Remove(index));
                }

                if (FindFromIndex(tableName) != -1)
                    ExtractSourceBody(tableName, ref semiSourceBody);
                else
                {
                    tableName = tableName.Substring(tableName.IndexOf('.') + 1);
                    tableAlias = tableAlias ?? tableName;
                    if (semiSourceBody.TryGetValue(tableName, out List<string> tableAliases))
                    {
                        if (!tableAliases.Exists(item => String.Compare(tableAlias, item, StringComparison.OrdinalIgnoreCase) == 0))
                            tableAliases.Add(tableAlias);
                    }
                    else
                    {
                        tableAliases = new List<string> {tableAlias};
                        semiSourceBody[tableName] = tableAliases;
                    }
                }
            }

            return semiSourceBody;
        }

        /// <summary>
        /// 取fetchScript中数据源队列
        /// </summary>
        /// <param name="fetchScript">fetch脚本</param>
        /// <returns>结果集(tableName-tableAliases)</returns>
        public static IDictionary<string, List<string>> GetSourceBody(string fetchScript)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            string text = ClearSpare(ClearComment(fetchScript));
            if (String.IsNullOrEmpty(text))
                return result;
            int index = FindFromIndex(text);
            if (index == -1)
                return result;
            text = text.Remove(0, index + 5);
            index = FindWhereIndex(text);
            if (index != -1)
            {
                ExtractSourceBody(text.Substring(index + 5), ref result);
                text = text.Remove(index);
            }

            index = FindOrderIndex(text);
            if (index != -1)
                text = text.Remove(index);
            index = FindGroupIndex(text);
            if (index != -1)
                text = text.Remove(index);
            index = FindConnectIndex(text);
            if (index != -1)
                text = text.Remove(index);
            text = text.ToUpper().Replace('(', ' ').Replace(')', ' ').Replace(" LEFT JOIN ", ",").Replace(" LEFT OUTER JOIN ", ",").Replace(" RIGHT JOIN ", ",").Replace(" RIGHT OUTER JOIN ", ",").Replace(" FULL JOIN ", ",").Replace(" FULL OUTER JOIN ", ",").Replace(" INNER JOIN ", ",").Replace(" CROSS JOIN ", ",").Replace(" JOIN ", ",").Replace(" AS ", " ");
            return ExtractSourceBody(text, result);
        }

        //private static string TrimColumnBody(string columnBody)
        //{
        //  string result = columnBody.Trim();
        //  result = Regex.Replace(result, @"\s{1,}", " ", RegexOptions.IgnoreCase);
        //  result = Regex.Replace(result, @"\s,|,\s", ",", RegexOptions.IgnoreCase);
        //  return result + ',';
        //}

        /// <summary>
        /// 搜寻与字段别名对应的表达式
        /// </summary>
        /// <param name="columnBody">字段体</param>
        /// <param name="columnAlias">字段别名</param>
        public static string FindColumnExpression(string columnBody, string columnAlias)
        {
            if (String.IsNullOrEmpty(columnBody))
                return null;
            string result;
            string aliasPattern = String.Format(@"^* {0}$", columnAlias);
            string columnPattern = String.Format(@"^\w*\.?\w+\.{0}$|^{0}$", columnAlias);
            foreach (string s in ClearComment(columnBody).Split(new Char[] {','}, StringSplitOptions.RemoveEmptyEntries))
            {
                result = s.Trim();
                try
                {
                    if (Regex.IsMatch(result, aliasPattern, RegexOptions.IgnoreCase))
                        goto Label;
                    if (Regex.IsMatch(result, columnPattern, RegexOptions.IgnoreCase))
                        return result;
                }
                catch (Exception)
                {
                    goto Label;
                }
            }

            return null;

            Label:
            if (result.Length > columnAlias.Length)
                result = result.Substring(0, result.Length - columnAlias.Length);
            result = result.TrimEnd();
            if (result.EndsWith(" as", StringComparison.OrdinalIgnoreCase))
                return result.Remove(result.LastIndexOf(' '));
            return result;
        }

        /// <summary>
        /// 唯一参数名 = 参数名+随机号 
        /// </summary>
        public static string UniqueParameterName()
        {
            return String.Format("P{0}", Guid.NewGuid().ToString("N").Substring(0, 29));
        }

        /// <summary>
        /// 拼装完整字段名 = 表名.字段名
        /// 当columnName中存在表名时返回值不变
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnName">字段名</param>
        public static string AssembleFullTableColumnName(string tableName, string columnName)
        {
            if (IsColumnName(columnName))
            {
                int index = columnName.IndexOf('.');
                if (index == -1 && !String.IsNullOrEmpty(tableName))
                    return tableName + '.' + columnName;
            }

            return columnName;
        }

        /// <summary>
        /// 抽取字段数据源
        /// </summary>
        /// <param name="columnName">字段名</param>
        public static string ExtractColumnSource(string columnName)
        {
            string result = null;
            if (IsColumnName(columnName))
            {
                string temp = columnName;
                int index = temp.IndexOf('.');
                while (index != -1)
                {
                    result = temp.Substring(0, index);
                    temp = temp.Substring(index + 1);
                    index = temp.IndexOf('.');
                }
            }

            return result;
        }

        /// <summary>
        /// 抽取短字段名
        /// 剔除columnName中的表名
        /// </summary>
        /// <param name="columnName">字段名</param>
        public static string ExtractShortColumnName(string columnName)
        {
            string result = columnName;
            if (IsColumnName(result))
            {
                int index = result.IndexOf('.');
                while (index != -1)
                {
                    result = result.Substring(index + 1);
                    index = result.IndexOf('.');
                }
            }

            return result;
        }

        private static bool IsColumnName(string columnName)
        {
            if (String.IsNullOrEmpty(columnName))
                return false;
            for (int i = 0; i < columnName.Length; i++)
                if (!Char.IsLetterOrDigit(columnName[i]) && columnName[i] != '.' && columnName[i] != '_')
                    return false;
            return true;
        }
    }
}