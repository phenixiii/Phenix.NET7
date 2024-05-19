using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Phenix.Core.Data;
using Phenix.Core.Log;
using Phenix.Core.SyncCollections;

namespace Phenix.Core.Reflection
{
    /// <summary>
    /// 工具集
    /// </summary>
    public static class Utilities
    {
        #region 属性

        /// <summary>
        /// Json时间格式
        /// </summary>
        public const string JsonDateFormatString = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";

        private static readonly SynchronizedDictionary<string, Type> _typeCache = new SynchronizedDictionary<string, Type>(StringComparer.Ordinal);

        #endregion

        #region 方法

        #region Assembly

        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="assemblyName">程序集名</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <returns>程序集</returns>
        public static Assembly LoadAssembly(string assemblyName, bool throwIfNotFound = false)
        {
            try
            {
                return AppDomain.CurrentDomain.Load(assemblyName);
            }
            catch (FileNotFoundException ex)
            {
                if (throwIfNotFound)
                    throw new InvalidOperationException(String.Format("不存在程序集: {0}", assemblyName), ex);
                return null;
            }
        }

        #endregion

        #region TypeName

        /// <summary>
        /// 拼装完整类名 = 命名空间.类名
        /// 当typeName中存在命名空间时返回值不变
        /// </summary>
        /// <param name="typeNamespace">命名空间</param>
        /// <param name="typeName">类名</param>
        public static string AssembleFullTypeName(string typeNamespace, string typeName)
        {
            if (typeName != null)
            {
                if (!typeName.Contains('.') && !String.IsNullOrEmpty(typeNamespace))
                    return String.Format("{0}.{1}", typeNamespace, typeName);
            }

            return typeName;
        }

        /// <summary>
        /// 抽取命名空间
        /// </summary>
        /// <param name="fullTypeName">完整类名 </param>
        public static string ExtractTypeNamespace(string fullTypeName)
        {
            if (fullTypeName == null)
                return null;

            string result = null;
            if (IsTypeName(fullTypeName))
            {
                string temp = fullTypeName;
                int index = temp.IndexOf('.');
                while (index != -1)
                {
                    result = result != null ? String.Format("{0}.{1}", result, temp.Substring(0, index)) : temp.Substring(0, index);
                    temp = temp.Substring(index + 1);
                    index = temp.IndexOf('.');
                }
            }

            return result;
        }

        /// <summary>
        /// 抽取类名
        /// </summary>
        /// <param name="fullTypeName">完整类名 </param>
        public static string ExtractTypeName(string fullTypeName)
        {
            if (fullTypeName == null)
                return null;

            string result = fullTypeName;
            if (IsTypeName(fullTypeName))
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

        private static bool IsTypeName(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
                return false;

            for (int i = 0; i < typeName.Length; i++)
                if (!Char.IsLetterOrDigit(typeName[i]) && typeName[i] != '.' && typeName[i] != '_')
                    return false;
            return true;
        }

        #endregion

        #region Type

        /// <summary>
        /// 加载公共类类型
        /// </summary>
        /// <param name="fileName">程序集文件名</param>
        /// <param name="includeAbstract">是否包括抽象类型</param>
        /// <returns>类型队列</returns>
        public static IList<Type> LoadExportedClassTypes(string fileName, bool includeAbstract = false)
        {
            return GetExportedClassTypes(Assembly.LoadFile(fileName), includeAbstract);
        }

        /// <summary>
        /// 获取公共类类型
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="includeAbstract">是否包括抽象类型</param>
        /// <returns>类型队列</returns>
        public static IList<Type> GetExportedClassTypes(Assembly assembly, bool includeAbstract = false)
        {
            List<Type> result = new List<Type>();
            foreach (Type item in assembly.GetExportedTypes())
                try
                {
                    if (IsNotApplicationType(item))
                        continue;
                    if (!item.IsClass || item.IsAbstract && !includeAbstract || item.IsGenericType || item.IsCOMObject)
                        continue;
                    result.Add(item);
                }
                catch (Exception ex)
                {
                    LogHelper.Warning(ex, "{@Assembly}", assembly.GetName().FullName);
                }

            return result;
        }

        /// <summary>
        /// 加载类型
        /// 主要用于IDE环境、typeof(T)
        /// </summary>
        /// <param name="type">类</param>
        /// <returns>类</returns>
        public static Type LoadType(Type type)
        {
            if (type == null)
                return null;

            try
            {
                Type result = Type.GetType(type.FullName, false);
                return result != null ? result.IsConstructedGenericType && result.GenericTypeArguments.Length == 1 ? result.GenericTypeArguments[0] : result : type;
            }
            catch (ArgumentException)
            {
                return type;
            }
        }

        /// <summary>
        /// 加载类型
        /// </summary>
        /// <param name="typeName">类名</param>
        /// <param name="assemblyName">程序集名</param>
        /// <returns>类</returns>
        public static Type LoadType(string typeName, string assemblyName = null)
        {
            if (String.IsNullOrEmpty(typeName))
                return null;

            Type result = Type.GetType(typeName, false);
            if (result != null)
                return result;

            if (!String.IsNullOrEmpty(assemblyName))
            {
                Assembly assembly = LoadAssembly(assemblyName);
                return assembly != null ? assembly.GetType(typeName, false) : null;
            }

            return _typeCache.GetValue(typeName, () =>
            {
                assemblyName = typeName;
                while (true)
                {
                    int i = assemblyName.LastIndexOf('.');
                    if (i > 0)
                    {
                        assemblyName = assemblyName.Remove(i);
                        Type value = LoadType(typeName, assemblyName);
                        if (value != null)
                            return value;
                    }
                    else
                        break;
                }

                return null;
            });
        }

        /// <summary>
        /// 返回基础类型
        /// </summary>
        /// <param name="type">类</param>
        public static Type GetUnderlyingType(Type type)
        {
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type);
            return type;
        }

