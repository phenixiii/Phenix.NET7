using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Security;

namespace Phenix.Core.Log
{
    /// <summary>
    /// 事件日志
    /// 由Database.Default的PH7_EventLog_X表(按季度X为1~4)存储日志
    /// </summary>
    public static class EventLog
    {
        #region 属性

        #region 配置项

        private const string DefExtension = "log";

        private static int CurrentQuarter
        {
            get
            {
                int month = DateTime.Today.Month;
                return month % 3 == 0 ? month / 3 : month / 3 + 1;
            }
        }

        /// <summary>
        /// 当前操作的表的表名
        /// </summary>
        public static string CurrentTableName
        {
            get { return String.Format("PH7_EventLog_{0}", CurrentQuarter); }
        }

        /// <summary>
        /// 可被清理的表的表名
        /// </summary>
        public static string CleanTableName
        {
            get
            {
                int quarter = CurrentQuarter + 1;
                return String.Format("PH7_EventLog_{0}", quarter > 4 ? 1 : quarter);
            }
        }

        /// <summary>
        /// 存储目录
        /// 默认：DateTime.Today.ToString("yyyyMMdd")
        /// </summary>
        public static string LocalDirectory
        {
            get { return Path.Combine(AppRun.TempDirectory, DateTime.Today.ToString("yyyyMMdd")); }
        }

        private static int? _breakThresholdOfRepeatPerMinute;

        /// <summary>
        /// 中断重复日志阈值(每分钟次数)
        /// 默认：6000(>=6000)
        /// </summary>
        public static int BreakThresholdOfRepeatPerMinute
        {
            get { return new[] {AppSettings.GetProperty(ref _breakThresholdOfRepeatPerMinute, 6000), 6000}.Max(); }
            set { AppSettings.SetProperty(ref _breakThresholdOfRepeatPerMinute, new[] {value, 6000}.Max()); }
        }

        #endregion

        #region 数据源

        private static Database _database;

        /// <summary>
        /// 数据库入口
        /// </summary>
        public static Database Database
        {
            get
            {
                if (_database == null)
                {
                    Database database = Database.Default;
                    lock (database)
                        if (_database == null)
                        {
                            _database = database;
                            InitializeTable(database);
                        }

                    Thread.MemoryBarrier();
                }

                return _database;
            }
        }

        #endregion

        private static readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
        private static long _messageRepetitions;
        private static DateTime _differedDateTime = DateTime.Now;
        private static string _differedMessage;

        #endregion

        #region 方法

        /// <summary>
        /// 保存错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="error">错误</param>
        public static void Save(string message, Exception error)
        {
            Save(new StackTrace().GetFrame(1).GetMethod(), message, null, error);
        }

        /// <summary>
        /// 保存错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="address">地址</param>
        /// <param name="error">错误</param>
        public static void Save(string message, string address = null, Exception error = null)
        {
            Save(new StackTrace().GetFrame(1).GetMethod(), message, address, error);
        }

