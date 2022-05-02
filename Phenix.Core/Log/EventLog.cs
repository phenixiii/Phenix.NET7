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
    /// �¼���־
    /// ��Database.Default��PH7_EventLog_X��(������XΪ1~4)�洢��־
    /// </summary>
    public static class EventLog
    {
        #region ����

        #region ������

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
        /// ��ǰ�����ı�ı���
        /// </summary>
        public static string CurrentTableName
        {
            get { return String.Format("PH7_EventLog_{0}", CurrentQuarter); }
        }

        /// <summary>
        /// �ɱ�����ı�ı���
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
        /// �洢Ŀ¼
        /// Ĭ�ϣ�DateTime.Today.ToString("yyyyMMdd")
        /// </summary>
        public static string LocalDirectory
        {
            get { return Path.Combine(AppRun.TempDirectory, DateTime.Today.ToString("yyyyMMdd")); }
        }

        private static int? _breakThresholdOfRepeatPerMinute;

        /// <summary>
        /// �ж��ظ���־��ֵ(ÿ���Ӵ���)
        /// Ĭ�ϣ�6000(>=6000)
        /// </summary>
        public static int BreakThresholdOfRepeatPerMinute
        {
            get { return new[] {AppSettings.GetProperty(ref _breakThresholdOfRepeatPerMinute, 6000), 6000}.Max(); }
            set { AppSettings.SetProperty(ref _breakThresholdOfRepeatPerMinute, new[] {value, 6000}.Max()); }
        }

        #endregion

        #region ����Դ

        private static Database _database;

        /// <summary>
        /// ���ݿ����
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

        #region ����

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="message">��Ϣ</param>
        /// <param name="error">����</param>
        public static void Save(string message, Exception error)
        {
            Save(new StackTrace().GetFrame(1).GetMethod(), message, null, error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="message">��Ϣ</param>
        /// <param name="address">��ַ</param>
        /// <param name="error">����</param>
        public static void Save(string message, string address = null, Exception error = null)
        {
            Save(new StackTrace().GetFrame(1).GetMethod(), message, address, error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="message">��Ϣ</param>
        /// <param name="traceKey">��������ֵ</param>
        /// <param name="traceOrder">������˳��</param>
        /// <param name="error">����</param>
        public static void Save(string message, long traceKey, int traceOrder, Exception error = null)
        {
            Save(new StackTrace().GetFrame(1).GetMethod(), message, traceKey, traceOrder, error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="method">��������Ϣ</param>
        /// <param name="message">��Ϣ</param>
        /// <param name="error">����</param>
        public static void Save(MethodBase method, string message, Exception error)
        {
            Save(method, message, null, error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="method">��������Ϣ</param>
        /// <param name="message">��Ϣ</param>
        /// <param name="address">��ַ</param>
        /// <param name="error">����</param>
        public static void Save(MethodBase method, string message, string address = null, Exception error = null)
        {
            Save(Database.Sequence.Value, method, message, address, error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="method">��������Ϣ</param>
        /// <param name="message">��Ϣ</param>
        /// <param name="traceKey">��������ֵ</param>
        /// <param name="traceOrder">������˳��</param>
        /// <param name="error">����</param>
        public static void Save(MethodBase method, string message, long traceKey, int traceOrder, Exception error = null)
        {
            Save(Database.Sequence.Value, method, message, traceKey, traceOrder, error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="method">��������Ϣ</param>
        /// <param name="message">��Ϣ</param>
        /// <param name="error">����</param>
        public static void Save(long id, MethodBase method, string message, Exception error)
        {
            Save(id, method, message, null, error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="method">��������Ϣ</param>
        /// <param name="message">��Ϣ</param>
        /// <param name="address">��ַ</param>
        /// <param name="error">����</param>
        public static void Save(long id, MethodBase method, string message, string address = null, Exception error = null)
        {
            if (!Save(new EventInfo(id, method, message, address, error)))
                SaveLocal(method, String.Format("{0}: {1}", address, message), error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="method">��������Ϣ</param>
        /// <param name="message">��Ϣ</param>
        /// <param name="traceKey">��������ֵ</param>
        /// <param name="traceOrder">������˳��</param>
        /// <param name="error">����</param>
        public static void Save(long id, MethodBase method, string message, long traceKey, int traceOrder, Exception error = null)
        {
            if (!Save(new EventInfo(id, method, message, traceKey, traceOrder, error)))
                SaveLocal(method, String.Format("{0}-{1}: {2}", traceKey, traceOrder, message), error);
        }

        /// <summary>
        /// ���������־
        /// </summary>
        /// <param name="info">�¼�����</param>
        /// <returns>���ض���</returns>
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
        /// ���������־������LocalDirectoryĿ¼
        /// </summary>
        /// <param name="message">��Ϣ</param>
        /// <param name="extension">��׺</param>
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
        /// ���������־������LocalDirectoryĿ¼
        /// </summary>
        /// <param name="message">��Ϣ</param>
        /// <param name="error">����</param>
        /// <param name="extension">��׺</param>
        public static void SaveLocal(string message, Exception error = null, string extension = DefExtension)
        {
            SaveLocal(error != null ? String.Format("{0} : {1}[{2}]", message, AppRun.GetErrorMessage(error), error.StackTrace) : message, extension);
        }

        /// <summary>
        /// ���������־������LocalDirectoryĿ¼
        /// </summary>
        /// <param name="method">��������Ϣ</param>
        /// <param name="message">��Ϣ</param>
        /// <param name="extension">��׺</param>
        public static void SaveLocal(MethodBase method, string message, string extension)
        {
            SaveLocal(method, message, null, extension);
        }

        /// <summary>
        /// ���������־������LocalDirectoryĿ¼
        /// </summary>
        /// <param name="method">��������Ϣ</param>
        /// <param name="message">��Ϣ</param>
        /// <param name="error">����</param>
        /// <param name="extension">��׺</param>
        public static void SaveLocal(MethodBase method, string message, Exception error = null, string extension = DefExtension)
        {
            SaveLocal(String.Format("{0}.{1}: {2}", method != null ? (method.ReflectedType ?? method.DeclaringType).FullName : null, method != null ? method.Name : null, message), error, extension);
        }

        #endregion

        #endregion
    }
}