        /// <summary>
        /// 返回数据源的核心类型
        /// </summary>
        /// <param name="type">类</param>
        public static Type GetCoreType(Type type)
        {
            return FindListItemType(type) ?? type;
        }

        /// <summary>
        /// 返回队列项类型
        /// </summary>
        /// <param name="type">类</param>
        public static Type FindListItemType(Type type)
        {
            if (type == null)
                return null;
            if (!typeof(IEnumerable).IsAssignableFrom(type))
                return null;
            if (type == typeof(string))
                return null;

            if (type.IsArray)
                return type.GetElementType();

            foreach (Type interfaceType in type.GetInterfaces())
                if (String.CompareOrdinal(interfaceType.Name, "IEnumerable`1") == 0)
                    foreach (Type itemType in interfaceType.GetGenericArguments())
                        if (!itemType.IsGenericParameter && String.CompareOrdinal(itemType.Name, "KeyValuePair`2") != 0)
                            return itemType;
            return null;
        }

        /// <summary>
        /// 返回队列项类型
        /// </summary>
        public static IList<Type> FindKnownTypes(object value, Type valueType)
        {
            List<Type> result = new List<Type>();
            if (valueType == null && value != null)
                valueType = value.GetType();
            if (valueType != null)
            {
                result.Add(valueType);
                Type listItemType = FindListItemType(GetUnderlyingType(valueType));
                if (listItemType != null)
                {
                    result.Add(listItemType);
                    if (value != null)
                        foreach (object item in (IEnumerable) value)
                            if (item != null)
                            {
                                Type itemType = item.GetType();
                                if (!result.Contains(itemType))
                                    result.AddRange(FindKnownTypes(item, itemType));
                            }
                }
            }

            return result;
        }

        /// <summary>
        /// 返回具有指定 resultType 类型而且其值等效于指定对象的值
        /// </summary>
        /// <param name="value">值</param>
        public static T ChangeType<T>(object value)
        {
            return (T) ChangeType(value, typeof(T));
        }

        /// <summary>
        /// 返回具有指定 resultType 类型而且其值等效于指定对象的值
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="resultType">返回值的类型</param>
        public static object ChangeType(object value, Type resultType)
        {
            if (resultType == null || resultType == typeof(object))
                return value;

            if (value is string vs && resultType != typeof(string))
            {
                string s = vs.Trim();
                if (String.IsNullOrEmpty(s) || String.Compare(s, Standards.Null, StringComparison.OrdinalIgnoreCase) == 0)
                    value = null;
            }

