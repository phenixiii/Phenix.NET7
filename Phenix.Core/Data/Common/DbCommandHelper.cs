using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
#if PgSQL
using Npgsql;
using NpgsqlTypes;
#endif
#if MsSQL
using System.Data.SqlClient;
#endif
#if MySQL
using MySql.Data.MySqlClient;
#endif
#if ORA
using Oracle.ManagedDataAccess.Client;
#endif
using Phenix.Core.Log;
using Phenix.Core.Reflection;

namespace Phenix.Core.Data.Common
{
    /// <summary>
    /// DbCommand ����
    /// </summary>
    public static class DbCommandHelper
    {
        #region ����

        #region Command

        /// <summary>
        /// ���� DbCommand
        /// </summary>
        public static DbCommand CreateCommand(DbConnection connection, string sql = null)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            DbCommand result = connection.CreateCommand();
#if PgSQL
#endif
#if MsSQL
#endif
#if MySQL
#endif
#if ORA
            if (result is OracleCommand oracleCommand)
            {
                oracleCommand.InitialLONGFetchSize = -1;
                oracleCommand.InitialLOBFetchSize = -1;
            }
#endif
            if (sql != null)
            {
                if (!sql.Trim(' ').Contains(' '))
                    result.CommandType = CommandType.StoredProcedure;
                result.CommandText = sql;
            }

            return result;
        }

