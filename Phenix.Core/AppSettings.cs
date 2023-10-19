using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Log;
using Phenix.Core.Reflection;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.SyncCollections;

namespace Phenix.Core
{
    /// <summary>
    /// Ӧ������
    /// ��Database.Default��PH7_AppSettings��洢����
    /// </summary>
    public static class AppSettings
    {
        #region ����

        private static readonly object _lock = new object();

        private static readonly byte[] _key = { 211, 90, 226, 153, 182, 179, 209, 78 };
        private static readonly byte[] _iv = { 103, 169, 72, 192, 158, 246, 136, 189 };

        #region ����Դ

        /// <summary>
        /// ���ݿ����
        /// </summary>
        public static Database Database
        {
            get { return Database.Default; }
        }

        #endregion

        private static readonly SynchronizedDictionary<string, string> _cache = new SynchronizedDictionary<string, string>(StringComparer.Ordinal);
        private static readonly SynchronizedDictionary<string, string> _localCache = new SynchronizedDictionary<string, string>(StringComparer.Ordinal);

        #endregion

        #region ����

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

        private static string FormatCompoundKey(MethodBase method, string key, AppSettingVersionAttribute classVersion, AppSettingVersionAttribute methodVersion, AppSettingVersionAttribute methodTypeVersion)
        {
            return String.Format("{0}.{1}.{2}{3}{4}{5}",
                method.ReflectedType.FullName, method.MemberType == MemberTypes.Property ? method.Name.Substring(4) : method.Name, key,
                classVersion != null ? String.Format(".{0}", classVersion.VersionNumber) : null,
                methodVersion != null ? String.Format(".{0}", methodVersion.VersionNumber) : null,
                methodTypeVersion != null ? String.Format(".{0}", methodTypeVersion.VersionNumber) : null);
        }

        private static string FormatCompoundKey<TValue>(MethodBase method, string key = null)
        {
            return FormatCompoundKey(method, key,
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(Utilities.GetUnderlyingType(method.ReflectedType), typeof(AppSettingVersionAttribute)) ??
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(Utilities.GetUnderlyingType(method.DeclaringType), typeof(AppSettingVersionAttribute)),
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(method, typeof(AppSettingVersionAttribute)),
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(Utilities.GetUnderlyingType(typeof(TValue)), typeof(AppSettingVersionAttribute)));
        }

