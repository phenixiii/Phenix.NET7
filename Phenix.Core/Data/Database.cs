using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
#if PgSQL
using Npgsql;
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
using Phenix.Core.Data.Common;
using Phenix.Core.Log;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.SyncCollections;

namespace Phenix.Core.Data
{
    /// <summary>
    /// 数据库入口
    /// 由Phenix.Common.db的PH7_Database表提供入口配置
    /// </summary>
    public class Database
    {
        private Database(string dataSourceKey, int dataSourceSubIndex, string dataSource, int? port, string databaseName, string userId, string password,
            bool pooling, int minPoolSize, int maxPoolSize, int connectionLifetime)
        {
            _dataSourceKey = dataSourceKey;
            _dataSourceSubIndex = dataSourceSubIndex;
            _dataSource = dataSource;
            _port = port;
            _databaseName = databaseName;
            _userId = userId;
            _password = password;
            _pooling = pooling;
            _minPoolSize = minPoolSize;
            _maxPoolSize = maxPoolSize;
            _connectionLifetime = connectionLifetime;
        }

        #region 工厂

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
                    string result = Path.Combine(AppRun.BaseDirectory, "Phenix.Core.Init.db");
                    if (!File.Exists(result))
                        throw new InvalidOperationException(String.Format("不存在配置库文件: {0}", result));
                    _configFilePath = result;
                }

