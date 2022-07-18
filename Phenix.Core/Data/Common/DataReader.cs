using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
using Phenix.Core.Reflection;

namespace Phenix.Core.Data.Common
{
    /// <summary>
    /// 数据读取器
    /// 封装 DbDataReader (自组织 Transaction/Connection+Command 的实现)
    /// </summary>
    public class DataReader : DisposableBase, IDataReader
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="worker">DbDataReader</param>
        public DataReader(DbDataReader worker)
        {
            _worker = worker ?? throw new ArgumentNullException(nameof(worker));
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="behavior">CommandBehavior</param>
        public DataReader(DbCommand command, CommandBehavior behavior = CommandBehavior.Default)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _behavior = behavior;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="paramValues">参数值</param>
        public DataReader(DbConnection connection, string sql, params ParamValue[] paramValues)
            : this(connection, sql, CommandBehavior.Default, paramValues)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="behavior">CommandBehavior</param>
        /// <param name="paramValues">参数值</param>
        public DataReader(DbConnection connection, string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _sql = sql;
            _behavior = behavior;
            _paramValues = paramValues;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="transaction">DbTransaction</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="paramValues">参数值</param>
        public DataReader(DbTransaction transaction, string sql, params ParamValue[] paramValues)
            : this(transaction, sql, CommandBehavior.Default, paramValues)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="transaction">DbTransaction</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="behavior">CommandBehavior</param>
        /// <param name="paramValues">参数值</param>
        public DataReader(DbTransaction transaction, string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _sql = sql;
            _behavior = behavior;
            _paramValues = paramValues;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="paramValues">参数值</param>
        public DataReader(Database database, string sql, params ParamValue[] paramValues)
            : this(database, sql, CommandBehavior.Default, paramValues)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="behavior">CommandBehavior</param>
        /// <param name="paramValues">参数值</param>
        public DataReader(Database database, string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            _database = database;
            _sql = sql;
            _behavior = behavior;
            _paramValues = paramValues;
        }

        #region 属性

        private readonly Database _database;

        /// <summary>
        /// 数据库入口
        /// </summary>
        public Database Database
        {
            get { return _database; }
        }

        private readonly string _sql;

        /// <summary>
        /// SQL 语句
        /// </summary>
        public string Sql
        {
            get { return _sql; }
        }

        private readonly CommandBehavior _behavior;

        /// <summary>
        /// CommandBehavior
        /// </summary>
        public CommandBehavior Behavior
        {
            get { return _behavior; }
        }

        private readonly ParamValue[] _paramValues;

        /// <summary>
        /// 参数值
        /// </summary>
        public ParamValue[] ParamValues
        {
            get { return _paramValues; }
        }

        private bool _myselfCommand;
        private DbCommand _command;

        /// <summary>
        /// DbCommand
        /// </summary>
        protected DbCommand Command
        {
            get
            {
                if (_command == null)
                {
                    _myselfCommand = true;
                    DbCommand command = Transaction != null
                        ? DbCommandHelper.CreateCommand(Transaction, _sql)
                        : DbCommandHelper.CreateCommand(Connection, _sql);
                    DbCommandHelper.CreateParameter(command, _paramValues);
                    _command = command;
                }

                return _command;
            }
        }

        private bool _myselfConnection;
        private DbConnection _connection;

        /// <summary>
        /// DbConnection
        /// </summary>
        protected DbConnection Connection
        {
            get
            {
                if (_command != null)
                    return _command.Connection;

                if (_transaction != null)
                    return _transaction.Connection;

                if (_connection == null)
                {
                    _myselfConnection = true;
                    _connection = _database.GetConnection();
                }

                return _connection;
            }
        }

        private readonly DbTransaction _transaction;

        /// <summary>
        /// DbTransaction
        /// </summary>
        protected DbTransaction Transaction
        {
            get { return _command != null ? _command.Transaction : _transaction; }
        }

        private bool _myselfWorker;
        private DbDataReader _worker;

        /// <summary>
        /// DbDataReader
        /// </summary>
        protected DbDataReader Worker
        {
            get
            {
                if (_worker == null)
                {
                    _myselfWorker = true;
                    _worker = DbCommandHelper.ExecuteReader(Command, _behavior);
                }

                return _worker;
            }
        }

        #region 封装 Worker

        /// <summary>
        /// 嵌套深度
        /// </summary>
        public int Depth
        {
            get { return _worker != null ? _worker.Depth : -1; }
        }

        /// <summary>
        /// 字段数量
        /// </summary>
        public int FieldCount
        {
            get { return _worker != null ? _worker.FieldCount : -1; }
        }

        /// <summary>
        /// 是否包含一行或多行
        /// </summary>
        public bool HasRows
        {
            get { return _worker != null && _worker.HasRows; }
        }

        /// <summary>
        /// 是否关闭
        /// </summary>
        public bool IsClosed
        {
            get { return _worker == null || _worker.IsClosed; }
        }

        /// <summary>
        /// 按照索引检索值
        /// </summary>
        /// <param name="ordinal">索引</param>
        public virtual object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }

        /// <summary>
        /// 按照字段名检索值
        /// </summary>
        /// <param name="name">字段名</param>
        public object this[string name]
        {
            get { return GetValue(name); }
        }

        /// <summary>
        /// 通过执行 SQL 语句获取插入、更改或删除的行数
        /// </summary>
        public int RecordsAffected
        {
            get { return _worker != null ? _worker.RecordsAffected : -1; }
        }

        /// <summary>
        /// 未隐藏的字段的数目
        /// </summary>
        public int VisibleFieldCount
        {
            get { return _worker != null ? _worker.VisibleFieldCount : -1; }
        }

        #endregion

        #endregion

        #region 方法

        #region DisposableBase

        /// <summary>
        /// 释放托管资源
        /// </summary>
        protected override void DisposeManagedResources()
        {
            if (_worker != null)
            {
                if (_myselfWorker)
                    _worker.Dispose();
                _worker = null;
            }

            if (_command != null)
            {
                if (_myselfCommand)
                    _command.Dispose();
                _command = null;
            }

            if (_connection != null)
            {
                if (_myselfConnection)
                    _database.PutConnection(_connection);
                _connection = null;
            }
        }

        /// <summary>
        /// 释放非托管资源
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
        }

        #endregion

        #region 封装 Worker

        IDataReader IDataRecord.GetData(int ordinal)
        {
            return Worker.GetData(ordinal);
        }

        /// <summary>
        /// 获取元数据
        /// </summary>
        public virtual DataTable GetSchemaTable()
        {
            return Worker.GetSchemaTable();
        }

        /// <summary>
        /// 读取下一条记录
        /// </summary>
        public virtual bool Read()
        {
            return Worker.Read();
        }

        /// <summary>
        /// 读取下一条记录
        /// </summary>
        public async Task<bool> ReadAsync()
        {
            return await ReadAsync(CancellationToken.None);
        }

        /// <summary>
        /// 读取下一条记录
        /// </summary>
        /// <param name="cancellationToken">取消指示，用于对应该取消操作的通知进行传播</param>
        public virtual async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return await Worker.ReadAsync(cancellationToken);
        }