            if (value == null || DBNull.Value.Equals(value))
                return resultType.IsValueType ? Activator.CreateInstance(resultType) : null;

            Type valueType = value.GetType();
            if (resultType.IsAssignableFrom(valueType))
                return value;

            Type resultUnderlyingType = GetUnderlyingType(resultType);
            if (valueType == resultUnderlyingType)
                return value;

            Type valueUnderlyingType = GetUnderlyingType(valueType);
            if (valueUnderlyingType == resultUnderlyingType)
                goto Label;

            if (valueUnderlyingType.IsValueType || resultUnderlyingType.IsValueType)
            {
                if (resultUnderlyingType.IsEnum)
                    return Enum.Parse(resultUnderlyingType, value.ToString());

                if (resultUnderlyingType == typeof(bool))
                {
                    if (valueUnderlyingType == typeof(string))
                    {
                        string s = ((string) value).Trim();
                        return IsNumeric(s) ? !IsZero(s) : String.Compare(s, "true", StringComparison.OrdinalIgnoreCase) == 0;
                    }

                    try
                    {
                        return Convert.ToBoolean(value);
                    }
                    catch (Exception)
                    {
                        goto Label;
                    }
                }

                if (resultUnderlyingType == typeof(System.Drawing.Color))
                    return System.Drawing.Color.FromArgb((int) Convert.ChangeType(value, typeof(int)));
                if (valueUnderlyingType == typeof(System.Drawing.Color))
                    return Convert.ChangeType(((System.Drawing.Color) value).ToArgb(), resultUnderlyingType);

                if (resultUnderlyingType == typeof(Guid))
                    return valueUnderlyingType == typeof(byte[])
                        ? new Guid((byte[]) value)
                        : new Guid(value.ToString());
                if (valueUnderlyingType == typeof(Guid))
                    return resultUnderlyingType == typeof(byte[])
                        ? ((Guid) value).ToByteArray()
                        : Convert.ChangeType(((Guid) value).ToString(), resultUnderlyingType);

                if (resultUnderlyingType == typeof(string))
                    return valueUnderlyingType == typeof(bool) ? value.ToString().ToLower() : value.ToString();

                goto Label;
            }
            
            if (resultUnderlyingType == typeof(string) && value is ICollection collection)
            {
                StringBuilder result = new StringBuilder();
                foreach (object item in collection)
                {
                    result.Append(item);
                    result.Append(Standards.ValueSeparator);
                }

                return result.ToString();
            }
            
            if (resultUnderlyingType.IsClass || resultUnderlyingType.IsInterface)
            {
                Type listItemType = FindListItemType(resultUnderlyingType);
                if (valueUnderlyingType == typeof(string))
                    if (listItemType != null)
                    {
                        ArrayList list = new ArrayList();
                        foreach (string s in ((string) value).Split(Standards.ValueSeparator))
                            list.Add(ChangeType(s, listItemType));
                        return resultUnderlyingType.IsArray || resultUnderlyingType.IsInterface
                            ? list.ToArray(listItemType)
                            : Activator.CreateInstance(resultUnderlyingType, list.ToArray(listItemType));
                    }
                    else if (resultUnderlyingType == typeof(Type))
                        return LoadType((string) value);
                    else if (!resultUnderlyingType.IsInterface)
                        return JsonConvert.DeserializeObject((string) value, resultUnderlyingType);

                if (!resultUnderlyingType.IsInterface)
                {
                    if (valueUnderlyingType == typeof(JObject))
                        return ((JObject) value).ToObject(resultUnderlyingType);
                    if (resultUnderlyingType == typeof(JObject) && valueUnderlyingType.IsClass)
                        return JObject.FromObject(value);
                }

                return value; //没辙了
            }

            if (resultUnderlyingType == typeof(string))
                if (valueUnderlyingType == typeof(Type))
                    return valueUnderlyingType.FullName;
                else if (valueUnderlyingType.IsClass)
                    return JsonSerialize(value);

