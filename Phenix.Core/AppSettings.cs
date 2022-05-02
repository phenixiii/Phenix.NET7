using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Threading.Tasks;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Log;
using Phenix.Core.Reflection;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.SyncCollections;

namespace Phenix.Core
{
    /// <summary>
    /// 应用配置
    /// 由Database.Default的PH7_AppSettings表存储配置
    /// </summary>
    public static class AppSettings
    {
        #region 属性

        private static readonly object _lock = new object();

        private static readonly byte[] _key = {211, 90, 226, 153, 182, 179, 209, 78};
        private static readonly byte[] _iv = {103, 169, 72, 192, 158, 246, 136, 189};

        #region 数据源

        /// <summary>
        /// 数据库入口
        /// </summary>
        public static Database Database
        {
            get { return Database.Default; }
        }

        #endregion

        private static readonly SynchronizedDictionary<string, string> _cache = new SynchronizedDictionary<string, string>(StringComparer.Ordinal);
        private static readonly SynchronizedDictionary<string, string> _localCache = new SynchronizedDictionary<string, string>(StringComparer.Ordinal);

        #endregion

        #region 方法

        private static string Encrypt(string sourceText)
        {
            return Convert.ToBase64String(DESCryptoTextProvider.Encrypt(_key, _iv, sourceText));
        }

        private static string Decrypt(string cipherText)
        {
            return DESCryptoTextProvider.Decrypt(_key, _iv, Convert.FromBase64String(cipherText));
        }

        private static bool IsEquality<TField, TValue>(TField field, TValue newValue)
        {
            if (typeof(TField).IsClass && typeof(TValue).IsClass)
                return object.Equals(field, newValue);

            string fieldString = Utilities.ChangeType<string>(field);
            string newValueString = Utilities.ChangeType<string>(newValue);
            return String.CompareOrdinal(fieldString, newValueString) == 0;
        }

        private static string FormatCompoundKey(MethodBase method, string key = null)
        {
            foreach (PropertyInfo item in method.DeclaringType.GetProperties())
                if (item.SetMethod == method || item.GetMethod == method)
                    return String.Format("{0}.{1}.{2}", method.DeclaringType.FullName, item.Name, key);
            return String.Format("{0}.{1}.{2}", method.DeclaringType.FullName, method.Name, key);
        }