                return _configFilePath;
            }
        }

        private const string PublicKey = "<RSAKeyValue><Modulus>mrZHjkajnbYp4j46DD6Rq27c7xMRIB2BxVggfTS5nTMr7TbGmiQtEzQcSej5kUG2cMAvfk14EmWeaVLFuRRwjx5ZhonFbxB08EeGkiC3Hnj5Dz/p21JZQ1iyQNRLCYai7y7NC9oZ8BZs3vOdhxKzIauW8Ojgpj1w/leuaY/p6uk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const string PrivateKey = "<RSAKeyValue><Modulus>mrZHjkajnbYp4j46DD6Rq27c7xMRIB2BxVggfTS5nTMr7TbGmiQtEzQcSej5kUG2cMAvfk14EmWeaVLFuRRwjx5ZhonFbxB08EeGkiC3Hnj5Dz/p21JZQ1iyQNRLCYai7y7NC9oZ8BZs3vOdhxKzIauW8Ojgpj1w/leuaY/p6uk=</Modulus><Exponent>AQAB</Exponent><P>wZ9R/FZOPaNXGa634iIgTFMaP+SxONumnYIBQ/Kp6vlmkAZAFK9Wom+mv9orRasZrKyeKMTKcbi5ckf2kbiIbw==</P><Q>zI3lW+s5qmFvgu6lFnEeV2t+inCCpzdKR+9/fAX/7FXZ/pPE3h52sXjYrtWLk46qq+NdMpfDBSZ0ezoMkwH+Jw==</Q><DP>hp69JBLlp1lv771YwHM1vZtx/saEqaGegJipRJLtrR/mPXD7EOav95RlfbK0r2W9Hh+Of44Dq3niBiEewEZrJQ==</DP><DQ>tiGt079I1BT3hhxY7/d+bZYv0LJHEv5e7FgqKdLpwZPbixolkySKyJByVpGbOqIwAuZpyC9qnL5Vvpa8NXTzxw==</DQ><InverseQ>ZaMrBkgaa4k4alAyjFqZIln0lop9aTrC4C3+AO8uI5HQKU9Pe2GPRh7vLO/Tst08r/wQNNzMR0RFum3mGX4XVA==</InverseQ><D>gFdB2Nbkl8oryJDSWqH0+X/IK1Nn23pe0rSejv8UE0IF6IyOCASGl+2coftwPH2EuNfVDjN5rdk6piiR+UzpaaX1++QZhcLX+NjdCMm5qM8b5nT30khL6c3OIwk4pEOvaadfbRSm5hby9MIy/Y8QDMj5du3kbSEciQXquzOjZtk=</D></RSAKeyValue>";

        private static readonly SynchronizedDictionary<string, Database> _cache = new SynchronizedDictionary<string, Database>(StringComparer.Ordinal);

        /// <summary>
        /// 缺省数据源键
        /// </summary>
        public const string DefaultDataSourceKey = "*";

        private static Database _default;

        /// <summary>
        /// 缺省数据库入口
        /// </summary>
        public static Database Default
        {
            get { return _default ??= Fetch(DefaultDataSourceKey, true); }
        }

        /// <summary>
        /// 获取数据库入口
        /// </summary>
        /// <param name="dataSourceKey">数据源键(null或*代表默认入口)</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <returns>数据库入口</returns>
        public static Database Fetch(string dataSourceKey = DefaultDataSourceKey, bool throwIfNotFound = false)
        {
            if (String.IsNullOrEmpty(dataSourceKey))
                dataSourceKey = DefaultDataSourceKey;
            return _cache.GetValue(dataSourceKey, () =>
            {
                Database value = null;
                int dataSourceSubIndex = 0;
                string pendingPassword = null;
                using (SQLiteConnection initConnection = new SQLiteConnection("Data Source=" + ConfigFilePath))
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + AppRun.ConfigFilePath))
                {
                    initConnection.Open();
                    connection.Open();
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
select DataSourceSubIndex, DataSource, Port, DatabaseName, UserId, Password, PasswordEncrypted, Pooling, MinPoolSize, MaxPoolSize, ConnectionLifetime
from PH7_Database
where DataSourceKey = @DataSourceKey
order by DataSourceSubIndex";
                        command.Parameters.AddWithValue("@DataSourceKey", dataSourceKey);
                        using (SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                dataSourceSubIndex = reader.GetInt32(0);
                                if (reader.IsDBNull(6) || reader.GetInt32(6) != 1)
                                    pendingPassword = reader.GetString(5);
                                try
                                {
                                    value = new Database(dataSourceKey, dataSourceSubIndex,
                                        reader.GetString(1), reader.IsDBNull(2) ? (int?) null : reader.GetInt32(2), reader.GetString(3), reader.GetString(4), pendingPassword != null ? pendingPassword : RSACryptoTextProvider.Decrypt(PrivateKey, reader.GetString(5)),
                                        reader.IsDBNull(7) || reader.GetInt32(7) == 1, reader.IsDBNull(8) ? 0 : reader.GetInt32(8), reader.IsDBNull(9) ? 100 : reader.GetInt32(9), reader.IsDBNull(10) ? 0 : reader.GetInt32(10));
                                }
                                catch (SystemException) //FormatException & CryptographicException
                                {
                                    pendingPassword = reader.GetString(5);
                                    value = new Database(dataSourceKey, dataSourceSubIndex,
                                        reader.GetString(1), reader.IsDBNull(2) ? (int?) null : reader.GetInt32(2), reader.GetString(3), reader.GetString(4), pendingPassword,
                                        reader.IsDBNull(7) || reader.GetInt32(7) == 1, reader.IsDBNull(8) ? 0 : reader.GetInt32(8), reader.IsDBNull(9) ? 100 : reader.GetInt32(9), reader.IsDBNull(10) ? 0 : reader.GetInt32(10));
                                }
                            }
                        }
                    }

                    if (value != null)
                    {
                        if (pendingPassword != null)
                            using (SQLiteCommand command = connection.CreateCommand())
                            {
                                command.CommandText = @"
update PH7_Database set
  Password = @Password,
  PasswordEncrypted = 1
where DataSourceKey = @DataSourceKey and DataSourceSubIndex = @DataSourceSubIndex";
                                command.Parameters.AddWithValue("@Password", RSACryptoTextProvider.Encrypt(PublicKey, pendingPassword));
                                command.Parameters.AddWithValue("@DataSourceKey", dataSourceKey);
                                command.Parameters.AddWithValue("@DataSourceSubIndex", dataSourceSubIndex);
                                command.ExecuteNonQuery();
                            }

                        using (SQLiteCommand command = initConnection.CreateCommand())
                        {
#if PgSQL
                            command.CommandText = @"
select PgSQL, Caption
from PH7_ConfigLibrary";
#endif
#if MsSQL
                            command.CommandText = @"
select MsSQL, Caption
from PH7_ConfigLibrary";
#endif
#if MySQL
                            command.CommandText = @"
select MySQL, Caption
from PH7_ConfigLibrary";
#endif
#if ORA
                            command.CommandText = @"
select ORA, Caption
from PH7_ConfigLibrary";
#endif
                            using (SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult))
                            {
                                while (reader.Read())
                                    try
                                    {
                                        value.ExecuteNonQuery(reader.GetString(0));
                                    }
#if PgSQL
                                    catch (NpgsqlException ex)
                                    {
                                        if (ex.Message.IndexOf("42P07:", StringComparison.Ordinal) == 0 || //表已存在
                                            ex.Message.IndexOf("42701:", StringComparison.Ordinal) == 0) //表中已存在要添加的列
                                            continue;
                                        LogHelper.Error(ex, reader.GetString(1));
                                    }
#endif
#if MsSQL
                                    catch (SqlException ex)
                                    {
                                        if (ex.Number == 2714) //数据库中已存在名为 '%.*ls' 的对象
                                            continue;
                                        LogHelper.Error(ex, reader.GetString(1));
                                    }
#endif
#if MySQL
                                    catch (MySqlException ex)
                                    {
                                        if (ex.Message.IndexOf("Table ", StringComparison.Ordinal) == 0 || //表已存在
                                            ex.Message.IndexOf("Duplicate ", StringComparison.Ordinal) == 0) //表中已存在要添加的列
                                            continue;
                                        LogHelper.Error(ex, reader.GetString(1));
                                    }
#endif
#if ORA
                                    catch (OracleException ex)
                                    {
                                        if (ex.Message.IndexOf("ORA-00955", StringComparison.Ordinal) == 0 || //表已存在
                                            ex.Message.IndexOf("ORA-01430", StringComparison.Ordinal) == 0) //表中已存在要添加的列
                                            continue;
                                        LogHelper.Error(ex, reader.GetString(1));
                                    }
#endif
                            }
                        }
                    }
                    else if (throwIfNotFound)
                        throw new InvalidOperationException(String.Format("请先在 {0} 库文件 PH7_Database 表里添加数据库连接串配置记录，或调用 Register() 函数注册 {1} 数据库入口", AppRun.ConfigFilePath, dataSourceKey));
                }

                return value;
            });
        }

        /// <summary>
        /// 注册数据库入口
        /// </summary>
        /// <param name="dataSource">数据源</param>
        /// <param name="port">端口</param>
        /// <param name="databaseName">数据库名称</param>
        /// <param name="userId">用户ID</param>
        /// <param name="password">用户口令</param>
        /// <param name="pooling">使用连接池</param>
        /// <param name="minPoolSize">最小连接池</param>
        /// <param name="maxPoolSize">最大连接池</param>
        /// <param name="connectionLifetime">连接生命周期</param>
        /// <returns>数据库入口</returns>
        public static Database RegisterDefault(string dataSource, int? port, string databaseName, string userId, string password,
            bool pooling = true, int minPoolSize = 0, int maxPoolSize = 100, int connectionLifetime = 0)
        {
            return _cache.GetValue(DefaultDataSourceKey, () => new Database(DefaultDataSourceKey, 0, dataSource, port, databaseName, userId, password, pooling, minPoolSize, maxPoolSize, connectionLifetime), true, true);
        }

        #endregion

        #region 属性

        #region 配置项

        private static int? _clearingPoolsThresholdOfExceptionPerMinute;

        /// <summary>
        /// 清空连接池的缺陷阈值(每分钟次数)
        /// 默认：60(>=60)
        /// </summary>
        public static int ClearingPoolsThresholdOfExceptionPerMinute
        {
            get { return new[] { AppSettings.GetProperty(ref _clearingPoolsThresholdOfExceptionPerMinute, 60), 60 }.Max(); }
            set { AppSettings.SetProperty(ref _clearingPoolsThresholdOfExceptionPerMinute, new[] { value, 60 }.Max()); }
        }

        #endregion

        private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
        private long _exceptionRepetitions;
        private DateTime _differedDateTime = DateTime.Now;
        private string _differedException;

        private readonly string _dataSourceKey;

        /// <summary>
        /// 数据源键
        /// </summary>
        public string DataSourceKey
        {
            get { return _dataSourceKey; }
        }

        private readonly int _dataSourceSubIndex;

        /// <summary>
        /// 数据源子索引
        /// </summary>
        public int DataSourceSubIndex
        {
            get { return _dataSourceSubIndex; }
        }

        private readonly string _dataSource;

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource
        {
            get { return _dataSource; }
        }
        
        private readonly int? _port;

        /// <summary>
        /// 端口
        /// </summary>
        public int? Port
        {
            get { return _port; }
        }

        private readonly string _databaseName;

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DatabaseName
        {
            get { return _databaseName; }
        }

        private readonly string _userId;

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId
        {
            get { return _userId; }
        }

        private readonly string _password;

        /// <summary>
        /// Password
        /// </summary>
        public string Password
        {
            get { return _password; }
        }

        private readonly bool _pooling;

        /// <summary>
        /// 使用连接池
        /// </summary>
        public bool Pooling
        {
            get { return _pooling; }
        }

        private readonly int _minPoolSize;

        /// <summary>
        /// 最小连接池
        /// </summary>
        public int MinPoolSize
        {
            get { return _minPoolSize; }
        }

        private readonly int _maxPoolSize;

        /// <summary>
        /// 最大连接池
        /// </summary>
        public int MaxPoolSize
        {
            get { return _maxPoolSize; }
        }

        private readonly int _connectionLifetime;

        /// <summary>
        /// 连接生命周期
        /// </summary>
        public int ConnectionLifetime
        {
            get { return _connectionLifetime; }
        }

        private string _connectionString;

        /// <summary>
        /// 数据库连接串
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString ?? (_connectionString = DbConnectionHelper.BuildConnectionString(DataSource, Port, DatabaseName, UserId, Password, Pooling, MinPoolSize, MaxPoolSize, ConnectionLifetime)); }
        }

        private readonly object _lock = new object();

        private Sequence _sequence;

        /// <summary>
        /// 64位序号
        /// </summary>
        public Sequence Sequence
        {
            get
            {
                if (_sequence == null)
                    lock (_lock)
                        if (_sequence == null)
                        {
                            _sequence = new Sequence(this);
                        }

                return _sequence;
            }
        }

        private Increment _increment;

        /// <summary>
        /// 64位增量
        /// </summary>
        public Increment Increment
        {
            get
            {
                if (_increment == null)
                    lock (_lock)
                        if (_increment == null)
                        {
                            _increment = new Increment(this);
                        }

                return _increment;
            }
        }

        private ReadOnlyDictionary<int, Database> _handles;

        /// <summary>
        /// 实际操作的数据库入口(0为主库序号，1-N为分库序号)
        /// </summary>
        public IDictionary<int, Database> Handles
        {
            get
            {
                if (_handles == null)
                    lock (_lock)
                        if (_handles == null)
                        {
                            Dictionary<int, Database> result = new Dictionary<int, Database>();
                            Dictionary<int, string> pendingPasswords = new Dictionary<int, string>();
                            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + AppRun.ConfigFilePath))
                            {
                                connection.Open();
                                using (SQLiteCommand command = connection.CreateCommand())
                                {
                                    command.CommandText = @"
select DataSourceSubIndex, DataSource, Port, DatabaseName, UserId, Password, PasswordEncrypted, Pooling, MinPoolSize, MaxPoolSize, ConnectionLifetime
from PH7_Database
where DataSourceKey = @DataSourceKey
order by DataSourceSubIndex";
                                    command.Parameters.AddWithValue("@DataSourceKey", DataSourceKey);
                                    using (SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult))
                                    {
                                        int i = 0;
                                        while (reader.Read())
                                        {
                                            int dataSourceSubIndex = reader.GetInt32(0);
                                            if (i == 0)
                                            {
                                                result.Add(i, this);
                                                if (dataSourceSubIndex != DataSourceSubIndex)
                                                    break;
                                            }
                                            else
                                            {
                                                string pendingPassword = reader.IsDBNull(6) || reader.GetInt32(6) != 1 ? reader.GetString(5) : null;
                                                try
                                                {
                                                    result.Add(i, new Database(DataSourceKey, dataSourceSubIndex,
                                                        reader.GetString(1), reader.IsDBNull(2) ? (int?) null : reader.GetInt32(2), reader.GetString(3), reader.GetString(4), pendingPassword != null ? pendingPassword : RSACryptoTextProvider.Decrypt(PrivateKey, reader.GetString(5)),
                                                        reader.IsDBNull(7) || reader.GetInt32(7) == 1, reader.IsDBNull(8) ? 0 : reader.GetInt32(8), reader.IsDBNull(9) ? 100 : reader.GetInt32(9), reader.IsDBNull(10) ? 0 : reader.GetInt32(10)));
                                                }
                                                catch (SystemException) //FormatException & CryptographicException
                                                {
                                                    pendingPassword = reader.GetString(5);
                                                    result.Add(i, new Database(DataSourceKey, dataSourceSubIndex,
                                                        reader.GetString(1), reader.IsDBNull(2) ? (int?) null : reader.GetInt32(2), reader.GetString(3), reader.GetString(4), pendingPassword,
                                                        reader.IsDBNull(7) || reader.GetInt32(7) == 1, reader.IsDBNull(8) ? 0 : reader.GetInt32(8), reader.IsDBNull(9) ? 100 : reader.GetInt32(9), reader.IsDBNull(10) ? 0 : reader.GetInt32(10)));
                                                }

                                                if (pendingPassword != null)
                                                    pendingPasswords.Add(dataSourceSubIndex, pendingPassword);
                                            }

                                            i = i + 1;
                                        }
                                    }
                                }

                                if (pendingPasswords.Count > 0)
                                    using (SQLiteCommand command = connection.CreateCommand())
                                    {
                                        command.CommandText = @"
update PH7_Database set
  Password = @Password,
  PasswordEncrypted = 1
where DataSourceKey = @DataSourceKey and DataSourceSubIndex = @DataSourceSubIndex";
                                        command.Parameters.Add("@Password", DbType.String);
                                        command.Parameters.Add("@DataSourceKey", DbType.String);
                                        command.Parameters.Add("@DataSourceSubIndex", DbType.Int32);
                                        foreach (KeyValuePair<int, string> kvp in pendingPasswords)
                                        {
                                            command.Parameters["@Password"].Value = RSACryptoTextProvider.Encrypt(PublicKey, kvp.Value);
                                            command.Parameters["@DataSourceKey"].Value = DataSourceKey;
                                            command.Parameters["@DataSourceSubIndex"].Value = kvp.Key;
                                            command.ExecuteNonQuery();
                                        }
                                    }
                            }

                            _handles = new ReadOnlyDictionary<int, Database>(result);
                        }

                return _handles;
            }
        }

        private readonly SynchronizedQueue<ConnectionInfo> _connectionInfos = new SynchronizedQueue<ConnectionInfo>();

        private readonly SynchronizedDictionary<string, TimedTaskInfo> _timedTasks = new SynchronizedDictionary<string, TimedTaskInfo>(StringComparer.Ordinal);
        private Thread _timedTasksThread;

        #endregion

        #region 内嵌类

        private class ConnectionInfo
        {
            public ConnectionInfo(DbConnection connection, int lifetime)
            {
                _connection = connection;
                _lifetime = lifetime;
                _dateTime = DateTime.Now;
            }

            #region 属性

            private DbConnection _connection;
            private readonly int _lifetime;
            private readonly DateTime _dateTime;

            public DbConnection Connection
            {
                get
                {
                    if (_connection != null)
                    {
                        if ((_connection.State == ConnectionState.Open || _connection.State == ConnectionState.Closed) &&
                            (_lifetime == 0 || DateTime.Now.Subtract(_dateTime).TotalSeconds <= _lifetime))
                            return _connection;
                        Close();
                    }

                    return null;
                }
            }

            #endregion

            #region 方法

            public void Close()
            {
                if (_connection != null)
                {
                    DbConnection connection = _connection;
                    try
                    {
                        _connection = null;
                    }
                    finally
                    {
                        connection.Dispose();
                    }
                }
            }

            #endregion
        }

        private class TimedTaskInfo
        {
            public TimedTaskInfo(Action<DbConnection> cycleTimedTask, int cycleDayMultiple, int cycleHourMultiple)
            {
                _cycleTimedTask = cycleTimedTask;
                _cycleDayMultiple = cycleDayMultiple;
                _cycleHourMultiple = cycleHourMultiple;
            }

            #region 属性

            private readonly Action<DbConnection> _cycleTimedTask;
            private readonly int _cycleDayMultiple;
            private readonly int _cycleHourMultiple;
            private DateTime _executeTime;
            private bool _alreadyExecute;

            #endregion

            #region 方法

            public void Execute(DbConnection connection)
            {
                if (_alreadyExecute)
                {
                    if ((_cycleDayMultiple < 2 || _cycleDayMultiple > 28) && _executeTime.Day != DateTime.Now.Day ||
                        _cycleDayMultiple >= 2 && _cycleDayMultiple <= 28 && DateTime.Now.Day % _cycleDayMultiple != 0)
                        if ((_cycleHourMultiple < 2 || _cycleHourMultiple > 23) && _executeTime.Hour != DateTime.Now.Hour ||
                            _cycleHourMultiple >= 2 && _cycleHourMultiple <= 23 && DateTime.Now.Hour % _cycleHourMultiple != 0)
                            _alreadyExecute = false;
                }
                else
                {
                    if (_cycleDayMultiple < 2 || _cycleDayMultiple > 28 || DateTime.Now.Day % _cycleDayMultiple == 0)
                        if (_cycleHourMultiple < 2 || _cycleHourMultiple > 23 || DateTime.Now.Hour % _cycleHourMultiple == 0)
                        {
                            _cycleTimedTask(connection);
                            _executeTime = DateTime.Now;
                            _alreadyExecute = true;
                        }
                }
            }

            #endregion
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取实际操作的数据库入口
        /// </summary>
        /// <param name="routeKey">路由键</param>
        /// <returns>Handles[routeKey != null ? Math.Abs(routeKey.GetHashCode()) % (Handles.Count + 1) : 0]</returns>
        public Database GetHandle(object routeKey)
        {
            return Handles[routeKey != null ? Math.Abs(routeKey.GetHashCode()) % (Handles.Count + 1) : 0];
        }

        #region 数据库连接

        /// <summary>
        /// 清空连接池
        /// </summary>
        public void ClearAllPools()
        {
#if PgSQL
            NpgsqlConnection.ClearAllPools();
#endif
#if MsSQL
            SqlConnection.ClearAllPools();
#endif
#if MySQL
            MySqlConnection.ClearAllPools();
#endif
#if ORA
            OracleConnection.ClearAllPools();
#endif
            while (_connectionInfos.TryDequeue(out ConnectionInfo item))
            {
                if (item != null)
                    item.Close();
            }
        }

        /// <summary>
        /// 提取 DbConnection
        /// </summary>
        /// <param name="clearAllPools">清空连接池</param>
        public DbConnection GetConnection(bool clearAllPools = false)
        {
            if (clearAllPools)
                ClearAllPools();

            if (_connectionInfos.TryDequeue(out ConnectionInfo connectionInfo) && connectionInfo != null)
            {
                DbConnection result = connectionInfo.Connection;
                if (result != null && (result.State == ConnectionState.Open || result.State == ConnectionState.Closed))
                    return result;
            }

            return DbConnectionHelper.CreateConnection(ConnectionString);
        }

        /// <summary>
        /// 安置 DbConnection
        /// </summary>
        /// <param name="connection">DbConnection</param>
        public void PutConnection(DbConnection connection)
        {
            if (connection == null)
                return;
            if (Pooling)
                connection.Close();
            if (_connectionInfos.Count < MaxPoolSize)
                _connectionInfos.Enqueue(new ConnectionInfo(connection, ConnectionLifetime));
            else
                connection.Dispose();
        }

        /// <summary>
        /// 尝试连接
        /// </summary>
        public bool TryConnection()
        {
            return TryConnection(out _);
        }

        /// <summary>
        /// 尝试连接
        /// </summary>
        public bool TryConnection(out Exception error)
        {
            return DbConnectionHelper.TryConnection(ConnectionString, out error);
        }

        private void CheckThreshold(Exception exception)
        {
#if PgSQL
            if (!(exception is NpgsqlException))
                return;
#endif
#if MsSQL
            if (!(exception is SqlException))
                return;
#endif
#if MySQL
            if (!(exception is MySqlException))
                return;
#endif
#if ORA
            if (!(exception is OracleException))
                return;
#endif

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                bool differed = String.CompareOrdinal(_differedException, exception.Message) != 0;
                if (!differed && _exceptionRepetitions >= ClearingPoolsThresholdOfExceptionPerMinute &&
                    _exceptionRepetitions / DateTime.Now.Subtract(_differedDateTime).TotalMinutes >= ClearingPoolsThresholdOfExceptionPerMinute)
                {
                    ClearAllPools();
                    return;
                }

                if (!differed)
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        _exceptionRepetitions = _exceptionRepetitions + 1;
                    }
                    finally
                    {
                        _rwLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }
                else if (_exceptionRepetitions != 0)
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        _exceptionRepetitions = 0;
                        _differedDateTime = DateTime.Now;
                        _differedException = exception.Message;
                    }
                    finally
                    {
                        _rwLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region 执行数据库操作

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        public void Execute(Action<DbConnection> doExecute)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        public void Execute<TIn1>(Action<DbConnection, TIn1> doExecute,
            TIn1 in1)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        public void Execute<TIn1, TIn2>(Action<DbConnection, TIn1, TIn2> doExecute,
            TIn1 in1, TIn2 in2)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        public void Execute<TIn1, TIn2, TIn3>(Action<DbConnection, TIn1, TIn2, TIn3> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4, in5);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4, in5, in6);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4, in5, in6, in7);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4, in5, in6, in7, in8);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4, in5, in6, in7, in8, in9);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        /// <param name="in11">in参数11</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        /// <param name="in11">in参数11</param>
        /// <param name="in12">in参数12</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12>(Action<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11, TIn12 in12)
        {
            DbConnection connection = GetConnection();
            try
            {
                doExecute(connection, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11, in12);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        public TResult ExecuteGet<TResult>(Func<DbConnection, TResult> doExecuteGet)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        public TResult ExecuteGet<TIn1, TResult>(Func<DbConnection, TIn1, TResult> doExecuteGet,
            TIn1 in1)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        public TResult ExecuteGet<TIn1, TIn2, TResult>(Func<DbConnection, TIn1, TIn2, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4, in5);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4, in5, in6);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4, in5, in6, in7);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4, in5, in6, in7, in8);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4, in5, in6, in7, in8, in9);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        /// <param name="in11">in参数11</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        /// <param name="in11">in参数11</param>
        /// <param name="in12">in参数12</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12, TResult>(Func<DbConnection, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11, TIn12 in12)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = doExecuteGet(connection, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11, in12);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        public void Execute(Action<DbTransaction> doExecute)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        public void Execute<TIn1>(Action<DbTransaction, TIn1> doExecute,
            TIn1 in1)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        public void Execute<TIn1, TIn2>(Action<DbTransaction, TIn1, TIn2> doExecute,
            TIn1 in1, TIn2 in2)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        public void Execute<TIn1, TIn2, TIn3>(Action<DbTransaction, TIn1, TIn2, TIn3> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4, in5);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4, in5, in6);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4, in5, in6, in7);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4, in5, in6, in7, in8);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4, in5, in6, in7, in8, in9);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        /// <param name="in11">in参数11</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        /// <param name="in11">in参数11</param>
        /// <param name="in12">in参数12</param>
        public void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12>(Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11, TIn12 in12)
        {
            DbConnection connection = GetConnection();
            try
            {
                DbConnectionHelper.Execute(connection, doExecute, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11, in12);
                PutConnection(connection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        public TResult ExecuteGet<TResult>(Func<DbTransaction, TResult> doExecuteGet)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        public TResult ExecuteGet<TIn1, TResult>(Func<DbTransaction, TIn1, TResult> doExecuteGet,
            TIn1 in1)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        public TResult ExecuteGet<TIn1, TIn2, TResult>(Func<DbTransaction, TIn1, TIn2, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4, in5);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4, in5, in6);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4, in5, in6, in7);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4, in5, in6, in7, in8);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4, in5, in6, in7, in8, in9);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        /// <param name="in11">in参数11</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        /// <param name="in9">in参数9</param>
        /// <param name="in10">in参数10</param>
        /// <param name="in11">in参数11</param>
        /// <param name="in12">in参数12</param>
        public TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12, TResult>(Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11, TIn12 in12)
        {
            DbConnection connection = GetConnection();
            try
            {
                TResult result = DbConnectionHelper.ExecuteGet(connection, doExecuteGet, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11, in12);
                PutConnection(connection);
                return result;
            }
            catch (Exception ex)
            {
                connection.Dispose();
                CheckThreshold(ex);
                throw;
            }
        }

        /// <summary>
        /// 执行DbCommand
        /// </summary>
        /// <returns>执行记录数</returns>
        public int ExecuteNonQuery(string sql, params ParamValue[] paramValues)
        {
            return ExecuteGet((Func<DbConnection, string, ParamValue[], int>) DbCommandHelper.ExecuteNonQuery, sql, paramValues);
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <returns>结果集(参数名-参数值)</returns>
        public IDictionary<string, object> ExecuteStoredProc(string storedProcedure, params ParamValue[] paramValues)
        {
            return ExecuteGet((Func<DbConnection, string, ParamValue[], IDictionary<string, object>>) DbCommandHelper.ExecuteStoredProc, storedProcedure, paramValues);
        }
        
        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <returns>返回值</returns>
        public object ExecuteScalar(string sql, params ParamValue[] paramValues)
        {
            return ExecuteGet((Func<DbConnection, string, ParamValue[], object>) DbCommandHelper.ExecuteScalar, sql, paramValues);
        }
        
        /// <summary>
        /// 构建 DataReader
        /// </summary>
        public DataReader CreateDataReader(string sql, params ParamValue[] paramValues)
        {
            return CreateDataReader(sql, CommandBehavior.Default, paramValues);
        }
        
        /// <summary>
        /// 构建 DataReader
        /// </summary>
        public DataReader CreateDataReader(string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            return new DataReader(this, sql, behavior, paramValues);
        }

        /// <summary>
        /// 读取JSON格式数据(属性名为表/视图的字段名/别名)
        /// </summary>
        public string ReadJsonData(string sql, params ParamValue[] paramValues)
        {
            return ReadJsonData(sql, CommandBehavior.Default, paramValues);
        }
        
        /// <summary>
        /// 读取JSON格式数据(属性名为表/视图的字段名/别名)
        /// </summary>
        public string ReadJsonData(string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            using (DataReader dataReader = CreateDataReader(sql, behavior, paramValues))
            {
                return dataReader.ReadJson(behavior == CommandBehavior.SingleRow);
            }
        }

        /// <summary>
        /// 读取Dictionary格式数据(属性名为表/视图的字段名/别名)
        /// </summary>
        public IList<IDictionary<string, object>> ReadDictionaryData(string sql, params ParamValue[] paramValues)
        {
            return ReadDictionaryData(sql, CommandBehavior.Default, paramValues);
        }

        /// <summary>
        /// 读取Dictionary格式数据(属性名为表/视图的字段名/别名)
        /// </summary>
        public IList<IDictionary<string, object>> ReadDictionaryData(string sql, CommandBehavior behavior, params ParamValue[] paramValues)
        {
            using (DataReader dataReader = CreateDataReader(sql, behavior, paramValues))
            {
                return dataReader.ReadDictionary(behavior == CommandBehavior.SingleRow);
            }
        }

        /// <summary>
        /// 填充 DataSet
        /// </summary>
        public DataSet FillDataSet(string sql, params ParamValue[] paramValues)
        {
            return ExecuteGet((Func<DbConnection, string, ParamValue[], DataSet>) DbCommandHelper.FillDataSet, sql, paramValues);
        }

        #endregion

        #region TimedTask

        /// <summary>
        /// 添加定时执行的任务
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cycleTimedTask">定时任务</param>
        /// <param name="cycleDayMultiple">执行于每月里可被参数(2~28)除整的日期(范围以外为每天)</param>
        /// <param name="cycleHourMultiple">执行于被筛选日期里可被参数(2~23)除整的小时(范围以外为每小时)</param>
        public void AddTimedTask(string key, Action<DbConnection> cycleTimedTask, int cycleDayMultiple, int cycleHourMultiple = 23)
        {
            if (cycleTimedTask == null)
                throw new ArgumentNullException(nameof(cycleTimedTask));

            InitializeTimedTasks();
            Execute(cycleTimedTask);

            _timedTasks[key] = new TimedTaskInfo(cycleTimedTask, cycleDayMultiple, cycleHourMultiple);
        }

        /// <summary>
        /// 移除定时执行的任务
        /// </summary>
        /// <param name="key">key</param>
        public void RemoveTimedTask(string key)
        {
            _timedTasks.Remove(key);
        }

        private void InitializeTimedTasks()
        {
            if (_timedTasksThread == null)
                lock (_timedTasks)
                    if (_timedTasksThread == null)
                    {
                        _timedTasksThread = new Thread(ExecuteTimedTasks);
                        _timedTasksThread.IsBackground = true;
                        _timedTasksThread.Start();
                    }

            Thread.MemoryBarrier();
        }

        private void ExecuteTimedTasks()
        {
            DbConnection connection = null;
            try
            {
                while (true)
                    try
                    {
                        if (connection == null)
                            connection = GetConnection();

                        int exceptionCount = 0;
                        foreach (KeyValuePair<string, TimedTaskInfo> kvp in _timedTasks)
                            try
                            {
                                kvp.Value.Execute(connection);
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex)
                            {
                                exceptionCount = exceptionCount + 1;
                                if (exceptionCount == _timedTasks.Count)
                                    throw;
                                LogHelper.Error(ex, "{@TimedTaskKey}", kvp.Key);
                            }

                        Thread.Sleep(1000 * 60);
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (connection != null)
                        {
                            connection.Dispose();
                            connection = null;
                        }

                        LogHelper.Error(ex, "{@TimedTasksCount}", _timedTasks.Count);
                        Thread.Sleep(1000 * 60);
                    }
            }
            finally
            {
                if (connection != null)
                    connection.Dispose();

                _timedTasksThread = null;
            }
        }

        #endregion

        #endregion
    }
}