            Label:
            try
            {
                return Convert.ChangeType(value, resultUnderlyingType);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(String.Format("{0}: value = '{1}', value type = '{2}', result type = '{3}[{4}]'", ex.Message, value, valueType, resultType, resultUnderlyingType), ex);
            }
        }

        /// <summary>
        /// 返回与数据库字段类型匹配的值
        /// </summary>
        /// <param name="value">值</param>
        public static object ConvertToDbValue(object value)
        {
            if (value == null)
                return DBNull.Value;

            Type valueUnderlyingType = GetUnderlyingType(value.GetType());
            if (valueUnderlyingType.IsEnum)
                return (int) value;
            if (valueUnderlyingType == typeof(bool))
                return (bool) value ? 1 : 0;
            if (valueUnderlyingType == typeof(DateTime))
                return ((DateTime) value).ToLocalTime();
            if (valueUnderlyingType == typeof(System.Drawing.Color))
                return ((System.Drawing.Color) value).ToArgb();
            if (valueUnderlyingType == typeof(Guid))
                return ((Guid) value).ToString();

            if (value is ICollection collection)
            {
                StringBuilder result = new StringBuilder();
                foreach (object item in collection)
                {
                    result.Append(item);
                    result.Append(Standards.ValueSeparator);
                }

                return result.ToString();
            }

            return value;
        }

        /// <summary>
        /// 检索主从结构中符合条件的队列类型
        /// </summary>
        /// <param name="type">类</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="levelNumber">层级数</param>
        public static Type FindDetailListType(Type type, string propertyName, int levelNumber)
        {
            int levelTally = 0;
            return DoFindDetailListType(type, propertyName, levelNumber, ref levelTally);
        }

        private static Type DoFindDetailListType(Type type, string propertyName, int levelNumber, ref int levelTally)
        {
            levelTally = levelTally + 1;
            if (levelTally > levelNumber)
                return null;

            foreach (PropertyInfo item in GetCoreType(type).GetProperties())
                if (FindListItemType(item.PropertyType) != null)
                {
                    if (levelTally == levelNumber && String.CompareOrdinal(item.Name, propertyName) == 0)
                        return item.PropertyType;
                    Type result = DoFindDetailListType(item.PropertyType, propertyName, levelNumber, ref levelTally);
                    if (result != null)
                        return result;
                }

            return null;
        }

        #endregion

        #region MemberInfo

        /// <summary>
        /// 检索属性信息
        /// </summary>
        /// <param name="objectType">类</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="bindingAttr">BindingFlags</param>
        public static PropertyInfo FindPropertyInfo(Type objectType, string propertyName,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (objectType == null || String.IsNullOrEmpty(propertyName))
                return null;

            Type type = objectType;
            do
            {
                PropertyInfo result = type.GetProperty(propertyName, BindingFlags.DeclaredOnly | bindingAttr);
                if (result != null)
                    return result;
                type = type.BaseType;
            } while (type != null);

            return null;
        }

        /// <summary>
        /// 检索字段信息
        /// </summary>
        /// <param name="objectType">类</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="bindingAttr">BindingFlags</param>
        public static FieldInfo FindFieldInfo(Type objectType, string fieldName,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (objectType == null || String.IsNullOrEmpty(fieldName))
                return null;

            Type type = objectType;
            do
            {
                FieldInfo result = type.GetField(fieldName, BindingFlags.DeclaredOnly | bindingAttr);
                if (result != null)
                    return result;
                type = type.BaseType;
            } while (type != null);

            return null;
        }

        /// <summary>
        /// 检索属性/字段信息
        /// </summary>
        /// <param name="objectType">类</param>
        /// <param name="memberName">属性/字段名</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        public static MemberInfo FindMemberInfo(Type objectType, string memberName, bool throwIfNotFound = false)
        {
            MemberInfo result = (MemberInfo) FindPropertyInfo(objectType, memberName) ?? FindFieldInfo(objectType, memberName);
            if (result != null)
                return result;

            if (throwIfNotFound)
                throw new InvalidOperationException(String.Format("类 {0} 不存在名称为 {1} 的属性/字段", objectType.FullName, memberName));
            return null;
        }