        /// <summary>
        /// ���� DbCommand
        /// </summary>
        public static DbCommand CreateCommand(DbTransaction transaction, string sql = null)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            DbCommand result = CreateCommand(transaction.Connection, sql);
            result.Transaction = transaction;
            return result;
        }

        /// <summary>
        /// �����洢���� DbCommand
        /// </summary>
        public static DbCommand CreateStoredProc(DbConnection connection)
        {
            DbCommand result = CreateCommand(connection);
            result.CommandType = CommandType.StoredProcedure;
            return result;
        }

        /// <summary>
        /// �����洢���� DbCommand
        /// </summary>
        public static DbCommand CreateStoredProc(DbTransaction transaction)
        {
            DbCommand result = CreateCommand(transaction);
            result.CommandType = CommandType.StoredProcedure;
            return result;
        }

        /// <summary>
        /// �����洢���� DbCommand
        /// </summary>
        public static DbCommand CreateStoredProc(DbConnection connection, string storedProcedure)
        {
            DbCommand result = CreateStoredProc(connection);
            result.CommandText = storedProcedure;
            return result;
        }

        /// <summary>
        /// �����洢���� DbCommand
        /// </summary>
        public static DbCommand CreateStoredProc(DbTransaction transaction, string storedProcedure)
        {
            DbCommand result = CreateStoredProc(transaction);
            result.CommandText = storedProcedure;
            return result;
        }

        #endregion

        #region ִ��SQL���

        /// <summary>
        /// ִ�� DbCommand
        /// </summary>
        /// <returns>ִ�м�¼��</returns>
        public static int ExecuteNonQuery(DbCommand command, bool? needSaveLog = null)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                try
                {
                    DbConnectionHelper.OpenConnection(command.Connection);
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    if (needSaveLog.HasValue && needSaveLog.Value || !needSaveLog.HasValue && AppRun.Debugging)
                        Task.Run(() => EventLog.Save(String.Format("{0} consume time {1} ms", PackCommandInfo(command), DateTime.Now.Subtract(dateTime).TotalMilliseconds)));
                }
            }
            catch (Exception ex)
            {
                if (needSaveLog.HasValue && needSaveLog.Value || !needSaveLog.HasValue && AppRun.Debugging)
                    Task.Run(() => EventLog.Save(PackCommandInfo(command), ex));
                throw;
            }
        }

        /// <summary>
        /// ִ�� DbCommand
        /// </summary>
        /// <returns>ִ�м�¼��</returns>
        public static int ExecuteNonQuery(DbConnection connection, string sql, params ParamValue[] paramValues)
        {
            return ExecuteNonQuery(connection, sql, null, paramValues);
        }

        /// <summary>
        /// ִ�� DbCommand
        /// </summary>
        /// <returns>ִ�м�¼��</returns>
        public static int ExecuteNonQuery(DbConnection connection, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(connection, sql))
            {
                CreateParameter(command, paramValues);
                return ExecuteNonQuery(command, needSaveLog);
            }
        }

        /// <summary>
        /// ִ�� DbCommand
        /// </summary>
        /// <returns>ִ�м�¼��</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, string sql, params ParamValue[] paramValues)
        {
            return ExecuteNonQuery(transaction, sql, null, paramValues);
        }

        /// <summary>
        /// ִ�� DbCommand
        /// </summary>
        /// <returns>ִ�м�¼��</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(transaction, sql))
            {
                CreateParameter(command, paramValues);
                return ExecuteNonQuery(command, needSaveLog);
            }
        }

        #endregion

        #region ExecuteStoredProc

        /// <summary>
        /// ִ�д洢����
        /// </summary>
        /// <returns>�����(������-����ֵ)</returns>
        public static IDictionary<string, object> ExecuteStoredProc(DbConnection connection, string storedProcedure, params ParamValue[] paramValues)
        {
            return ExecuteStoredProc(connection, storedProcedure, null, paramValues);
        }

        /// <summary>
        /// ִ�д洢����
        /// </summary>
        /// <returns>�����(������-����ֵ)</returns>
        public static IDictionary<string, object> ExecuteStoredProc(DbConnection connection, string storedProcedure, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateStoredProc(connection, storedProcedure))
            {
                CreateParameter(command, paramValues);
                ExecuteNonQuery(command, needSaveLog);
                return PickupResults(command, paramValues);
            }
        }

        /// <summary>
        /// ִ�д洢����
        /// </summary>
        /// <returns>�����(������-����ֵ)</returns>
        public static IDictionary<string, object> ExecuteStoredProc(DbTransaction transaction, string storedProcedure, params ParamValue[] paramValues)
        {
            return ExecuteStoredProc(transaction, storedProcedure, null, paramValues);
        }

        /// <summary>
        /// ִ�д洢����
        /// </summary>
        /// <returns>�����(������-����ֵ)</returns>
        public static IDictionary<string, object> ExecuteStoredProc(DbTransaction transaction, string storedProcedure, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateStoredProc(transaction, storedProcedure))
            {
                CreateParameter(command, paramValues);
                ExecuteNonQuery(command, needSaveLog);
                return PickupResults(command, paramValues);
            }
        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// ִ�в�ѯ�������ز�ѯ�����صĽ�����е�һ�еĵ�һ��
        /// </summary>
        /// <returns>����ֵ</returns>
        public static object ExecuteScalar(DbCommand command, bool? needSaveLog = null)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                try
                {
                    DbConnectionHelper.OpenConnection(command.Connection);
                    object result = command.ExecuteScalar();
                    return result == null || DBNull.Value.Equals(result) ? null : result;
                }
                finally
                {
                    if (needSaveLog.HasValue && needSaveLog.Value || !needSaveLog.HasValue && AppRun.Debugging)
                        Task.Run(() => EventLog.Save(String.Format("{0} consume time {1} ms", PackCommandInfo(command), DateTime.Now.Subtract(dateTime).TotalMilliseconds)));
                }
            }
            catch (Exception ex)
            {
                if (needSaveLog.HasValue && needSaveLog.Value || !needSaveLog.HasValue && AppRun.Debugging)
                    Task.Run(() => EventLog.Save(PackCommandInfo(command), ex));
                throw;
            }
        }

        /// <summary>
        /// ִ�в�ѯ�������ز�ѯ�����صĽ�����е�һ�еĵ�һ��
        /// </summary>
        /// <returns>����ֵ</returns>
        public static object ExecuteScalar(DbConnection connection, string sql, params ParamValue[] paramValues)
        {
            return ExecuteScalar(connection, sql, null, paramValues);
        }

        /// <summary>
        /// ִ�в�ѯ�������ز�ѯ�����صĽ�����е�һ�еĵ�һ��
        /// </summary>
        /// <returns>����ֵ</returns>
        public static object ExecuteScalar(DbConnection connection, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(connection, sql))
            {
                CreateParameter(command, paramValues);
                return ExecuteScalar(command, needSaveLog);
            }
        }

        /// <summary>
        /// ִ�в�ѯ�������ز�ѯ�����صĽ�����е�һ�еĵ�һ��
        /// </summary>
        /// <returns>����ֵ</returns>
        public static object ExecuteScalar(DbTransaction transaction, string sql, params ParamValue[] paramValues)
        {
            return ExecuteScalar(transaction, sql, null, paramValues);
        }

        /// <summary>
        /// ִ�в�ѯ�������ز�ѯ�����صĽ�����е�һ�еĵ�һ��
        /// </summary>
        /// <returns>����ֵ</returns>
        public static object ExecuteScalar(DbTransaction transaction, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(transaction, sql))
            {
                CreateParameter(command, paramValues);
                return ExecuteScalar(command, needSaveLog);
            }
        }

        #endregion

        #region ExecuteReader

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbCommand command, bool? needSaveLog)
        {
            return ExecuteReader(command, CommandBehavior.Default, needSaveLog);
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbCommand command, CommandBehavior behavior = CommandBehavior.Default, bool? needSaveLog = null)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                try
                {
                    DbConnectionHelper.OpenConnection(command.Connection);
                    return command.ExecuteReader(behavior);
                }
                finally
                {
                    if (needSaveLog.HasValue && needSaveLog.Value || !needSaveLog.HasValue && AppRun.Debugging)
                        Task.Run(() => EventLog.Save(String.Format("{0} consume time {1} ms", PackCommandInfo(command), DateTime.Now.Subtract(dateTime).TotalMilliseconds)));
                }
            }
            catch (Exception ex)
            {
                if (needSaveLog.HasValue && needSaveLog.Value || !needSaveLog.HasValue && AppRun.Debugging)
                    Task.Run(() => EventLog.Save(PackCommandInfo(command), ex));
                throw;
            }
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbConnection connection, string sql, params ParamValue[] paramValues)
        {
            return ExecuteReader(connection, sql, CommandBehavior.Default, null, paramValues);
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbConnection connection, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            return ExecuteReader(connection, sql, CommandBehavior.Default, needSaveLog, paramValues);
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbConnection connection, string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            return ExecuteReader(connection, sql, behavior, null, paramValues);
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbConnection connection, string sql, CommandBehavior behavior, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(connection, sql))
            {
                CreateParameter(command, paramValues);
                return ExecuteReader(command, behavior, needSaveLog);
            }
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string sql, params ParamValue[] paramValues)
        {
            return ExecuteReader(transaction, sql, CommandBehavior.Default, null, paramValues);
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            return ExecuteReader(transaction, sql, CommandBehavior.Default, needSaveLog, paramValues);
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            return ExecuteReader(transaction, sql, behavior, null, paramValues);
        }

        /// <summary>
        /// ִ�в�ѯ
        /// </summary>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string sql, CommandBehavior behavior, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(transaction, sql))
            {
                CreateParameter(command, paramValues);
                return ExecuteReader(command, behavior, needSaveLog);
            }
        }

        #endregion

        #region FillDataSet

        /// <summary>
        /// ��� DataSet
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="needSaveLog">�Ƿ��¼��־</param>
        public static DataSet FillDataSet(DbCommand command, bool? needSaveLog = null)
        {
#if PgSQL
            using (DbDataAdapter adapter = new NpgsqlDataAdapter())
#endif
#if MsSQL
            using (DbDataAdapter adapter = new SqlDataAdapter())
#endif
#if MySQL
            using (DbDataAdapter adapter = new MySqlDataAdapter())
#endif
#if ORA
            using (DbDataAdapter adapter = new OracleDataAdapter())
#endif
            {
                adapter.SelectCommand = command;
                DateTime dateTime = DateTime.Now;
                try
                {
                    try
                    {
                        DbConnectionHelper.OpenConnection(command.Connection);
                        DataSet result = new DataSet();
                        adapter.Fill(result);
                        return result;
                    }
                    finally
                    {
                        if (needSaveLog.HasValue && needSaveLog.Value || !needSaveLog.HasValue && AppRun.Debugging)
                            Task.Run(() => EventLog.Save(String.Format("{0} consume time {1} ms", PackCommandInfo(command), DateTime.Now.Subtract(dateTime).TotalMilliseconds)));
                    }
                }
                catch (Exception ex)
                {
                    if (needSaveLog.HasValue && needSaveLog.Value || !needSaveLog.HasValue && AppRun.Debugging)
                        Task.Run(() => EventLog.Save(PackCommandInfo(command), ex));
                    throw;
                }
            }
        }

        /// <summary>
        /// ��� DataSet
        /// </summary>
        public static DataSet FillDataSet(DbConnection connection, string sql, params ParamValue[] paramValues)
        {
            return FillDataSet(connection, sql, null, paramValues);
        }

        /// <summary>
        /// ��� DataSet
        /// </summary>
        public static DataSet FillDataSet(DbConnection connection, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(connection, sql))
            {
                CreateParameter(command, paramValues);
                return FillDataSet(command, needSaveLog);
            }
        }

        /// <summary>
        /// ��� DataSet
        /// </summary>
        public static DataSet FillDataSet(DbTransaction transaction, string sql, params ParamValue[] paramValues)
        {
            return FillDataSet(transaction, sql, null, paramValues);
        }

        /// <summary>
        /// ��� DataSet
        /// </summary>
        public static DataSet FillDataSet(DbTransaction transaction, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(transaction, sql))
            {
                CreateParameter(command, paramValues);
                return FillDataSet(command, needSaveLog);
            }
        }

        #endregion

        #region DbParameter

        private static DbParameter CreateParameter(DbCommand command, string name, int? index)
        {
#if PgSQL
            if (name.IndexOf('@') != 0)
                name = "@" + name;
#endif
#if MsSQL
            if (name.IndexOf('@') != 0)
                name = "@" + name;
#endif
#if MySQL
            if (name.IndexOf('?') != 0)
                name = "?" + name;
#endif
#if ORA
            if (name.IndexOf(':') != 0)
                name = ":" + name;
#endif
            if (command.Parameters.Contains(name))
                return command.Parameters[name];

            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            if (index.HasValue)
                command.Parameters.Insert(index.Value, parameter);
            else
                command.Parameters.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, object value)
        {
            DbParameter result = CreateParameter(command, SqlHelper.UniqueParameterName(), null);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, int? index, object value)
        {
            DbParameter result = CreateParameter(command, SqlHelper.UniqueParameterName(), index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, int? index = null)
        {
            DbParameter result = CreateParameter(command, name, index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }

#if PgSQL
        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, NpgsqlDbType dbType, ParameterDirection direction)
        {
            return CreateParameter(command, name, dbType, direction, null);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, NpgsqlDbType dbType,ParameterDirection direction, int? index)
        {
            if (!(command is NpgsqlCommand))
                throw new ArgumentException("Ӧ���� NpgsqlCommand ����", nameof(command));

            NpgsqlParameter result = (NpgsqlParameter) CreateParameter(command, name, index);
            result.NpgsqlDbType = dbType;
            result.Direction = direction;
            return result;
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, NpgsqlDbType dbType, int? index)
        {
            return CreateParameter(command, name, value, dbType, ParameterDirection.Input, index);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, NpgsqlDbType dbType, ParameterDirection direction = ParameterDirection.Input, int? index = null)
        {
            DbParameter result = CreateParameter(command, name, dbType, direction, index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }
#endif
#if MsSQL
        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, SqlDbType dbType, ParameterDirection direction)
        {
            return CreateParameter(command, name, dbType, direction, null);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, SqlDbType dbType,ParameterDirection direction, int? index)
        {
            if (!(command is SqlCommand))
                throw new ArgumentException("Ӧ���� SqlCommand ����", nameof(command));

            SqlParameter result = (SqlParameter) CreateParameter(command, name, index);
            result.SqlDbType = dbType;
            result.Direction = direction;
            return result;
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, SqlDbType dbType, int? index)
        {
            return CreateParameter(command, name, value, dbType, ParameterDirection.Input, index);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, SqlDbType dbType, ParameterDirection direction = ParameterDirection.Input, int? index = null)
        {
            DbParameter result = CreateParameter(command, name, dbType, direction, index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }
#endif
#if MySQL
        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, MySqlDbType dbType, ParameterDirection direction)
        {
            return CreateParameter(command, name, dbType, direction, null);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, MySqlDbType dbType,ParameterDirection direction, int? index)
        {
            if (!(command is MySqlCommand))
                throw new ArgumentException("Ӧ���� MySqlCommand ����", nameof(command));

            MySqlParameter result = (MySqlParameter) CreateParameter(command, name, index);
            result.MySqlDbType = dbType;
            result.Direction = direction;
            return result;
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, MySqlDbType dbType, int? index)
        {
            return CreateParameter(command, name, value, dbType, ParameterDirection.Input, index);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, MySqlDbType dbType, ParameterDirection direction = ParameterDirection.Input, int? index = null)
        {
            DbParameter result = CreateParameter(command, name, dbType, direction, index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }
#endif
#if ORA
        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, OracleDbType dbType, ParameterDirection direction)
        {
            return CreateParameter(command, name, dbType, direction, null);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, OracleDbType dbType, ParameterDirection direction, int? index)
        {
            if (!(command is OracleCommand))
                throw new ArgumentException("Ӧ���� OracleCommand ����", nameof(command));

            OracleParameter result = (OracleParameter) CreateParameter(command, name, index);
            result.OracleDbType = dbType;
            result.Direction = direction;
            if (direction != ParameterDirection.Input)
                if (dbType == OracleDbType.Char || dbType == OracleDbType.Clob || dbType == OracleDbType.Varchar2 || dbType == OracleDbType.Long ||
                    dbType == OracleDbType.NChar || dbType == OracleDbType.NClob || dbType == OracleDbType.NVarchar2)
                    result.Size = UInt16.MaxValue;
            return result;
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, OracleDbType dbType, int? index)
        {
            return CreateParameter(command, name, value, dbType, ParameterDirection.Input, index);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, OracleDbType dbType, ParameterDirection direction = ParameterDirection.Input, int? index = null)
        {
            DbParameter result = CreateParameter(command, name, dbType, direction, index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }
#endif

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static void CreateParameter(DbCommand command, ParamValue[] paramValues)
        {
            if (paramValues == null || paramValues.Length == 0)
                return;
            foreach (ParamValue item in paramValues)
                if (item.Direction == ParameterDirection.Input)
                    CreateParameter(command, item.Name, item.Value);
                else if (item.DbType == null)
                    CreateParameter(command, item.Name, item.Value);
                else
                    CreateParameter(command, item.Name, item.Value, item.DbType.Value, item.Direction);
        }

        /// <summary>
        /// ���� DbParameter
        /// </summary>
        public static void CreateParameter(DbCommand command, IDictionary<string, object> paramValues)
        {
            if (paramValues == null || paramValues.Count == 0)
                return;
            foreach (KeyValuePair<string, object> kvp in paramValues)
                CreateParameter(command, kvp.Key, kvp.Value);
        }

        private static IDictionary<string, object> PickupResults(DbCommand command, ParamValue[] paramValues)
        {
            if (paramValues == null || paramValues.Length == 0)
                return null;
            Dictionary<string, object> result = new Dictionary<string, object>(paramValues.Length);
            foreach (ParamValue item in paramValues)
                if (item.Direction != ParameterDirection.Input)
                {
                    item.Value = command.Parameters[item.Name].Value;
                    result.Add(item.Name, item.Value);
                }

            return result;
        }

        #endregion

        #region ������־

        /// <summary>
        /// ��� DbCommand ��Ϣ
        /// </summary>
        /// <returns>XML��ʽ����</returns>
        public static string PackCommandInfo(DbCommand command)
        {
            StringBuilder result = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(result, new XmlWriterSettings {ConformanceLevel = ConformanceLevel.Fragment, CheckCharacters = false}))
            {
                xmlWriter.WriteStartElement("Command");
                xmlWriter.WriteAttributeString("Text", command.CommandText);
                xmlWriter.WriteAttributeString("Timeout", command.CommandTimeout.ToString());
                xmlWriter.WriteEndElement();
                foreach (DbParameter item in command.Parameters)
                {
                    xmlWriter.WriteStartElement("Parameter");
                    xmlWriter.WriteAttributeString("Name", item.ParameterName);
                    xmlWriter.WriteAttributeString("Value", item.Value == null || DBNull.Value.Equals(item.Value) ? Standards.Null : item.Value.ToString());
                    xmlWriter.WriteEndElement();
                }
            }

            return result.ToString();
        }

        #endregion

        #endregion
    }
}