        /// <summary>
        /// 前进到下一个结果集
        /// </summary>
        public virtual bool NextResult()
        {
            return Worker.NextResult();
        }

        /// <summary>
        /// 前进到下一个结果集
        /// </summary>
        public async Task<bool> NextResultAsync()
        {
            return await NextResultAsync(CancellationToken.None);
        }

        /// <summary>
        /// 前进到下一个结果集
        /// </summary>
        /// <param name="cancellationToken">取消指示，用于对应该取消操作的通知进行传播</param>
        public virtual async Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            return await Worker.NextResultAsync(cancellationToken);
        }

        /// <summary>
        /// 获取当前行的集合中的所有属性列
        /// </summary>
        /// <param name="values">要将属性列复制到的 Object 数组</param>
        public int GetValues(object[] values)
        {
            return Worker.GetValues(values);
        }

        /// <summary>
        /// 获取序号
        /// </summary>
        /// <param name="name">字段名</param>
        public int GetOrdinal(string name)
        {
            return Worker.GetOrdinal(name);
        }

        /// <summary>
        /// 获取字段名
        /// </summary>
        /// <param name="ordinal">序号</param>
        public string GetName(int ordinal)
        {
            return Worker.GetName(ordinal);
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        /// <param name="name">字段名</param>
        public string GetDataTypeName(string name)
        {
            return GetDataTypeName(GetOrdinal(name));
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual string GetDataTypeName(int ordinal)
        {
            return Worker.GetDataTypeName(ordinal);
        }

        /// <summary>
        /// 获取字段类型
        /// </summary>
        /// <param name="name">字段名</param>
        public Type GetFieldType(string name)
        {
            return GetFieldType(GetOrdinal(name));
        }

        /// <summary>
        /// 获取字段类型
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual Type GetFieldType(int ordinal)
        {
            return Worker.GetFieldType(ordinal);
        }

        /// <summary>
        /// 是否包含不存在的或已丢失的值
        /// </summary>
        /// <param name="name">字段名</param>
        public bool IsDBNull(string name)
        {
            return IsDBNull(GetOrdinal(name));
        }

        /// <summary>
        /// 是否包含不存在的或已丢失的值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual bool IsDBNull(int ordinal)
        {
            return Worker.IsDBNull(ordinal);
        }

        /// <summary>
        /// 是否包含不存在的或已丢失的值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public async Task<bool> IsDBNullAsync(int ordinal)
        {
            return await IsDBNullAsync(ordinal, CancellationToken.None);
        }

        /// <summary>
        /// 是否包含不存在的或已丢失的值
        /// </summary>
        /// <param name="ordinal">序号</param>
        /// <param name="cancellationToken">取消指示，用于对应该取消操作的通知进行传播</param>
        public virtual async Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return await Worker.IsDBNullAsync(ordinal, cancellationToken);
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <param name="name">字段名</param>
        public T GetFieldValue<T>(string name)
        {
            return GetFieldValue<T>(GetOrdinal(name));
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual T GetFieldValue<T>(int ordinal)
        {
            return Worker.GetFieldValue<T>(ordinal);
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public async Task<T> GetFieldValueAsync<T>(int ordinal)
        {
            return await GetFieldValueAsync<T>(ordinal, CancellationToken.None);
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <param name="ordinal">序号</param>
        /// <param name="cancellationToken">取消指示，用于对应该取消操作的通知进行传播</param>
        public virtual async Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            return await Worker.GetFieldValueAsync<T>(ordinal, cancellationToken);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public object GetValue(string name)
        {
            return GetValue(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual object GetValue(int ordinal)
        {
            return IsDBNull(ordinal) ? null : Worker.GetValue(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public T GetValue<T>(string name)
        {
            return (T)GetValue(GetOrdinal(name), typeof(T));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public T GetValue<T>(int ordinal)
        {
            return (T)GetValue(ordinal, typeof(T));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="resultType">返回值的类型</param>
        public object GetValue(string name, Type resultType)
        {
            return GetValue(GetOrdinal(name), resultType);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        /// <param name="resultType">返回值的类型</param>
        public object GetValue(int ordinal, Type resultType)
        {
            return IsDBNull(ordinal) ? null : Utilities.ChangeType(Worker.GetValue(ordinal), resultType);
        }

        /// <summary>
        /// 获取数据流
        /// </summary>
        /// <param name="name">字段名</param>
        public Stream GetStream(string name)
        {
            return GetStream(GetOrdinal(name));
        }

        /// <summary>
        /// 获取数据流
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual Stream GetStream(int ordinal)
        {
            return Worker.GetStream(ordinal);
        }

        /// <summary>
        /// 获取文本流
        /// </summary>
        /// <param name="name">字段名</param>
        public TextReader GetTextReader(string name)
        {
            return GetTextReader(GetOrdinal(name));
        }

        /// <summary>
        /// 获取文本流
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual TextReader GetTextReader(int ordinal)
        {
            return Worker.GetTextReader(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public string GetString(string name)
        {
            return GetString(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual string GetString(int ordinal)
        {
            return Worker.GetString(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public Guid GetGuid(string name)
        {
            return GetGuid(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual Guid GetGuid(int ordinal)
        {
            return Worker.GetGuid(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public bool GetBoolean(string name)
        {
            return GetBoolean(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual bool GetBoolean(int ordinal)
        {
            return Worker.GetBoolean(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public byte GetByte(string name)
        {
            return GetByte(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual byte GetByte(int ordinal)
        {
            return Worker.GetByte(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="dataOffset">行中的索引，从其开始读取操作</param>
        /// <param name="buffer">作为数据复制目标的缓冲区</param>
        /// <param name="bufferOffset">具有作为数据复制目标的缓冲区的索引</param>
        /// <param name="length">最多读取的字符数</param>
        public long GetBytes(string name, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return GetBytes(GetOrdinal(name), dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        /// <param name="dataOffset">行中的索引，从其开始读取操作</param>
        /// <param name="buffer">作为数据复制目标的缓冲区</param>
        /// <param name="bufferOffset">具有作为数据复制目标的缓冲区的索引</param>
        /// <param name="length">最多读取的字符数</param>
        public virtual long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return Worker.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public char GetChar(string name)
        {
            return GetChar(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual char GetChar(int ordinal)
        {
            return Worker.GetChar(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="dataOffset">行中的索引，从其开始读取操作</param>
        /// <param name="buffer">作为数据复制目标的缓冲区</param>
        /// <param name="bufferOffset">具有作为数据复制目标的缓冲区的索引</param>
        /// <param name="length">最多读取的字符数</param>
        public long GetChars(string name, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return GetChars(GetOrdinal(name), dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        /// <param name="dataOffset">行中的索引，从其开始读取操作</param>
        /// <param name="buffer">作为数据复制目标的缓冲区</param>
        /// <param name="bufferOffset">具有作为数据复制目标的缓冲区的索引</param>
        /// <param name="length">最多读取的字符数</param>
        public virtual long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return Worker.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public DateTime GetDateTime(string name)
        {
            return GetDateTime(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual DateTime GetDateTime(int ordinal)
        {
            return Worker.GetDateTime(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public decimal GetDecimal(string name)
        {
            return GetDecimal(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual decimal GetDecimal(int ordinal)
        {
            return Worker.GetDecimal(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public long GetInt64ForDecimal(string name)
        {
            return GetInt64ForDecimal(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual long GetInt64ForDecimal(int ordinal)
        {
            return (long)GetDecimal(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public bool GetBooleanForDecimal(string name)
        {
            return GetBooleanForDecimal(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual bool GetBooleanForDecimal(int ordinal)
        {
            return GetDecimal(ordinal) > 0;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public float GetFloat(string name)
        {
            return GetFloat(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual float GetFloat(int ordinal)
        {
            return Worker.GetFloat(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public double GetDouble(string name)
        {
            return GetDouble(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual double GetDouble(int ordinal)
        {
            return Worker.GetDouble(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public short GetInt16(string name)
        {
            return GetInt16(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual short GetInt16(int ordinal)
        {
            return Worker.GetInt16(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public int GetInt32(string name)
        {
            return GetInt32(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual int GetInt32(int ordinal)
        {
            return Worker.GetInt32(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name">字段名</param>
        public long GetInt64(string name)
        {
            return GetInt64(GetOrdinal(name));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public virtual long GetInt64(int ordinal)
        {
            return Worker.GetInt64(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public string GetNullableString(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetString(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public Guid? GetNullableGuid(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetGuid(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public bool? GetNullableBoolean(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetBoolean(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public byte? GetNullableByte(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetByte(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        /// <param name="dataOffset">行中的索引，从其开始读取操作</param>
        /// <param name="buffer">作为数据复制目标的缓冲区</param>
        /// <param name="bufferOffset">具有作为数据复制目标的缓冲区的索引</param>
        /// <param name="length">最多读取的字符数</param>
        public long? GetNullableBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return IsDBNull(ordinal) ? null : GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public char? GetNullableChar(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetChar(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        /// <param name="dataOffset">行中的索引，从其开始读取操作</param>
        /// <param name="buffer">作为数据复制目标的缓冲区</param>
        /// <param name="bufferOffset">具有作为数据复制目标的缓冲区的索引</param>
        /// <param name="length">最多读取的字符数</param>
        public long? GetNullableChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return IsDBNull(ordinal) ? null : GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public DateTime? GetNullableDateTime(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetDateTime(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public decimal? GetNullableDecimal(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetDecimal(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public long? GetNullableInt64ForDecimal(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetInt64ForDecimal(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public bool? GetNullableBooleanForDecimal(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetBooleanForDecimal(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public float? GetNullableFloat(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetFloat(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public double? GetNullableDouble(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetDouble(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public short? GetNullableInt16(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetInt16(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public int? GetNullableInt32(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetInt32(ordinal);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="ordinal">序号</param>
        public long? GetNullableInt64(int ordinal)
        {
            return IsDBNull(ordinal) ? null : GetInt64(ordinal);
        }

        #endregion

        #region DbParameter

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public DbParameter CreateParameter(object value)
        {
            return DbCommandHelper.CreateParameter(Command, value);
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public DbParameter CreateParameter(int? index, object value)
        {
            return DbCommandHelper.CreateParameter(Command, index, value);
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
        public DbParameter CreateParameter(string name, object value, int? index = null)
        {
            return DbCommandHelper.CreateParameter(Command, name, value, index);
        }

        /// <summary>
        /// 构建 DbParameter
        /// </summary>
#if PgSQL
        public DbParameter CreateParameter(string name, object value, NpgsqlDbType dbType, int? index = null)
#endif
#if MsSQL
        public DbParameter CreateParameter(string name, object value, SqlDbType dbType, int? index = null)
#endif
#if MySQL
        public DbParameter CreateParameter(string name, object value, MySqlDbType dbType, int? index = null)
#endif
#if ORA
        public DbParameter CreateParameter(string name, object value, OracleDbType dbType, int? index = null)
#endif
        {
            return DbCommandHelper.CreateParameter(Command, name, value, dbType, index);
        }

        #endregion

        #region Select

        /// <summary>
        /// 读取JSON记录(属性名为表/视图的字段名/别名)
        /// </summary>
        /// <param name="first">是否返回第一条记录</param>
        /// <returns>记录(JSON格式)</returns>
        public string ReadJson(bool first = false)
        {
            return Utilities.JsonSerialize(this, first);
        }

        /// <summary>
        /// 读取Dictionary记录(表/视图的字段名/别名-值)
        /// </summary>
        /// <param name="first">是否返回第一条记录</param>
        /// <returns>记录(表/视图的字段名/别名-值)</returns>
        public IList<IDictionary<string, object>> ReadDictionary(bool first = false)
        {
            IList<IDictionary<string, object>> result = new List<IDictionary<string, object>>();
            IDictionary<string, object> item = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            while (Read())
            {
                for (int i = 0; i < FieldCount; i++)
                    item.Add(GetName(i), GetValue(i));
                result.Add(item);
                if (first)
                    break;
            }

            return result;
        }

        #endregion

        #endregion
    }
}