        /// <summary>
        /// 保存错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="traceKey">调用链键值</param>
        /// <param name="traceOrder">调用链顺序</param>
        /// <param name="error">错误</param>
        public static void Save(string message, long traceKey, int traceOrder, Exception error = null)
        {
            Save(new StackTrace().GetFrame(1).GetMethod(), message, traceKey, traceOrder, error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="error">错误</param>
        public static void Save(MethodBase method, string message, Exception error)
        {
            Save(method, message, null, error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="address">地址</param>
        /// <param name="error">错误</param>
        public static void Save(MethodBase method, string message, string address = null, Exception error = null)
        {
            Save(Database.Sequence.Value, method, message, address, error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="traceKey">调用链键值</param>
        /// <param name="traceOrder">调用链顺序</param>
        /// <param name="error">错误</param>
        public static void Save(MethodBase method, string message, long traceKey, int traceOrder, Exception error = null)
        {
            Save(Database.Sequence.Value, method, message, traceKey, traceOrder, error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="error">错误</param>
        public static void Save(long id, MethodBase method, string message, Exception error)
        {
            Save(id, method, message, null, error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="address">地址</param>
        /// <param name="error">错误</param>
        public static void Save(long id, MethodBase method, string message, string address = null, Exception error = null)
        {
            if (!Save(new EventInfo(id, method, message, address, error)))
                SaveLocal(method, String.Format("{0}: {1}", address, message), error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="traceKey">调用链键值</param>
        /// <param name="traceOrder">调用链顺序</param>
        /// <param name="error">错误</param>
        public static void Save(long id, MethodBase method, string message, long traceKey, int traceOrder, Exception error = null)
        {
            if (!Save(new EventInfo(id, method, message, traceKey, traceOrder, error)))
                SaveLocal(method, String.Format("{0}-{1}: {2}", traceKey, traceOrder, message), error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="info">事件资料</param>
        /// <returns>返回对象</returns>
        public static bool Save(EventInfo info)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                bool differed = String.CompareOrdinal(_differedMessage, info.Message) != 0;
                if (!differed && _messageRepetitions >= BreakThresholdOfRepeatPerMinute &&
                    _messageRepetitions / DateTime.Now.Subtract(_differedDateTime).TotalMinutes >= BreakThresholdOfRepeatPerMinute)
                    return true;

                bool result = Database.ExecuteGet(Save, info);

                if (!differed)
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        _messageRepetitions = _messageRepetitions + 1;
                    }
                    finally
                    {
                        _rwLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }
                else if (_messageRepetitions != 0)
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        _messageRepetitions = 0;
                        _differedDateTime = DateTime.Now;
                        _differedMessage = info.Message;
                    }
                    finally
                    {
                        _rwLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }

                return result;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        private static bool Save(DbConnection connection, EventInfo info)
        {
#if PgSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, String.Format(@"
insert into {0}
  (EL_ID, EL_Time, EL_ClassName, EL_MethodName, EL_ExceptionName, EL_ExceptionMessage, EL_User, EL_Address, EL_Trace_Key, EL_Trace_Order)
values
  (@EL_ID, @EL_Time, @EL_ClassName, @EL_MethodName, @EL_ExceptionName, @EL_ExceptionMessage, @EL_User, @EL_Address, @EL_Trace_Key, @EL_Trace_Order)",
                CurrentTableName)))
#endif
#if MsSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, String.Format(@"
insert into {0}
  (EL_ID, EL_Time, EL_ClassName, EL_MethodName, EL_ExceptionName, EL_ExceptionMessage, EL_User, EL_Address, EL_Trace_Key, EL_Trace_Order)
values
  (@EL_ID, @EL_Time, @EL_ClassName, @EL_MethodName, @EL_ExceptionName, @EL_ExceptionMessage, @EL_User, @EL_Address, @EL_Trace_Key, @EL_Trace_Order)",
                CurrentTableName)))
#endif
#if MySQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, String.Format(@"
insert into {0}
  (EL_ID, EL_Time, EL_ClassName, EL_MethodName, EL_ExceptionName, EL_ExceptionMessage, EL_User, EL_Address, EL_Trace_Key, EL_Trace_Order)
values
  (?EL_ID, ?EL_Time, ?EL_ClassName, ?EL_MethodName, ?EL_ExceptionName, ?EL_ExceptionMessage, ?EL_User, ?EL_Address, ?EL_Trace_Key, ?EL_Trace_Order)",
                CurrentTableName)))
#endif
#if ORA
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, String.Format(@"
insert into {0}
  (EL_ID, EL_Time, EL_ClassName, EL_MethodName, EL_ExceptionName, EL_ExceptionMessage, EL_User, EL_Address, EL_Trace_Key, EL_Trace_Order)
values
  (:EL_ID, :EL_Time, :EL_ClassName, :EL_MethodName, :EL_ExceptionName, :EL_ExceptionMessage, :EL_User, :EL_Address, :EL_Trace_Key, :EL_Trace_Order)",
                CurrentTableName)))
#endif
            {
                DbCommandHelper.CreateParameter(command, "EL_ID", info.Id);
                DbCommandHelper.CreateParameter(command, "EL_Time", info.Time);
                DbCommandHelper.CreateParameter(command, "EL_ClassName", info.ClassName);
                DbCommandHelper.CreateParameter(command, "EL_MethodName", info.MethodName);
                DbCommandHelper.CreateParameter(command, "EL_ExceptionName", info.ExceptionName);
                DbCommandHelper.CreateParameter(command, "EL_ExceptionMessage", Phenix.Core.Reflection.Utilities.SubString(info.ExceptionMessage, 4000, false));
                DbCommandHelper.CreateParameter(command, "EL_User", info.User);
                DbCommandHelper.CreateParameter(command, "EL_Address", info.Address);
                DbCommandHelper.CreateParameter(command, "EL_Trace_Key", info.traceKey);
                DbCommandHelper.CreateParameter(command, "EL_Trace_Order", info.traceOrder);
                try
                {
                    DbCommandHelper.ExecuteNonQuery(command, false);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
#if PgSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, String.Format(@"
update {0} set
  EL_Message = @EL_Message
where EL_ID = @EL_ID",
                CurrentTableName)))
#endif
#if MsSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, String.Format(@"
update {0} set
  EL_Message = @EL_Message
where EL_ID = @EL_ID",
                CurrentTableName)))
