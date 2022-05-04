using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Phenix.Core.Data;

namespace Phenix.Core
{
    /// <summary>
    /// 应用系统执行
    /// </summary>
    public static class AppRun
    {
        #region 属性

        private static bool? _debugging;

        /// <summary>
        /// 调试中?
        /// 缺省为 false
        /// </summary>
        public static bool Debugging
        {
            get { return AppSettings.GetLocalProperty(ref _debugging, false); }
            set { AppSettings.SetLocalProperty(ref _debugging, value); }
        }

        private static string _baseDirectory;

        /// <summary>
        /// 基础目录
        /// </summary>
        public static string BaseDirectory
        {
            get { return _baseDirectory ??= AppDomain.CurrentDomain.SetupInformation.ApplicationBase; }
        }

        private static string _tempDirectory;

        /// <summary>
        /// 临时目录
        /// </summary>
        public static string TempDirectory
        {
            get
            {
                if (_tempDirectory == null)
                    _tempDirectory = Path.Combine(BaseDirectory, "TEMP");
                if (!Directory.Exists(_tempDirectory))
                    Directory.CreateDirectory(_tempDirectory);
                return _tempDirectory;
            }
        }

        private static string _configFilePath;

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string ConfigFilePath
        {
            get
            {
                if (_configFilePath == null)
                {
                    string result = Path.Combine(BaseDirectory, "Phenix.Core.db");
                    if (!File.Exists(result))
                        throw new InvalidOperationException(String.Format("不存在配置库文件: {0}", result));
                    _configFilePath = result;
                }

                return _configFilePath;
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 分离出当前线程语言的中英文文本
        /// </summary>
        /// <param name="text">用'|'分隔中英文的文本</param>
        public static string SplitCulture(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;
            string[] strings = text.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length == 0)
                return text;
            return strings.Length == 1 || Thread.CurrentThread.CurrentCulture.Name.IndexOf("zh-", StringComparison.OrdinalIgnoreCase) == 0
                ? strings[0]
                : strings[1];
        }

        #region Exception

        /// <summary>
        /// 检索错误类型
        /// </summary>
        /// <param name="error">错误</param>
        public static T FindException<T>(Exception error)
            where T : Exception
        {
            if (error == null)
                return null;
            if (String.CompareOrdinal(error.GetType().FullName, typeof(T).FullName) == 0)
                return error as T;
            return FindException<T>(error.InnerException);
        }

        /// <summary>
        /// 取错误信息
        /// </summary>
        /// <param name="error">错误</param>
        public static string GetErrorMessage(Exception error)
        {
            return error != null
                ? Debugging
                    ? error.GetType().FullName + ": " + error.Message + " -> " + GetErrorMessage(error.InnerException)
                    : error.Message
                : String.Empty;
        }

        /// <summary>
        /// 取错误提示
        /// </summary>
        /// <param name="error">错误</param>
        /// <param name="ignoreErrorTypes">忽略错误类型</param>
        public static string GetErrorHint(Exception error, IList<Type> ignoreErrorTypes)
        {
            return error != null && (ignoreErrorTypes == null || !ignoreErrorTypes.Contains(error.GetType()))
                ? Debugging
                    ? error.GetType().FullName + ": " + error.Message + Standards.CrLf + GetErrorHint(error.InnerException, ignoreErrorTypes)
                    : error.Message
                : String.Empty;
        }

        /// <summary>
        /// 取错误提示
        /// </summary>
        /// <param name="error">错误</param>
        /// <param name="ignoreErrorTypes">忽略错误类型</param>
        public static string GetErrorHint(Exception error, params Type[] ignoreErrorTypes)
        {
            return GetErrorHint(error, ignoreErrorTypes != null ? new List<Type>(ignoreErrorTypes) : null);
        }

        /// <summary>
        /// 是否致命的错误
        /// </summary>
        /// <param name="error">错误</param>
        public static bool IsFatal(Exception error)
        {
            while (error != null)
            {
                if (error is OutOfMemoryException && !(error is InsufficientMemoryException) ||
                    error is AccessViolationException ||
                    error is SEHException)
                    return true;

                // These exceptions aren't themselves fatal, but since the CLR uses them to wrap other exceptions,
                // we want to check to see whether they've been used to wrap a fatal exception.  If so, then they
                // count as fatal.
                if (error is TypeInitializationException ||
                    error is TargetInvocationException)
                    error = error.InnerException;
                else
                    break;
            }

            return false;
        }

        #endregion

        #endregion
    }
}