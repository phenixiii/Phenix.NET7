using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
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
using Phenix.Core.Log;

namespace Phenix.Core.Data.Common
{
    /// <summary>
    /// DbConnection 助手
    /// </summary>
    public static class DbConnectionHelper
    {
        #region 方法

        /// <summary>
        /// 构建数据库连接串
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
        public static string BuildConnectionString(string dataSource, int? port, string databaseName, string userId, string password,
            bool pooling = true, int minPoolSize = 0, int maxPoolSize = 100, int connectionLifetime = 0)
        {
#if PgSQL
            string result = port.HasValue
                ? String.Format("Server={0};Port={1};Database={2};User ID={3};Password={4}", dataSource, port, databaseName, userId, password)
                : String.Format("Server={0};Database={1};User ID={2};Password={3}", dataSource, databaseName, userId, password);
            return pooling
                ? String.Format("Pooling=true;MinPoolSize={0};MaxPoolSize={1};ConnectionIdleLifetime={2};{3}", minPoolSize, maxPoolSize, connectionLifetime > 0 ? connectionLifetime : 300, result)
                : String.Format("Pooling=false;{0}", result);
#endif
#if MsSQL
            string result = port.HasValue
                ? String.Format("Data Source={0},{1};Initial Catalog={2};User ID={3};Password={4}", dataSource, port, databaseName, userId, password)
                : String.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", dataSource, databaseName, userId, password);
            return pooling
                ? String.Format("Pooling=true;Min Pool Size={0};Max Pool Size={1};Connection Lifetime={2};{3}", minPoolSize, maxPoolSize, connectionLifetime > 0 ? connectionLifetime : 0, result)
                : String.Format("Pooling=false;{0}", result);
#endif
#if MySQL
            string result = port.HasValue
                ? String.Format("Server={0};Port={1};Database={2};User ID={3};Password={4};Charset=utf8;TreatTinyAsBoolean=false;AllowUserVariables=true", dataSource, port, databaseName, userId, password)
                : String.Format("Server={0};Database={1};User ID={2};Password={3};Charset=utf8;TreatTinyAsBoolean=false;AllowUserVariables=true", dataSource, databaseName, userId, password);
            return pooling
                ? String.Format("Pooling=true;Min Pool Size={0};Max Pool Size={1};Connection Lifetime={2};{3}", minPoolSize, maxPoolSize, connectionLifetime > 0 ? connectionLifetime : 0, result)
                : String.Format("Pooling=false;{0}", result);
#endif
#if ORA
            string result = port.HasValue
                ? String.Format("Data Source={0}:{1}/{2};User ID={3};Password={4}", dataSource, port, databaseName, userId, password)
                : String.Format("Data Source={0}/{1};User ID={2};Password={3}", dataSource, databaseName, userId, password);
            return pooling
                ? String.Format("Pooling=true;Min Pool Size={0};Max Pool Size={1};Connection Lifetime={2};{3}", minPoolSize, maxPoolSize, connectionLifetime > 0 ? connectionLifetime : 0, result)
                : String.Format("Pooling=false;{0}", result);
#endif
        }

        #region Connection


        /// <summary>
        /// 构建 DbConnection
        /// </summary>
        public static DbConnection CreateConnection()
        {
#if PgSQL
            return new NpgsqlConnection();
#endif
#if MsSQL
            return new SqlConnection();
#endif
#if MySQL
            return new MySqlConnection();
#endif
#if ORA
            return new OracleConnection();
#endif
        }

        /// <summary>
        /// 构建 DbConnection
        /// </summary>
        public static DbConnection CreateConnection(string connectionString)
        {
#if PgSQL
            return new NpgsqlConnection(connectionString);
#endif
#if MsSQL
            return new SqlConnection(connectionString);
#endif
#if MySQL
            return new MySqlConnection(connectionString);
#endif
#if ORA
            return new OracleConnection(connectionString);
#endif
        }

        /// <summary>
        /// 尝试连接
        /// </summary>
        public static bool TryConnection(string connectionString, out Exception error)
        {
            using (DbConnection connection = CreateConnection(connectionString))
            {
                return TryConnection(connection, out error);
            }
        }

        /// <summary>
        /// 尝试连接
        /// </summary>
        public static bool TryConnection(DbConnection connection, out Exception error)
        {
            try
            {
                connection.Close();
                connection.Open();
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        public static void OpenConnection(DbConnection connection)
        {
            try
            {
                switch (connection.State)
                {
                    case ConnectionState.Broken:
                        connection.Close();
                        connection.Open();
                        break;
                    case ConnectionState.Closed:
                        connection.Open();
                        break;
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => EventLog.SaveLocal(MethodBase.GetCurrentMethod(), connection.DataSource, ex));
                throw;
            }
        }

        #endregion

        #region Transaction

        /// <summary>
        /// 开始数据库事务
        /// </summary>
        /// <param name="connection">DbConnection</param>
        public static DbTransaction BeginTransaction(DbConnection connection)
        {
            OpenConnection(connection);
            return connection.BeginTransaction();
        }

        #endregion

        #region 执行数据库操作

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        public static void Execute(DbConnection connection, Action<DbTransaction> doExecute)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        public static void Execute<TIn1>(DbConnection connection, Action<DbTransaction, TIn1> doExecute,
            TIn1 in1)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        public static void Execute<TIn1, TIn2>(DbConnection connection, Action<DbTransaction, TIn1, TIn2> doExecute,
            TIn1 in1, TIn2 in2)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        public static void Execute<TIn1, TIn2, TIn3>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        public static void Execute<TIn1, TIn2, TIn3, TIn4>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        public static void Execute<TIn1, TIn2, TIn3, TIn4, TIn5>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4, in5);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        public static void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4, in5, in6);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        public static void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4, in5, in6, in7);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecute">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        public static void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4, in5, in6, in7, in8);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
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
        public static void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4, in5, in6, in7, in8, in9);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
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
        public static void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
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
        public static void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
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
        public static void Execute<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12>(DbConnection connection, Action<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12> doExecute,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11, TIn12 in12)
        {
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    doExecute(transaction, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11, in12);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        public static TResult ExecuteGet<TResult>(DbConnection connection, Func<DbTransaction, TResult> doExecuteGet)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        public static TResult ExecuteGet<TIn1, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TResult> doExecuteGet,
            TIn1 in1)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        public static TResult ExecuteGet<TIn1, TIn2, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4, in5);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4, in5, in6);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4, in5, in6, in7);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="doExecuteGet">执行数据库操作处理函数</param>
        /// <param name="in1">in参数1</param>
        /// <param name="in2">in参数2</param>
        /// <param name="in3">in参数3</param>
        /// <param name="in4">in参数4</param>
        /// <param name="in5">in参数5</param>
        /// <param name="in6">in参数6</param>
        /// <param name="in7">in参数7</param>
        /// <param name="in8">in参数8</param>
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4, in5, in6, in7, in8);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
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
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4, in5, in6, in7, in8, in9);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
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
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
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
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="connection">DbConnection</param>
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
        public static TResult ExecuteGet<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12, TResult>(DbConnection connection, Func<DbTransaction, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TIn10, TIn11, TIn12, TResult> doExecuteGet,
            TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9, TIn10 in10, TIn11 in11, TIn12 in12)
        {
            TResult result;
            using (DbTransaction transaction = BeginTransaction(connection))
                try
                {
                    result = doExecuteGet(transaction, in1, in2, in3, in4, in5, in6, in7, in8, in9, in10, in11, in12);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            return result;
        }

        #endregion

        #endregion
    }
}