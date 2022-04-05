using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using Phenix.Common.Reflection;
using Phenix.Common.Security.Cryptography;
using Phenix.Common.SyncCollections;

namespace Phenix.Common
{
    /// <summary>
    /// Ӧ������
    /// </summary>
    public static class AppSettings
    {
        #region ����

        private static readonly object _lock = new object();

        private static readonly byte[] _key = { 211, 90, 226, 153, 182, 179, 209, 78 };
        private static readonly byte[] _iv = { 103, 169, 72, 192, 158, 246, 136, 189 };

        private static readonly SynchronizedDictionary<string, string> _localCache = new SynchronizedDictionary<string, string>(StringComparer.Ordinal);

        #endregion

        #region ����

        private static string Encrypt(string sourceText)
        {
            return Convert.ToBase64String(DesCryptoTextProvider.Encrypt(_key, _iv, sourceText));
        }

        private static string Decrypt(string cipherText)
        {
            return DesCryptoTextProvider.Decrypt(_key, _iv, Convert.FromBase64String(cipherText));
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

        #region Local

        /// <summary>
        /// ������Ϣ������Phenix.Common.db��PH7_AppSettings��
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
        /// ��ȡ��Ϣ�Ա���Phenix.Common.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
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
        /// ��ȡ��Ϣ�Ա���Phenix.Common.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        public static TValue GetLocalValue<TValue>(TValue defaultValue, bool inEncrypt = false)
        {
            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalValue(FormatCompoundKey(method), defaultValue, inEncrypt);
        }

        /// <summary>
        /// ��ȡ��Ϣ�Ա���Phenix.Common.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
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
                    // 
                }

            SaveLocalValue(key, Utilities.ChangeType<string>(defaultValue), inEncrypt);
            return defaultValue;
        }

        /// <summary>
        /// ��������ֵ������Phenix.Common.db��PH7_AppSettings��
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
            field = DoSetLocalProperty<TField, TValue>(FormatCompoundKey(method), newValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// ��������ֵ������Phenix.Common.db��PH7_AppSettings��
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
            field = DoSetLocalProperty<TField, TValue>(FormatCompoundKey(method, key), newValue, inEncrypt, allowSave);
        }

        private static TField DoSetLocalProperty<TField, TValue>(string key, TValue newValue, bool inEncrypt, bool allowSave)
        {
            if (allowSave)
                SaveLocalValue(key, Utilities.ChangeType<string>(newValue), inEncrypt);
            return Utilities.ChangeType<TField>(newValue);
        }

        /// <summary>
        /// ��ȡ����ֵ�Ա���Phenix.Common.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// key = ���÷���ȫ��
        /// </summary>
        /// <param name="field">�����ֶ�</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static TValue GetLocalProperty<TField, TValue>(ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                return Utilities.ChangeType<TValue>(field);

            MethodBase method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            return DoGetLocalProperty(FormatCompoundKey(method), ref field, defaultValue, inEncrypt, allowSave);
        }

        /// <summary>
        /// ��ȡ����ֵ�Ա���Phenix.Common.db��PH7_AppSettings��(�������ǰ�߳����Ե���Ӣ���ı�)
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="field">�����ֶ�</param>
        /// <param name="defaultValue">ȱʡֵ(��Ӣ���á�|���ָ�)</param>
        /// <param name="inEncrypt">�Ƿ����</param>
        /// <param name="allowSave">������״̬��Ϣ?</param>
        public static TValue GetLocalProperty<TField, TValue>(string key, ref TField field, TValue defaultValue, bool inEncrypt = false, bool allowSave = true)
        {
            if (!object.Equals(field, null))
                return Utilities.ChangeType<TValue>(field);

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
                    //
                }

            field = DoSetLocalProperty<TField, TValue>(key, defaultValue, inEncrypt, allowSave);
            return defaultValue;
        }

        #endregion

        #endregion
    }
}