        private static string FormatCompoundKey<TField, TValue>(MethodBase method, string key = null)
        {
            return FormatCompoundKey(method, key,
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(Utilities.GetUnderlyingType(method.ReflectedType), typeof(AppSettingVersionAttribute)) ??
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(Utilities.GetUnderlyingType(method.DeclaringType), typeof(AppSettingVersionAttribute)),
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(method, typeof(AppSettingVersionAttribute)),
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(Utilities.GetUnderlyingType(typeof(TField)), typeof(AppSettingVersionAttribute)) ??
                (AppSettingVersionAttribute)Attribute.GetCustomAttribute(Utilities.GetUnderlyingType(typeof(TValue)), typeof(AppSettingVersionAttribute)));
        }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="value">ֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        public static void SaveValue(string key, string value, bool inEncrypt = false)
        {
            if (String.CompareOrdinal(ReadValue(key, inEncrypt), value) == 0)
                return;

            lock (_lock)
            {
                if (Database.ExecuteNonQuery(
#if PgSQL
                        @"
update PH7_AppSettings set
  AS_Value = @AS_Value,
  AS_ValueEncrypted = @AS_ValueEncrypted
where AS_Key = @AS_Key",
#endif
#if MsSQL
                        @"
update PH7_AppSettings set
  AS_Value = @AS_Value,
  AS_ValueEncrypted = @AS_ValueEncrypted
where AS_Key = @AS_Key",
#endif
#if MySQL
                        @"
update PH7_AppSettings set
  AS_Value = ?AS_Value,
  AS_ValueEncrypted = ?AS_ValueEncrypted
where AS_Key = ?AS_Key",
#endif
#if ORA
                        @"
update PH7_AppSettings set
  AS_Value = :AS_Value,
  AS_ValueEncrypted = :AS_ValueEncrypted
where AS_Key = :AS_Key",
#endif
                        ParamValue.Input("AS_Value", inEncrypt ? Encrypt(value) : value),
                        ParamValue.Input("AS_ValueEncrypted", inEncrypt ? 1 : 0),
                        ParamValue.Input("AS_Key", key)) == 1)
                    return;

                Database.ExecuteNonQuery(
#if PgSQL
                    @"
insert into PH7_AppSettings
  (AS_Key, AS_Value, AS_ValueEncrypted)
values
  (@AS_Key, @AS_Value, @AS_ValueEncrypted)",
#endif
#if MsSQL
                    @"
insert into PH7_AppSettings
  (AS_Key, AS_Value, AS_ValueEncrypted)
values
  (@AS_Key, @AS_Value, @AS_ValueEncrypted)",
#endif
#if MySQL
                    @"
insert into PH7_AppSettings
  (AS_Key, AS_Value, AS_ValueEncrypted)
values
  (?AS_Key, ?AS_Value, ?AS_ValueEncrypted)",
#endif
#if ORA
                    @"
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
        /// ��ȡ��Ϣ(�������ǰ�߳����Ե���Ӣ���ı�)
        /// Thread.CurrentThread.CurrentCulture.NameΪ��'zh-'ʱ���غ���
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        public static string ReadValue(string key, bool inEncrypt = false)
        {
            try
            {
                return _cache.GetValue(key, () =>
                {
                    string pendingValue;
                    using (DataReader reader = Database.CreateDataReader(
#if PgSQL
                               @"
select AS_Value, AS_ValueEncrypted
from PH7_AppSettings
where AS_Key = @AS_Key",
#endif
#if MsSQL
                               @"
select AS_Value, AS_ValueEncrypted
from PH7_AppSettings
where AS_Key = @AS_Key",
#endif
#if MySQL
                               @"
select AS_Value, AS_ValueEncrypted
from PH7_AppSettings
where AS_Key = ?AS_Key",
#endif
#if ORA
                               @"
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
                        Database.ExecuteNonQuery(
#if PgSQL
                            @"
update PH7_AppSettings set
  AS_Value = @AS_Value,
  AS_ValueEncrypted = 1
where AS_Key = @AS_Key",
#endif
#if MsSQL
                            @"
update PH7_AppSettings set
  AS_Value = @AS_Value,
  AS_ValueEncrypted = 1
where AS_Key = @AS_Key",
#endif
#if MySQL
                            @"
update PH7_AppSettings set
  AS_Value = ?AS_Value,
  AS_ValueEncrypted = 1
where AS_Key = ?AS_Key",
#endif
#if ORA
                            @"
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
        /// ��ȡ��Ϣ(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        public static TValue GetValue<TValue>(TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetValue(FormatCompoundKey<TValue>(method), defaultValue, inEncrypt);
        }

        /// <summary>
        /// ��ȡ��Ϣ(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        public static TValue GetValue<TValue>(string key, TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetValue(FormatCompoundKey<TValue>(method, key), defaultValue, inEncrypt);
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
                    LogHelper.Error(ex, "{@AppSettingValue}", new { Key = key, Value = value });
                }

            SaveValue(key, Utilities.ChangeType<string>(defaultValue), inEncrypt);
            return defaultValue;
        }

        /// <summary>
        /// ��������ֵ
        /// key = ���÷���ȫ��
        /// </summary>
        /// <param name="field">�����ֶ�</param>
        /// <param name="newValue">��ֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static void SetProperty<TField, TValue>(ref TField field, TValue newValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (IsEquality(field, newValue))
                return;

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            field = DoSetProperty<TField, TValue>(FormatCompoundKey<TField, TValue>(method), newValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// ��������ֵ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="field">�����ֶ�</param>
        /// <param name="newValue">��ֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static void SetProperty<TField, TValue>(string key, ref TField field, TValue newValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (IsEquality(field, newValue))
                return;

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            field = DoSetProperty<TField, TValue>(FormatCompoundKey<TField, TValue>(method, key), newValue, inEncrypt, allowSave);
        }

        private static TField DoSetProperty<TField, TValue>(string key, TValue newValue, bool inEncrypt, bool allowSave)
        {
            if (allowSave)
                SaveValue(key, Utilities.ChangeType<string>(newValue), inEncrypt);
            return Utilities.ChangeType<TField>(newValue);
        }

        /// <summary>
        /// ��ȡ����ֵ(�������ǰ�߳����Ե���Ӣ���ı�)
        /// key = ���÷���ȫ��
        /// </summary>
        /// <param name="field">�����ֶ�</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static TValue GetProperty<TField, TValue>(ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                try
                {
                    return Utilities.ChangeType<TValue>(field);
                }
                catch (InvalidCastException)
                {
                    // ignored
                }

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetProperty(FormatCompoundKey<TField, TValue>(method), ref field, defaultValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// ��ȡ����ֵ(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="field">�����ֶ�</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static TValue GetProperty<TField, TValue>(string key, ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                try
                {
                    return Utilities.ChangeType<TValue>(field);
                }
                catch (InvalidCastException)
                {
                    // ignored
                }

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetProperty(FormatCompoundKey<TField, TValue>(method, key), ref field, defaultValue, inEncrypt, allowSave);
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
                    LogHelper.Error(ex, "{@AppSettingProperty}", new { Key = key, Value = value });
                }

            field = DoSetProperty<TField, TValue>(key, defaultValue, inEncrypt, allowSave);
            return defaultValue;
        }

        #region Local

        /// <summary>
        /// ������Ϣ������Phenix.Core.db��PH7_AppSettings��
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="value">ֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
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
        /// ��ȡ��Ϣ�Ա���Phenix.Core.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// Thread.CurrentThread.CurrentCulture.NameΪ��'zh-'ʱ���غ���
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="inEncrypt">�Ƿ����</param>
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
        /// ��ȡ��Ϣ�Ա���Phenix.Core.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        public static TValue GetLocalValue<TValue>(TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalValue(FormatCompoundKey<TValue>(method), defaultValue, inEncrypt);
        }

        /// <summary>
        /// ��ȡ��Ϣ�Ա���Phenix.Core.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        public static TValue GetLocalValue<TValue>(string key, TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalValue(FormatCompoundKey<TValue>(method, key), defaultValue, inEncrypt);
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
                    LogHelper.Error(ex, "{@AppSettingLocalValue}", new { Key = key, Value = value });
                }

            SaveLocalValue(key, Utilities.ChangeType<string>(defaultValue), inEncrypt);
            return defaultValue;
        }

        /// <summary>
        /// ��������ֵ������Phenix.Core.db��PH7_AppSettings��
        /// key = ���÷���ȫ��
        /// </summary>
        /// <param name="field">�����ֶ�</param>
        /// <param name="newValue">��ֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static void SetLocalProperty<TField, TValue>(ref TField field, TValue newValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (IsEquality(field, newValue))
                return;

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            field = DoSetLocalProperty<TField, TValue>(FormatCompoundKey<TField, TValue>(method), newValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// ��������ֵ������Phenix.Core.db��PH7_AppSettings��
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="field">�����ֶ�</param>
        /// <param name="newValue">��ֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static void SetLocalProperty<TField, TValue>(string key, ref TField field, TValue newValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (IsEquality(field, newValue))
                return;

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            field = DoSetLocalProperty<TField, TValue>(FormatCompoundKey<TField, TValue>(method, key), newValue, inEncrypt, allowSave);
        }

        private static TField DoSetLocalProperty<TField, TValue>(string key, TValue newValue, bool inEncrypt, bool allowSave)
        {
            if (allowSave)
                SaveLocalValue(key, Utilities.ChangeType<string>(newValue), inEncrypt);
            return Utilities.ChangeType<TField>(newValue);
        }

        /// <summary>
        /// ��ȡ����ֵ�Ա���Phenix.Core.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// key = ���÷���ȫ��
        /// </summary>
        /// <param name="field">�����ֶ�</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static TValue GetLocalProperty<TField, TValue>(ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                try
                {
                    return Utilities.ChangeType<TValue>(field);
                }
                catch (InvalidCastException)
                {
                    // ignored
                }

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalProperty(FormatCompoundKey<TField, TValue>(method), ref field, defaultValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// ��ȡ����ֵ�Ա���Phenix.Core.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="field">�����ֶ�</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static TValue GetLocalProperty<TField, TValue>(string key, ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                try
                {
                    return Utilities.ChangeType<TValue>(field);
                }
                catch (InvalidCastException)
                {
                    // ignored
                }

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalProperty(FormatCompoundKey<TField, TValue>(method, key), ref field, defaultValue, inEncrypt, allowSave);
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
                    LogHelper.Error(ex, "{@AppSettingLocalProperty}", new { Key = key, Value = value });
                }

            field = DoSetLocalProperty<TField, TValue>(key, defaultValue, inEncrypt, allowSave);
            return defaultValue;
        }

        #endregion

        #endregion
    }
}