        /// <summary>
        /// 获取属性/字段的类型
        /// </summary>
        /// <param name="memberInfo">属性/字段信息</param>
        public static Type GetMemberType(MemberInfo memberInfo)
        {
            return memberInfo is PropertyInfo propertyInfo ? propertyInfo.PropertyType : memberInfo is FieldInfo fieldInfo ? fieldInfo.FieldType : null;
        }

        /// <summary>
        /// 获取属性/字段的值
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="memberName">属性/字段名</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        public static object GetMemberValue<T>(T entity, string memberName, bool throwIfNotFound = true)
            where T : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return GetMemberValue(entity, FindMemberInfo(entity.GetType(), memberName, throwIfNotFound));
        }

        /// <summary>
        /// 获取属性/字段的值
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="memberInfo">属性/字段信息</param>
        public static object GetMemberValue<T>(T entity, MemberInfo memberInfo)
            where T : class
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                if ((propertyInfo.ReflectedType ?? propertyInfo.DeclaringType).IsInterface)
                    propertyInfo = FindPropertyInfo(typeof(T), propertyInfo.Name) ?? propertyInfo;
                MethodInfo getMethodInfo = propertyInfo.GetGetMethod(true);
                if (getMethodInfo == null)
                    throw new InvalidOperationException(String.Format("不存在 {0} 属性 {1} 的get访问器", (propertyInfo.ReflectedType ?? propertyInfo.DeclaringType).FullName, propertyInfo.Name));
                return getMethodInfo.IsStatic
                    ? propertyInfo.GetValue(null)
                    : entity != null
                        ? propertyInfo.GetValue(entity)
                        : null;
            }

            if (memberInfo is FieldInfo fieldInfo)
            {
                if ((fieldInfo.ReflectedType ?? fieldInfo.DeclaringType).IsInterface)
                    fieldInfo = FindFieldInfo(typeof(T), fieldInfo.Name) ?? fieldInfo;
                return fieldInfo.IsStatic
                    ? fieldInfo.GetValue(null)
                    : entity != null
                        ? fieldInfo.GetValue(entity)
                        : null;
            }

            return null;
        }

        #endregion

        #region Expression

        /// <summary>
        /// 检索类属性的信息
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <exception cref="ArgumentNullException">lambda</exception>
        /// <exception cref="ArgumentException">不存在成员访问</exception>
        /// <returns>类属性</returns>
        public static PropertyInfo GetPropertyInfo<T>(LambdaExpression propertyLambda, bool throwIfNotFound = true)
            where T : class
        {
            PropertyInfo result = GetMemberInfo<T>(propertyLambda, throwIfNotFound) as PropertyInfo;
            if (result != null)
                return result;

            if (throwIfNotFound)
                throw new InvalidOperationException("不存在属性访问");
            return null;
        }

        /// <summary>
        /// 检索类属性的信息
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <exception cref="ArgumentNullException">lambda</exception>
        /// <exception cref="ArgumentException">不存在成员访问</exception>
        /// <returns>类属性</returns>
        public static PropertyInfo GetPropertyInfo(LambdaExpression propertyLambda, bool throwIfNotFound = true)
        {
            PropertyInfo result = GetMemberInfo(propertyLambda, throwIfNotFound) as PropertyInfo;
            if (result != null)
                return result;

            if (throwIfNotFound)
                throw new InvalidOperationException("不存在属性访问");
            return null;
        }

        /// <summary>
        /// 检索类字段的信息
        /// </summary>
        /// <param name="fieldLambda">含类字段的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <exception cref="ArgumentNullException">lambda</exception>
        /// <exception cref="ArgumentException">不存在成员访问</exception>
        /// <returns>类属性</returns>
        public static FieldInfo GetFieldInfo<T>(LambdaExpression fieldLambda, bool throwIfNotFound = true)
            where T : class
        {
            FieldInfo result = GetMemberInfo<T>(fieldLambda, throwIfNotFound) as FieldInfo;
            if (result != null)
                return result;

            if (throwIfNotFound)
                throw new InvalidOperationException("不存在字段访问");
            return null;
        }

        /// <summary>
        /// 检索类字段的信息
        /// </summary>
        /// <param name="fieldLambda">含类字段的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <exception cref="ArgumentNullException">lambda</exception>
        /// <exception cref="ArgumentException">不存在成员访问</exception>
        /// <returns>类属性</returns>
        public static FieldInfo GetFieldInfo(LambdaExpression fieldLambda, bool throwIfNotFound = true)
        {
            FieldInfo result = GetMemberInfo(fieldLambda, throwIfNotFound) as FieldInfo;
            if (result != null)
                return result;

            if (throwIfNotFound)
                throw new InvalidOperationException("不存在字段访问");
            return null;
        }

        /// <summary>
        /// 检索类属性/类字段的信息
        /// </summary>
        /// <param name="memberLambda">含类属性/类字段的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <exception cref="ArgumentNullException">lambda</exception>
        /// <exception cref="ArgumentException">不存在成员访问</exception>
        /// <returns>类属性/类字段</returns>
        public static MemberInfo GetMemberInfo<T>(LambdaExpression memberLambda, bool throwIfNotFound = true)
            where T : class
        {
            MemberInfo result = GetMemberInfo(memberLambda, throwIfNotFound);
            if (result != null && (result.ReflectedType ?? result.DeclaringType).IsInterface)
                return FindMemberInfo(typeof(T), result.Name, throwIfNotFound) ?? result;
            return result;
        }

        /// <summary>
        /// 检索类属性/类字段的信息
        /// </summary>
        /// <param name="memberLambda">含类属性/类字段的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <exception cref="ArgumentNullException">lambda</exception>
        /// <exception cref="ArgumentException">不存在成员访问</exception>
        /// <returns>类属性/类字段</returns>
        public static MemberInfo GetMemberInfo(LambdaExpression memberLambda, bool throwIfNotFound = true)
        {
            if (memberLambda == null)
                throw new ArgumentNullException(nameof(memberLambda));

            MemberExpression memberExpr = null;
            if (memberLambda.Body.NodeType == ExpressionType.Convert)
                memberExpr = ((UnaryExpression) memberLambda.Body).Operand as MemberExpression;
            else if (memberLambda.Body.NodeType == ExpressionType.MemberAccess)
                memberExpr = memberLambda.Body as MemberExpression;
            if (memberExpr != null)
                return memberExpr.Member;

            if (throwIfNotFound)
                throw new InvalidOperationException("不存在成员访问");
            return null;
        }

        /// <summary>
        /// 检索类方法的信息
        /// </summary>
        /// <param name="methodLambda">含类方法的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <exception cref="ArgumentNullException">lambda</exception>
        /// <exception cref="ArgumentException">不存在方法调用</exception>
        /// <returns>类方法</returns>
        public static MethodInfo GetMethodInfo(LambdaExpression methodLambda, bool throwIfNotFound = true)
        {
            if (methodLambda == null)
                throw new ArgumentNullException(nameof(methodLambda));

            if (methodLambda.Body.NodeType == ExpressionType.Call)
                return ((MethodCallExpression) methodLambda.Body).Method;

            if (throwIfNotFound)
                throw new InvalidOperationException("不存在方法调用");
            return null;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="instance">实例</param>
        /// <returns>序列化文本(XML格式)</returns>
        public static string XmlSerialize(object instance)
        {
            if (instance == null)
                return null;
            if (instance is string s)
                return s;

            XmlSerializer serializer = new XmlSerializer(instance.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, instance);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializedText">序列化文本(XML格式)</param>
        /// <returns>对象</returns>
        public static T XmlDeserialize<T>(string serializedText)
        {
            return (T) XmlDeserialize(serializedText, typeof(T));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializedText">序列化文本(XML格式)</param>
        /// <param name="objectType">对象类型</param>
        /// <returns>对象</returns>
        public static object XmlDeserialize(string serializedText, Type objectType)
        {
            if (objectType == typeof(string))
                return serializedText;
            if (objectType.IsValueType)
                return ChangeType(serializedText, objectType);
            if (String.IsNullOrEmpty(serializedText))
                return null;

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedText)))
            {
                XmlSerializer serializer = new XmlSerializer(objectType);
                return serializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="instance">实例</param>
        /// <returns>序列化文本(JSON格式)</returns>
        public static string JsonSerialize(object instance)
        {
            if (instance == null)
                return null;
            if (instance is string s)
                return s;
            if (instance is IDataReader dataReader)
                return JsonSerialize(dataReader);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Newtonsoft.Json.Converters.IsoDateTimeConverter timeConverter = new Newtonsoft.Json.Converters.IsoDateTimeConverter();
            timeConverter.DateTimeFormat = JsonDateFormatString;
            settings.Converters.Add(timeConverter);
            return JsonConvert.SerializeObject(instance, settings);
        }

        /// <summary>
        /// 序列化(属性名为表/视图的字段名/别名)
        /// </summary>
        /// <param name="reader">IDataReader</param>
        /// <param name="first">是否返回第一条记录</param>
        /// <returns>序列化文本(JSON格式)</returns>
        public static string JsonSerialize(IDataReader reader, bool first = false)
        {
            StringBuilder result = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(result, CultureInfo.InvariantCulture))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.DateFormatString = JsonDateFormatString;
                jsonWriter.Formatting = Formatting.None;
                if (!first)
                    jsonWriter.WriteStartArray();

                while (reader.Read())
                {
                    jsonWriter.WriteStartObject();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        jsonWriter.WritePropertyName(reader.GetName(i));
                        jsonWriter.WriteValue(reader.GetValue(i));
                    }

                    jsonWriter.WriteEndObject();
                    if (first)
                        break;
                }

                if (!first)
                    jsonWriter.WriteEndArray();
                jsonWriter.Flush();
            }

            return result.ToString();
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializedText">序列化文本(JSON格式)</param>
        /// <returns>对象</returns>
        public static T JsonDeserialize<T>(string serializedText)
        {
            return (T) JsonDeserialize(serializedText, typeof(T));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializedText">序列化文本(JSON格式)</param>
        /// <param name="objectType">对象类型</param>
        /// <returns>对象</returns>
        public static object JsonDeserialize(string serializedText, Type objectType)
        {
            if (objectType == typeof(string))
                return serializedText;
            if (objectType.IsValueType)
                return ChangeType(serializedText, objectType);
            return !String.IsNullOrEmpty(serializedText) ? JsonConvert.DeserializeObject(serializedText, objectType) : null;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializedText">序列化文本(JSON格式)</param>
        /// <returns>对象PropertyName-Value字典</returns>
        public static IDictionary<string, object> JsonDeserialize(string serializedText)
        {
            if (String.IsNullOrEmpty(serializedText))
                return null;
            using (StringReader stringReader = new StringReader(serializedText))
            using (JsonTextReader reader = new JsonTextReader(stringReader))
            {
                reader.DateFormatString = JsonDateFormatString;
                reader.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                return JsonDeserialize(reader);
            }
        }

        private static IDictionary<string, object> JsonDeserialize(JsonTextReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.Ordinal);
            while (reader.Read())
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value.ToString();
                    reader.Read();
                    switch (reader.TokenType)
                    {
                        case JsonToken.StartArray:
                            result.Add(propertyName, JsonDeserializeList(reader));
                            break;
                        case JsonToken.StartObject:
                            result.Add(propertyName, JsonDeserialize(reader));
                            break;
                        default:
                            result.Add(propertyName, reader.Value);
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;

            return result;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializedText">序列化文本(JSON格式)</param>
        /// <returns>对象PropertyName-Value字典List</returns>
        public static IList<IDictionary<string, object>> JsonDeserializeList(string serializedText)
        {
            if (String.IsNullOrEmpty(serializedText))
                return null;
            using (StringReader stringReader = new StringReader(serializedText))
            using (JsonTextReader reader = new JsonTextReader(stringReader))
            {
                reader.DateFormatString = JsonDateFormatString;
                reader.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                return JsonDeserializeList(reader);
            }
        }

        private static IList<IDictionary<string, object>> JsonDeserializeList(JsonTextReader reader)
        {
            List<IDictionary<string, object>> result = new List<IDictionary<string, object>>();
            while (reader.Read())
                if (reader.TokenType == JsonToken.StartObject)
                    result.Add(JsonDeserialize(reader));
                else if (reader.TokenType == JsonToken.EndArray)
                    break;

            return result;
        }

        #endregion

        #region Object

        /// <summary>
        /// 是零
        /// </summary>
        /// <param name="value">值</param>
        public static bool IsZero(string value)
        {
            return new Regex(@"^[0]+$|^[0]+(\.)?[0]+$").IsMatch(value);
        }

        /// <summary>
        /// 是数字
        /// </summary>
        /// <param name="value">值</param>
        public static bool IsNumeric(string value)
        {
            return new Regex(@"^\d+$|^\d+(\.)?\d+$").IsMatch(value);
        }

        /// <summary>
        /// 是整数
        /// </summary>
        /// <param name="value">值</param>
        public static bool IsInteger(string value)
        {
            return new Regex(@"^\d+$").IsMatch(value);
        }

        /// <summary>
        /// 字符串长度
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="isUnicode">是否Unicode</param>
        public static int Length(string value, bool isUnicode)
        {
            if (String.IsNullOrEmpty(value))
                return 0;

            return isUnicode ? value.Length : Encoding.Default.GetByteCount(value);
        }

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="length">截取长度</param>
        /// <param name="isUnicode">是否Unicode</param>
        public static string SubString(string value, int length, bool isUnicode)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            if (isUnicode)
            {
                if (value.Length > length)
                    return value.Remove(length);
            }
            else
            {
                if (Encoding.Default.GetByteCount(value) > length)
                    return Encoding.Default.GetString(Encoding.Default.GetBytes(value), 0, length);
            }

            return value;
        }

        /// <summary>
        /// 比较List对象
        /// </summary>
        /// <param name="arrayA">对象A</param>
        /// <param name="arrayB">对象B</param>
        public static bool CompareList(IList arrayA, IList arrayB)
        {
            if (object.Equals(arrayA, arrayB))
                return true;

            if (arrayA != null && arrayB != null && arrayA.Count == arrayB.Count)
            {
                for (int i = 0; i < arrayA.Count; i++)
                    if (!object.Equals(arrayA[i], arrayB[i]))
                        return false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 填充对象字段值
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="target">目标对象</param>
        /// <param name="reset">重新设定</param>
        public static void FillFieldValues<T>(T source, T target, bool reset = true)
            where T : class
        {
            if (source == null)
                return;

            foreach (FieldInfo item in source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (reset || item.GetValue(target) == null)
                    item.SetValue(target, item.GetValue(source));
        }

        #endregion

        #region Attribute

        /// <summary>
        /// 获取成员信息的标签
        /// </summary>
        /// <typeparam name="T">Attribute</typeparam>
        /// <param name="objectType">类</param>
        /// <returns>标签</returns>
        public static T GetFirstCustomAttribute<T>(Type objectType)
            where T : Attribute
        {
            Type type = objectType;
            while (type != null)
            {
                T result = (T) Attribute.GetCustomAttribute(type, typeof(T), false);
                if (result != null)
                    return result;
                type = type.BaseType;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// 非应用系统类
        /// </summary>
        public static bool IsNotApplicationAssembly(Assembly assembly)
        {
            if (assembly == null)
                return true;

            string assemblyName = assembly.GetName().Name;
            return assembly is AssemblyBuilder ||
                   assemblyName.IndexOf("Microsoft.", StringComparison.Ordinal) == 0 ||
                   String.CompareOrdinal(assemblyName, "System") == 0 || assemblyName.IndexOf("System.", StringComparison.Ordinal) == 0 ||
                   assemblyName.IndexOf("Newtonsoft.", StringComparison.Ordinal) == 0;
        }

        /// <summary>
        /// 非应用系统类
        /// </summary>
        public static bool IsNotApplicationType(Type type)
        {
            if (type == null)
                return true;

            string typeNamespace = type.Namespace;
            return String.IsNullOrEmpty(typeNamespace) ||
                   typeNamespace.IndexOf("Microsoft.", StringComparison.Ordinal) == 0 ||
                   String.CompareOrdinal(typeNamespace, "System") == 0 || typeNamespace.IndexOf("System.", StringComparison.Ordinal) == 0 ||
                   typeNamespace.IndexOf("Newtonsoft.", StringComparison.Ordinal) == 0;
        }

        #endregion
    }
}