#endif
#if MySQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, String.Format(@"
update {0} set
  EL_Message = ?EL_Message
where EL_ID = ?EL_ID",
                CurrentTableName)))
#endif
#if ORA
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, String.Format(@"
update {0} set
  EL_Message = :EL_Message
where EL_ID = :EL_ID",
                CurrentTableName)))
#endif
            {
                DbCommandHelper.CreateParameter(command, "EL_Message", info.Message);
                DbCommandHelper.CreateParameter(command, "EL_ID", info.Id);
                try
                {
                    return DbCommandHelper.ExecuteNonQuery(command, false) == 1;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        #region Initialize

        private static void InitializeTable(Database database)
        {
            database.AddTimedTask("Clear PH7_EventLog", Clear, 20);
        }

        private static void Clear(DbConnection connection)
        {
            DbCommandHelper.ExecuteNonQuery(connection, String.Format("truncate table {0}", CleanTableName), false);
        }

        #endregion

        #region Local

        /// <summary>
        /// 保存错误日志到本地LocalDirectory目录
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="extension">后缀</param>
        public static void SaveLocal(string message, string extension)
        {
            try
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    bool differed = String.CompareOrdinal(_differedMessage, message) != 0;
                    if (!differed && _messageRepetitions >= BreakThresholdOfRepeatPerMinute &&
                        _messageRepetitions / DateTime.Now.Subtract(_differedDateTime).TotalMinutes >= BreakThresholdOfRepeatPerMinute)
                        return;

                    string directory = LocalDirectory;
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    using (StreamWriter logFile = File.AppendText(Path.Combine(directory, String.Format("{0}Log.{1}", Principal.CurrentIdentity != null ? Principal.CurrentIdentity.UserName : null, extension))))
                    {
                        logFile.WriteLine("{0} {1}", DateTime.Now, message);
                        logFile.WriteLine();
                    }

                    if (!differed)
                    {
                        LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                        try
                        {
                            _messageRepetitions = _messageRepetitions + 1;
                        }
                        finally
                        {
                            _rwLock.DowngradeFromWriterLock(ref lockCookie);
                        }
                    }
                    else if (_messageRepetitions != 0)
                    {
                        LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                        try
                        {
                            _messageRepetitions = 0;
                            _differedDateTime = DateTime.Now;
                            _differedMessage = message;
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
            catch (IOException)
            {
                SaveLocal(message, String.Format("{0}.{1}", extension, Guid.NewGuid().ToString("N").Substring(20)));
            }
        }

        /// <summary>
        /// 保存错误日志到本地LocalDirectory目录
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="error">错误</param>
        /// <param name="extension">后缀</param>
        public static void SaveLocal(string message, Exception error = null, string extension = DefExtension)
        {
            SaveLocal(error != null ? String.Format("{0} : {1}[{2}]", message, AppRun.GetErrorMessage(error), error.StackTrace) : message, extension);
        }

        /// <summary>
        /// 保存对象日志到本地LocalDirectory目录
        /// </summary>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="extension">后缀</param>
        public static void SaveLocal(MethodBase method, string message, string extension)
        {
            SaveLocal(method, message, null, extension);
        }

        /// <summary>
        /// 保存对象日志到本地LocalDirectory目录
        /// </summary>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="error">错误</param>
        /// <param name="extension">后缀</param>
        public static void SaveLocal(MethodBase method, string message, Exception error = null, string extension = DefExtension)
        {
            SaveLocal(String.Format("{0}.{1}: {2}", method != null ? (method.ReflectedType ?? method.DeclaringType).FullName : null, method != null ? method.Name : null, message), error, extension);
        }

        #endregion

        #endregion
    }
}