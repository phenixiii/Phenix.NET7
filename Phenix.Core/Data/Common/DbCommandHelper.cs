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
    /// DbCommand 助手
    /// </summary>
    public static class DbCommandHelper
    {
        #region 方法

        #region Command

        /// <summary>
        /// 构建 DbCommand
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
        /// 构建 DbCommand
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
        /// 构建存储过程 DbCommand
        /// </summary>
        public static DbCommand CreateStoredProc(DbConnection connection)
        {
            DbCommand result = CreateCommand(connection);
            result.CommandType = CommandType.StoredProcedure;
            return result;
        }

        /// <summary>
        /// 构建存储过程 DbCommand
        /// </summary>
        public static DbCommand CreateStoredProc(DbTransaction transaction)
        {
            DbCommand result = CreateCommand(transaction);
            result.CommandType = CommandType.StoredProcedure;
            return result;
        }

        /// <summary>
        /// 构建存储过程 DbCommand
        /// </summary>
        public static DbCommand CreateStoredProc(DbConnection connection, string storedProcedure)
        {
            DbCommand result = CreateStoredProc(connection);
            result.CommandText = storedProcedure;
            return result;
        }

        /// <summary>
        /// 构建存储过程 DbCommand
        /// </summary>
        public static DbCommand CreateStoredProc(DbTransaction transaction, string storedProcedure)
        {
            DbCommand result = CreateStoredProc(transaction);
            result.CommandText = storedProcedure;
            return result;
        }

        #endregion

        #region 执行SQL语句

        /// <summary>
        /// 执行 DbCommand
        /// </summary>
        /// <returns>执行记录数</returns>
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
        /// 执行 DbCommand
        /// </summary>
        /// <returns>执行记录数</returns>
        public static int ExecuteNonQuery(DbConnection connection, string sql, params ParamValue[] paramValues)
        {
            return ExecuteNonQuery(connection, sql, null, paramValues);
        }

        /// <summary>
        /// 执行 DbCommand
        /// </summary>
        /// <returns>执行记录数</returns>
        public static int ExecuteNonQuery(DbConnection connection, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(connection, sql))
            {
                CreateParameter(command, paramValues);
                return ExecuteNonQuery(command, needSaveLog);
            }
        }

        /// <summary>
        /// 执行 DbCommand
        /// </summary>
        /// <returns>执行记录数</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, string sql, params ParamValue[] paramValues)
        {
            return ExecuteNonQuery(transaction, sql, null, paramValues);
        }

        /// <summary>
        /// 执行 DbCommand
        /// </summary>
        /// <returns>执行记录数</returns>
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
        /// 执行存储过程
        /// </summary>
        /// <returns>结果集(参数名-参数值)</returns>
        public static IDictionary<string, object> ExecuteStoredProc(DbConnection connection, string storedProcedure, params ParamValue[] paramValues)
        {
            return ExecuteStoredProc(connection, storedProcedure, null, paramValues);
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <returns>结果集(参数名-参数值)</returns>
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
        /// 执行存储过程
        /// </summary>
        /// <returns>结果集(参数名-参数值)</returns>
        public static IDictionary<string, object> ExecuteStoredProc(DbTransaction transaction, string storedProcedure, params ParamValue[] paramValues)
        {
            return ExecuteStoredProc(transaction, storedProcedure, null, paramValues);
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <returns>结果集(参数名-参数值)</returns>
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
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <returns>返回值</returns>
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
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <returns>返回值</returns>
        public static object ExecuteScalar(DbConnection connection, string sql, params ParamValue[] paramValues)
        {
            return ExecuteScalar(connection, sql, null, paramValues);
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <returns>返回值</returns>
        public static object ExecuteScalar(DbConnection connection, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            using (DbCommand command = CreateCommand(connection, sql))
            {
                CreateParameter(command, paramValues);
                return ExecuteScalar(command, needSaveLog);
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <returns>返回值</returns>
        public static object ExecuteScalar(DbTransaction transaction, string sql, params ParamValue[] paramValues)
        {
            return ExecuteScalar(transaction, sql, null, paramValues);
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <returns>返回值</returns>
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
        /// 执行查询
        /// </summary>
        public static DbDataReader ExecuteReader(DbCommand command, bool? needSaveLog)
        {
            return ExecuteReader(command, CommandBehavior.Default, needSaveLog);
        }

        /// <summary>
        /// 执行查询
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
        /// 执行查询
        /// </summary>
        public static DbDataReader ExecuteReader(DbConnection connection, string sql, params ParamValue[] paramValues)
        {
            return ExecuteReader(connection, sql, CommandBehavior.Default, null, paramValues);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public static DbDataReader ExecuteReader(DbConnection connection, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            return ExecuteReader(connection, sql, CommandBehavior.Default, needSaveLog, paramValues);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public static DbDataReader ExecuteReader(DbConnection connection, string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            return ExecuteReader(connection, sql, behavior, null, paramValues);
        }

        /// <summary>
        /// 执行查询
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
        /// 执行查询
        /// </summary>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string sql, params ParamValue[] paramValues)
        {
            return ExecuteReader(transaction, sql, CommandBehavior.Default, null, paramValues);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string sql, bool? needSaveLog, params ParamValue[] paramValues)
        {
            return ExecuteReader(transaction, sql, CommandBehavior.Default, needSaveLog, paramValues);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            return ExecuteReader(transaction, sql, behavior, null, paramValues);
        }

        /// <summary>
        /// 执行查询
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
        /// 填充 DataSet
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="needSaveLog">是否记录日志</param>
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
        /// 填充 DataSet
        /// </summary>
        public static DataSet FillDataSet(DbConnection connection, string sql, params ParamValue[] paramValues)
        {
            return FillDataSet(connection, sql, null, paramValues);
        }

        /// <summary>
        /// 填充 DataSet
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
        /// 填充 DataSet
        /// </summary>
        public static DataSet FillDataSet(DbTransaction transaction, string sql, params ParamValue[] paramValues)
        {
            return FillDataSet(transaction, sql, null, paramValues);
        }

        /// <summary>
        /// 填充 DataSet
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
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, object value)
        {
            DbParameter result = CreateParameter(command, SqlHelper.UniqueParameterName(), null);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, int? index, object value)
        {
            DbParameter result = CreateParameter(command, SqlHelper.UniqueParameterName(), index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, int? index = null)
        {
            DbParameter result = CreateParameter(command, name, index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }

#if PgSQL
        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, NpgsqlDbType dbType, ParameterDirection direction)
        {
            return CreateParameter(command, name, dbType, direction, null);
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, NpgsqlDbType dbType,ParameterDirection direction, int? index)
        {
            if (!(command is NpgsqlCommand))
                throw new ArgumentException("应该是 NpgsqlCommand 类型", nameof(command));

            NpgsqlParameter result = (NpgsqlParameter) CreateParameter(command, name, index);
            result.NpgsqlDbType = dbType;
            result.Direction = direction;
            return result;
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, NpgsqlDbType dbType, int? index)
        {
            return CreateParameter(command, name, value, dbType, ParameterDirection.Input, index);
        }

        /// <summary>
        /// 构建 DbParameter
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
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, SqlDbType dbType, ParameterDirection direction)
        {
            return CreateParameter(command, name, dbType, direction, null);
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, SqlDbType dbType,ParameterDirection direction, int? index)
        {
            if (!(command is SqlCommand))
                throw new ArgumentException("应该是 SqlCommand 类型", nameof(command));

            SqlParameter result = (SqlParameter) CreateParameter(command, name, index);
            result.SqlDbType = dbType;
            result.Direction = direction;
            return result;
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, SqlDbType dbType, int? index)
        {
            return CreateParameter(command, name, value, dbType, ParameterDirection.Input, index);
        }

        /// <summary>
        /// 构建 DbParameter
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
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, MySqlDbType dbType, ParameterDirection direction)
        {
            return CreateParameter(command, name, dbType, direction, null);
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, MySqlDbType dbType,ParameterDirection direction, int? index)
        {
            if (!(command is MySqlCommand))
                throw new ArgumentException("应该是 MySqlCommand 类型", nameof(command));

            MySqlParameter result = (MySqlParameter) CreateParameter(command, name, index);
            result.MySqlDbType = dbType;
            result.Direction = direction;
            return result;
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, MySqlDbType dbType, int? index)
        {
            return CreateParameter(command, name, value, dbType, ParameterDirection.Input, index);
        }

        /// <summary>
        /// 构建 DbParameter
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
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, OracleDbType dbType, ParameterDirection direction)
        {
            return CreateParameter(command, name, dbType, direction, null);
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, OracleDbType dbType, ParameterDirection direction, int? index)
        {
            if (!(command is OracleCommand))
                throw new ArgumentException("应该是 OracleCommand 类型", nameof(command));

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
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, OracleDbType dbType, int? index)
        {
            return CreateParameter(command, name, value, dbType, ParameterDirection.Input, index);
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public static DbParameter CreateParameter(DbCommand command, string name, object value, OracleDbType dbType, ParameterDirection direction = ParameterDirection.Input, int? index = null)
        {
            DbParameter result = CreateParameter(command, name, dbType, direction, index);
            result.Value = Utilities.ConvertToDbValue(value);
            return result;
        }
#endif

        /// <summary>
        /// 构建 DbParameter
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
        /// 构建 DbParameter
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

        #region 调试日志

        /// <summary>
        /// 打包 DbCommand 信息
        /// </summary>
        /// <returns>XML格式内容</returns>
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