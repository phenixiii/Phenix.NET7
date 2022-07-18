using System;
using System.Data;
#if PgSQL
using NpgsqlTypes;
#endif
#if MsSQL
#endif
#if MySQL
using MySql.Data.MySqlClient;
#endif
#if ORA
using Oracle.ManagedDataAccess.Client;
#endif

namespace Phenix.Core.Data.Common
{
    /// <summary>
    /// 参数值
    /// </summary>
    public sealed class ParamValue
    {
        private ParamValue(string name, object value, ParameterDirection direction)
        {
            _name = name;
            _value = value;
            _direction = direction;
        }

#if PgSQL
        private ParamValue(string name, object value, NpgsqlDbType dbType, ParameterDirection direction)
#endif
#if MsSQL
        private ParamValue(string name, object value, SqlDbType dbType, ParameterDirection direction)
#endif
#if MySQL
        private ParamValue(string name, object value, MySqlDbType dbType, ParameterDirection direction)
#endif
#if ORA
        private ParamValue(string name, object value, OracleDbType dbType, ParameterDirection direction)
#endif
            : this(name, value, direction)
        {
            _dbType = dbType;
        }

        #region 工厂

        /// <summary>
        /// 输入参数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        public static ParamValue Input(string name, object value)
        {
            return new ParamValue(name, value, ParameterDirection.Input);
        }

        /// <summary>
        /// 输入输出参数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="dbType">参数类型</param>
#if PgSQL
        public static ParamValue Input(string name, object value, NpgsqlDbType dbType)
#endif
#if MsSQL
        public static ParamValue Input(string name, object value, SqlDbType dbType)
#endif
#if MySQL
        public static ParamValue Input(string name, object value, MySqlDbType dbType)
#endif
#if ORA
        public static ParamValue Input(string name, object value, OracleDbType dbType)
#endif
        {
            return new ParamValue(name, value, dbType, ParameterDirection.Input);
        }

        /// <summary>
        /// 输入输出参数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="dbType">参数类型</param>
#if PgSQL
        public static ParamValue InputOutput(string name, object value, NpgsqlDbType dbType)
#endif
#if MsSQL
        public static ParamValue InputOutput(string name, object value, SqlDbType dbType)
#endif
#if MySQL
        public static ParamValue InputOutput(string name, object value, MySqlDbType dbType)
#endif
#if ORA
        public static ParamValue InputOutput(string name, object value, OracleDbType dbType)
#endif
        {
            return new ParamValue(name, value, dbType, ParameterDirection.InputOutput);
        }

        /// <summary>
        /// 输出参数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="dbType">参数类型</param>
#if PgSQL
        public static ParamValue Output(string name, NpgsqlDbType dbType)
#endif
#if MsSQL
        public static ParamValue Output(string name, SqlDbType dbType)
#endif
#if MySQL
        public static ParamValue Output(string name, MySqlDbType dbType)
#endif
#if ORA
        public static ParamValue Output(string name, OracleDbType dbType)
#endif
        {
            return new ParamValue(name, null, dbType, ParameterDirection.Output);
        }

        /// <summary>
        /// 返回值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="dbType">参数类型</param>
#if PgSQL
        public static ParamValue ReturnValue(string name, NpgsqlDbType dbType)
#endif
#if MsSQL
        public static ParamValue ReturnValue(string name, SqlDbType dbType)
#endif
#if MySQL
        public static ParamValue ReturnValue(string name, MySqlDbType dbType)
#endif
#if ORA
        public static ParamValue ReturnValue(string name, OracleDbType dbType)
#endif
        {
            return new ParamValue(name, null, dbType, ParameterDirection.ReturnValue);
        }

        #endregion

        #region 属性

        private readonly string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        private object _value;

        /// <summary>
        /// 值
        /// </summary>
        public object Value
        {
            get { return _value; }
            internal set { _value = value == null || DBNull.Value.Equals(value) ? null : value; }
        }

#if PgSQL
        private readonly NpgsqlDbType? _dbType;

        /// <summary>
        /// 参数类型
        /// </summary>
        public NpgsqlDbType? DbType
        {
            get { return _dbType; }
        }
#endif
#if MsSQL
        private readonly SqlDbType? _dbType;

        /// <summary>
        /// 参数类型
        /// </summary>
        public SqlDbType? DbType
        {
            get { return _dbType; }
        }
#endif
#if MySQL
        private readonly MySqlDbType? _dbType;

        /// <summary>
        /// 参数类型
        /// </summary>
        public MySqlDbType? DbType
        {
            get { return _dbType; }
        }
#endif
#if ORA
        private readonly OracleDbType? _dbType;

        /// <summary>
        /// 参数类型
        /// </summary>
        public OracleDbType? DbType
        {
            get { return _dbType; }
        }
#endif

        private readonly ParameterDirection _direction;

        /// <summary>
        /// ParameterDirection
        /// </summary>
        public ParameterDirection Direction
        {
            get { return _direction; }
        }

        #endregion
    }
}