        /// <summary>
        /// 保存信息
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        public static void SaveValue(string key, string value, bool inEncrypt = false)
        {
            if (String.CompareOrdinal(ReadValue(key, inEncrypt), value) == 0)
                return;

            lock (_lock)
            {
#if PgSQL
                if (Database.ExecuteNonQuery(@"
update PH7_AppSettings set
  AS_Value = @AS_Value,
  AS_ValueEncrypted = @AS_ValueEncrypted
where AS_Key = @AS_Key",
#endif
#if MsSQL
                if (Database.ExecuteNonQuery(@"
update PH7_AppSettings set
  AS_Value = @AS_Value,
  AS_ValueEncrypted = @AS_ValueEncrypted
where AS_Key = @AS_Key",
#endif
#if MySQL
                if (Database.ExecuteNonQuery(@"
update PH7_AppSettings set
  AS_Value = ?AS_Value,
  AS_ValueEncrypted = ?AS_ValueEncrypted
where AS_Key = ?AS_Key",
#endif
#if ORA
                if (Database.ExecuteNonQuery(@"
update PH7_AppSettings set
  AS_Value = :AS_Value,
  AS_ValueEncrypted = :AS_ValueEncrypted
where AS_Key = :AS_Key",
#endif
                    ParamValue.Input("AS_Value", inEncrypt ? Encrypt(value) : value),
                    ParamValue.Input("AS_ValueEncrypted", inEncrypt ? 1 : 0),
                    ParamValue.Input("AS_Key", key)) == 1)
                    return;

#if PgSQL
                Database.ExecuteNonQuery(@"
insert into PH7_AppSettings
  (AS_Key, AS_Value, AS_ValueEncrypted)
values
  (@AS_Key, @AS_Value, @AS_ValueEncrypted)",
#endif
#if MsSQL
                Database.ExecuteNonQuery(@"
insert into PH7_AppSettings
  (AS_Key, AS_Value, AS_ValueEncrypted)
values
  (@AS_Key, @AS_Value, @AS_ValueEncrypted)",
#endif
#if MySQL
                Database.ExecuteNonQuery(@"
insert into PH7_AppSettings
  (AS_Key, AS_Value, AS_ValueEncrypted)
values
  (?AS_Key, ?AS_Value, ?AS_ValueEncrypted)",
#endif
#if ORA
                Database.ExecuteNonQuery(@"
insert into PH7_AppSettings
  (AS_Key, AS_Value, AS_ValueEncrypted)
values
  (:AS_Key, :AS_Value, :AS_ValueEncrypted)",
#endif
                    ParamValue.Input("AS_Key", key),
                    ParamValue.Input("AS_Value", inEncrypt ? Encrypt(value) : value),
                    ParamValue.Input("AS_ValueEncrypted", inEncrypt ? 1 : 0));
            }
        }

        /// <summary>
        /// 读取信息(分离出当前线程语言的中英文文本)
        /// Thread.CurrentThread.CurrentCulture.Name为非'zh-'时返回后半截
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="inEncrypt">是否加密</param>
        public static string ReadValue(string key, bool inEncrypt = false)
        {
            try
            {
                return _cache.GetValue(key, () =>
                {
                    string pendingValue;
#if PgSQL
                    using (DataReader reader = Database.CreateDataReader(@"
select AS_Value, AS_ValueEncrypted
from PH7_AppSettings
where AS_Key = @AS_Key",
#endif
#if MsSQL
                    using (DataReader reader = Database.CreateDataReader(@"
select AS_Value, AS_ValueEncrypted
from PH7_AppSettings
where AS_Key = @AS_Key",
#endif
#if MySQL
                    using (DataReader reader = Database.CreateDataReader(@"
select AS_Value, AS_ValueEncrypted
from PH7_AppSettings
where AS_Key = ?AS_Key",
#endif
#if ORA
                    using (DataReader reader = Database.CreateDataReader(@"
select AS_Value, AS_ValueEncrypted
from PH7_AppSettings
where AS_Key = :AS_Key",
#endif
                        CommandBehavior.SingleRow))
                    {
                        reader.CreateParameter("AS_Key", key);
                        if (reader.Read())
                            if (inEncrypt)
                                if (reader.IsDBNull(1) || reader.GetInt32(1) != 1)
                                    pendingValue = reader.GetString(0);
                                else
                                    try
                                    {
                                        return AppRun.SplitCulture(Decrypt(reader.GetString(0)));
                                    }
                                    catch (SystemException) //FormatException & CryptographicException
                                    {
                                        pendingValue = reader.GetString(0);
                                    }
                            else
                                return AppRun.SplitCulture(reader.GetString(0));
                        else
                            throw new KeyNotFoundException();
                    }

                    if (pendingValue != null)
#if PgSQL
                        Database.ExecuteNonQuery(@"
update PH7_AppSettings set
  AS_Value = @AS_Value,
  AS_ValueEncrypted = 1
where AS_Key = @AS_Key",
#endif
#if MsSQL
                        Database.ExecuteNonQuery(@"
update PH7_AppSettings set
  AS_Value = @AS_Value,
  AS_ValueEncrypted = 1
where AS_Key = @AS_Key",
#endif
#if MySQL
                        Database.ExecuteNonQuery(@"
update PH7_AppSettings set
  AS_Value = ?AS_Value,
  AS_ValueEncrypted = 1
where AS_Key = ?AS_Key",
#endif
#if ORA
                        Database.ExecuteNonQuery(@"
update PH7_AppSettings set
  AS_Value = :AS_Value,
  AS_ValueEncrypted = 1
where AS_Key = :AS_Key",
#endif
                            ParamValue.Input("AS_Value", Encrypt(pendingValue)),
                            ParamValue.Input("AS_Key", key));

                    return AppRun.SplitCulture(pendingValue);
                });
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// 读取信息(分离出当前线程语言的中英文文本)
        /// </summary>
        /// <param name="defaultValue">缺省值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        public static TValue GetValue<TValue>(TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetValue(FormatCompoundKey(method), defaultValue, inEncrypt);
        }

        /// <summary>
        /// 读取信息(分离出当前线程语言的中英文文本)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">缺省值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        public static TValue GetValue<TValue>(string key, TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetValue(FormatCompoundKey(method, key), defaultValue, inEncrypt);
        }

        private static TValue DoGetValue<TValue>(string key, TValue defaultValue, bool inEncrypt = false)
        {
            string value = ReadValue(key, inEncrypt);
            if (value != null)
                try
                {
                    return Utilities.ChangeType<TValue>(value);
                }
                catch (InvalidCastException ex)
                {
                    Task.Run(() => EventLog.Save(MethodBase.GetCurrentMethod(), value, ex));
                }

            SaveValue(key, Utilities.ChangeType<string>(defaultValue), inEncrypt);
            return defaultValue;
        }

        /// <summary>
        /// 设置属性值
        /// key = 调用方法全名
        /// </summary>
        /// <param name="field">属性字段</param>
        /// <param name="newValue">新值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        /// <param name="allowSave">允许保存状态信息?</param>
        public static void SetProperty<TField, TValue>(ref TField field, TValue newValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (IsEquality(field, newValue))
                return;

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            field = DoSetProperty<TField, TValue>(FormatCompoundKey(method), newValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="field">属性字段</param>
        /// <param name="newValue">新值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        /// <param name="allowSave">允许保存状态信息?</param>
        public static void SetProperty<TField, TValue>(string key, ref TField field, TValue newValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (IsEquality(field, newValue))
                return;

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            field = DoSetProperty<TField, TValue>(FormatCompoundKey(method, key), newValue, inEncrypt, allowSave);
        }

        private static TField DoSetProperty<TField, TValue>(string key, TValue newValue, bool inEncrypt, bool allowSave)
        {
            if (allowSave)
                SaveValue(key, Utilities.ChangeType<string>(newValue), inEncrypt);
            return Utilities.ChangeType<TField>(newValue);
        }

        /// <summary>
        /// 获取属性值(分离出当前线程语言的中英文文本)
        /// key = 调用方法全名
        /// </summary>
        /// <param name="field">属性字段</param>
        /// <param name="defaultValue">缺省值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        /// <param name="allowSave">允许保存状态信息?</param>
        public static TValue GetProperty<TField, TValue>(ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                try
                {
                    return Utilities.ChangeType<TValue>(field);
                }
                catch (InvalidCastException ex)
                {
                    TField value = field;
                    Task.Run(() => EventLog.SaveLocal(MethodBase.GetCurrentMethod(), value.ToString(), ex));
                }

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetProperty(FormatCompoundKey(method), ref field, defaultValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// 获取属性值(分离出当前线程语言的中英文文本)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="field">属性字段</param>
        /// <param name="defaultValue">缺省值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        /// <param name="allowSave">允许保存状态信息?</param>
        public static TValue GetProperty<TField, TValue>(string key, ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                try
                {
                    return Utilities.ChangeType<TValue>(field);
                }
                catch (InvalidCastException ex)
                {
                    TField value = field;
                    Task.Run(() => EventLog.SaveLocal(MethodBase.GetCurrentMethod(), value.ToString(), ex));
                }

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetProperty(FormatCompoundKey(method, key), ref field, defaultValue, inEncrypt, allowSave);
        }

        private static TValue DoGetProperty<TField, TValue>(string key, ref TField field, TValue defaultValue, bool inEncrypt, bool allowSave)
        {
            string value = ReadValue(key, inEncrypt);
            if (value != null)
                try
                {
                    field = Utilities.ChangeType<TField>(value);
                    return Utilities.ChangeType<TValue>(value);
                }
                catch (InvalidCastException ex)
                {
                    Task.Run(() => EventLog.Save(MethodBase.GetCurrentMethod(), value, ex));
                }

            field = DoSetProperty<TField, TValue>(key, defaultValue, inEncrypt, allowSave);
            return defaultValue;
        }

        #region Local

        /// <summary>
        /// 保存信息到本地Phenix.Core.db的PH7_AppSettings表
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        public static void SaveLocalValue(string key, string value, bool inEncrypt = false)
        {
            if (String.CompareOrdinal(ReadLocalValue(key, inEncrypt), value) == 0)
                return;

            lock (_lock)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + AppRun.ConfigFilePath))
                {
                    connection.Open();
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
update PH7_AppSettings set
  Value = @Value,
  ValueEncrypted = @ValueEncrypted
where Key = @Key";
                        command.Parameters.AddWithValue("@Value", inEncrypt ? Encrypt(value) : value);
                        command.Parameters.AddWithValue("@ValueEncrypted", inEncrypt ? 1 : 0);
                        command.Parameters.AddWithValue("@Key", key);
                        if (command.ExecuteNonQuery() == 1)
                            return;
                    }

                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
insert into PH7_AppSettings
  (Key, Value, ValueEncrypted)
values
  (@Key, @Value, @ValueEncrypted)";
                        command.Parameters.AddWithValue("@Key", key);
                        command.Parameters.AddWithValue("@Value", inEncrypt ? Encrypt(value) : value);
                        command.Parameters.AddWithValue("@ValueEncrypted", inEncrypt ? 1 : 0);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// 读取信息自本地Phenix.Core.db的PH7_AppSettings表(分离出当前线程语言的中英文文本)
        /// Thread.CurrentThread.CurrentCulture.Name为非'zh-'时返回后半截
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="inEncrypt">是否加密</param>
        public static string ReadLocalValue(string key, bool inEncrypt = false)
        {
            try
            {
                return _localCache.GetValue(key, () =>
                {
                    string pendingValue;
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + AppRun.ConfigFilePath))
                    {
                        connection.Open();
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
select Value, ValueEncrypted
from PH7_AppSettings
where Key = @Key";
                            command.Parameters.AddWithValue("@Key", key);
                            using (SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                if (reader.Read())
                                    if (inEncrypt)
                                        if (reader.IsDBNull(1) || reader.GetInt32(1) != 1)
                                            pendingValue = reader.GetString(0);
                                        else
                                            try
                                            {
                                                return AppRun.SplitCulture(Decrypt(reader.GetString(0)));
                                            }
                                            catch (SystemException) //FormatException & CryptographicException
                                            {
                                                pendingValue = reader.GetString(0);
                                            }
                                    else
                                        return AppRun.SplitCulture(reader.GetString(0));
                                else
                                    throw new KeyNotFoundException();
                            }
                        }

                        if (pendingValue != null)
                        {
                            using (SQLiteCommand command = connection.CreateCommand())
                            {
                                command.CommandText = @"
update PH7_AppSettings set
  Value = @Value,
  ValueEncrypted = 1
where Key = @Key";
                                command.Parameters.AddWithValue("@Value", Encrypt(pendingValue));
                                command.Parameters.AddWithValue("@Key", key);
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    return AppRun.SplitCulture(pendingValue);
                });
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// 读取信息自本地Phenix.Core.db的PH7_AppSettings表(分离出当前线程语言的中英文文本)
        /// </summary>
        /// <param name="defaultValue">缺省值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        public static TValue GetLocalValue<TValue>(TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalValue(FormatCompoundKey(method), defaultValue, inEncrypt);
        }

        /// <summary>
        /// 读取信息自本地Phenix.Core.db的PH7_AppSettings表(分离出当前线程语言的中英文文本)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">缺省值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        public static TValue GetLocalValue<TValue>(string key, TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalValue(FormatCompoundKey(method, key), defaultValue, inEncrypt);
        }

        private static TValue DoGetLocalValue<TValue>(string key, TValue defaultValue, bool inEncrypt = false)
        {
            string value = ReadLocalValue(key, inEncrypt);
            if (value != null)
                try
                {
                    return Utilities.ChangeType<TValue>(value);
                }
                catch (InvalidCastException ex)
                {
                    Task.Run(() => EventLog.Save(MethodBase.GetCurrentMethod(), value, ex));
                }

            SaveLocalValue(key, Utilities.ChangeType<string>(defaultValue), inEncrypt);
            return defaultValue;
        }

        /// <summary>
        /// 设置属性值到本地Phenix.Core.db的PH7_AppSettings表
        /// key = 调用方法全名
        /// </summary>
        /// <param name="field">属性字段</param>
        /// <param name="newValue">新值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        /// <param name="allowSave">允许保存状态信息?</param>
        public static void SetLocalProperty<TField, TValue>(ref TField field, TValue newValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (IsEquality(field, newValue))
                return;

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            field = DoSetLocalProperty<TField, TValue>(FormatCompoundKey(method), newValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// 设置属性值到本地Phenix.Core.db的PH7_AppSettings表
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="field">属性字段</param>
        /// <param name="newValue">新值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        /// <param name="allowSave">允许保存状态信息?</param>
        public static void SetLocalProperty<TField, TValue>(string key, ref TField field, TValue newValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (IsEquality(field, newValue))
                return;

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            field = DoSetLocalProperty<TField, TValue>(FormatCompoundKey(method, key), newValue, inEncrypt, allowSave);
        }

        private static TField DoSetLocalProperty<TField, TValue>(string key, TValue newValue, bool inEncrypt, bool allowSave)
        {
            if (allowSave)
                SaveLocalValue(key, Utilities.ChangeType<string>(newValue), inEncrypt);
            return Utilities.ChangeType<TField>(newValue);
        }

        /// <summary>
        /// 获取属性值自本地Phenix.Core.db的PH7_AppSettings表(分离出当前线程语言的中英文文本)
        /// key = 调用方法全名
        /// </summary>
        /// <param name="field">属性字段</param>
        /// <param name="defaultValue">缺省值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        /// <param name="allowSave">允许保存状态信息?</param>
        public static TValue GetLocalProperty<TField, TValue>(ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                try
                {
                    return Utilities.ChangeType<TValue>(field);
                }
                catch (InvalidCastException ex)
                {
                    TField value = field;
                    Task.Run(() => EventLog.SaveLocal(MethodBase.GetCurrentMethod(), value.ToString(), ex));
                }

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalProperty(FormatCompoundKey(method), ref field, defaultValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// 获取属性值自本地Phenix.Core.db的PH7_AppSettings表(分离出当前线程语言的中英文文本)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="field">属性字段</param>
        /// <param name="defaultValue">缺省值(中英文用‘|’分隔)</param>
        /// <param name="inEncrypt">是否加密</param>
        /// <param name="allowSave">允许保存状态信息?</param>
        public static TValue GetLocalProperty<TField, TValue>(string key, ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                try
                {
                    return Utilities.ChangeType<TValue>(field);
                }
                catch (InvalidCastException ex)
                {
                    TField value = field;
                    Task.Run(() => EventLog.SaveLocal(MethodBase.GetCurrentMethod(), value.ToString(), ex));
                }

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalProperty(FormatCompoundKey(method, key), ref field, defaultValue, inEncrypt, allowSave);
        }

        private static TValue DoGetLocalProperty<TField, TValue>(string key, ref TField field, TValue defaultValue, bool inEncrypt, bool allowSave)
        {
            string value = ReadLocalValue(key, inEncrypt);
            if (value != null)
                try
                {
                    field = Utilities.ChangeType<TField>(value);
                    return Utilities.ChangeType<TValue>(value);
                }
                catch (InvalidCastException ex)
                {
                    Task.Run(() => EventLog.SaveLocal(MethodBase.GetCurrentMethod(), value, ex));
                }

            field = DoSetLocalProperty<TField, TValue>(key, defaultValue, inEncrypt, allowSave);
            return defaultValue;
        }

        #endregion

        #endregion
    }
}