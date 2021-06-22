using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;

namespace Phenix.Services.Business.Message
{
    /// <summary>
    /// 用户消息
    /// 由Database.Default的PH7_UserMessage表提供消息缓存
    /// </summary>
    public static class UserMessage
    {
        #region 属性

        #region 配置项

        private static int? _clearMessageDeferMonths;

        /// <summary>
        /// 清理几个月前的消息
        /// 默认：12(>=3)
        /// </summary>
        public static int ClearMessageDeferMonths
        {
            get { return AppSettings.GetProperty(ref _clearMessageDeferMonths, 12); }
            set { AppSettings.SetProperty(ref _clearMessageDeferMonths, value >= 3 ? value : 3); }
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

        #endregion

        #region 方法
        
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="sender">发送用户</param>
        /// <param name="receiver">接收用户</param>
        /// <param name="content">消息内容</param>
        public static UserMessageInfo Send(string sender, string receiver, string content)
        {
            UserMessageInfo result = new UserMessageInfo(Database.Sequence.Value, sender, receiver, content);
            Send(result);
            return result;
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="info">事件资料</param>
        public static void Send(UserMessageInfo info)
        {
            Database.Execute(Send, info);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="transaction">DbTransaction</param>
        /// <param name="info">事件资料</param>
        public static void Send(DbTransaction transaction, UserMessageInfo info)
        {
            if (String.IsNullOrEmpty(info.Sender))
                throw new InvalidOperationException("必须指定发送方!");
            if (String.IsNullOrEmpty(info.Receiver))
                throw new InvalidOperationException("必须指定接收方!");

            bool existed = false;
#if PgSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
insert into PH7_UserMessage
  (UM_ID, UM_Sender, UM_Receiver, UM_CreateTime)
values
  (@UM_ID, @UM_Sender, @UM_Receiver, @UM_CreateTime)"))
#endif
#if MsSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
insert into PH7_UserMessage
  (UM_ID, UM_Sender, UM_Receiver, UM_CreateTime)
values
  (@UM_ID, @UM_Sender, @UM_Receiver, @UM_CreateTime)"))
#endif
#if MySQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
insert into PH7_UserMessage
  (UM_ID, UM_Sender, UM_Receiver, UM_CreateTime)
values
  (?UM_ID, ?UM_Sender, ?UM_Receiver, ?UM_CreateTime)"))
#endif
#if ORA
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
insert into PH7_UserMessage
  (UM_ID, UM_Sender, UM_Receiver, UM_CreateTime)
values
  (:UM_ID, :UM_Sender, :UM_Receiver, :UM_CreateTime)"))
#endif
            {
                DbCommandHelper.CreateParameter(command, "UM_ID", info.Id);
                DbCommandHelper.CreateParameter(command, "UM_Sender", info.Sender);
                DbCommandHelper.CreateParameter(command, "UM_Receiver", info.Receiver);
                DbCommandHelper.CreateParameter(command, "UM_CreateTime", info.CreateTime);
                try
                {
                    DbCommandHelper.ExecuteNonQuery(command, false);
                }
                catch (Exception)
                {
                    existed = true;
                }
            }

            if (existed)
            {
#if PgSQL
                using (DataReader reader = new DataReader(transaction, @"
select UM_Content
from PH7_UserMessage
where UM_ID = @UM_ID", CommandBehavior.SingleRow, false))
#endif
#if MsSQL
                using (DataReader reader = new DataReader(transaction, @"
select UM_Content
from PH7_UserMessage
where UM_ID = @UM_ID", CommandBehavior.SingleRow, false))
#endif
#if MySQL
                using (DataReader reader = new DataReader(transaction, @"
select UM_Content
from PH7_UserMessage
where UM_ID = ?UM_ID", CommandBehavior.SingleRow, false))
#endif
#if ORA
                using (DataReader reader = new DataReader(transaction, @"
select UM_Content
from PH7_UserMessage
where UM_ID = :UM_ID", CommandBehavior.SingleRow, false))
#endif
                {
                    reader.CreateParameter("UM_ID", info.Id);
                    if (reader.Read())
                        if (String.CompareOrdinal(reader.GetNullableString(0), info.Content) == 0)
                            return;
                }
#if PgSQL
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_Sender = @UM_Sender,
  UM_Receiver = @UM_Receiver,
  UM_CreateTime = now(),
  UM_SendTime = now(),
  UM_ReceivedTime = null
where UM_ID = @UM_ID"))
#endif
#if MsSQL
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_Sender = @UM_Sender,
  UM_Receiver = @UM_Receiver,
  UM_CreateTime = getdate(),
  UM_SendTime = getdate(),
  UM_ReceivedTime = null
where UM_ID = @UM_ID"))
#endif
#if MySQL
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_Sender = ?UM_Sender,
  UM_Receiver = ?UM_Receiver,
  UM_CreateTime = now(),
  UM_SendTime = now(),
  UM_ReceivedTime = null
where UM_ID = ?UM_ID"))
#endif
#if ORA
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_Sender = :UM_Sender,
  UM_Receiver = :UM_Receiver,
  UM_CreateTime = sysdate,
  UM_SendTime = sysdate,
  UM_ReceivedTime = null
where UM_ID = :UM_ID"))
#endif
                {
                    DbCommandHelper.CreateParameter(command, "UM_Sender", info.Sender);
                    DbCommandHelper.CreateParameter(command, "UM_Receiver", info.Receiver);
                    DbCommandHelper.CreateParameter(command, "UM_ID", info.Id);
                    DbCommandHelper.ExecuteNonQuery(command, false);
                }
            }
#if PgSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_Content = @UM_Content
where UM_ID = @UM_ID"))
#endif
#if MsSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_Content = @UM_Content
where UM_ID = @UM_ID"))
#endif
#if MySQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_Content = ?UM_Content
where UM_ID = ?UM_ID"))
#endif
#if ORA
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_Content = :UM_Content
where UM_ID = :UM_ID"))
#endif
            {
                DbCommandHelper.CreateParameter(command, "UM_Content", info.Content);
                DbCommandHelper.CreateParameter(command, "UM_ID", info.Id);
                if (DbCommandHelper.ExecuteNonQuery(command, false) == 0)
                    throw new InvalidOperationException(String.Format(AppSettings.GetValue("未能发送消息: {0}-{1}"), info.Id, info.Content));
            }
        }

        /// <summary>
        /// 接收消息（PULL）
        /// </summary>
        /// <param name="receiver">接收用户</param>
        /// <returns>结果集(消息ID-消息内容)</returns>
        public static IDictionary<long, string> Receive(string receiver)
        {
            return Database.ExecuteGet(Receive, receiver);
        }

        /// <summary>
        /// 接收消息（PULL）
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="receiver">接收用户</param>
        /// <returns>结果集(消息ID-消息内容)</returns>
        public static IDictionary<long, string> Receive(DbConnection connection, string receiver)
        {
            if (String.IsNullOrEmpty(receiver))
                throw new InvalidOperationException("必须指定接收方!");

            Dictionary<long, string> result = new Dictionary<long, string>();
#if PgSQL
            using (DataReader reader = new DataReader(connection, @"
select UM_ID, UM_SendTime, UM_Content
from PH7_UserMessage
where UM_ReceivedTime is null and UM_Receiver = @UM_Receiver
order by UM_CreateTime", CommandBehavior.SingleResult, false))
#endif
#if MsSQL
            using (DataReader reader = new DataReader(connection, @"
select UM_ID, UM_SendTime, UM_Content
from PH7_UserMessage
where UM_ReceivedTime is null and UM_Receiver = @UM_Receiver
order by UM_CreateTime", CommandBehavior.SingleResult, false))
#endif
#if MySQL
            using (DataReader reader = new DataReader(connection, @"
select UM_ID, UM_SendTime, UM_Content
from PH7_UserMessage
where UM_ReceivedTime is null and UM_Receiver = ?UM_Receiver
order by UM_CreateTime", CommandBehavior.SingleResult, false))
#endif
#if ORA
            using (DataReader reader = new DataReader(connection, @"
select UM_ID, UM_SendTime, UM_Content
from PH7_UserMessage
where UM_ReceivedTime is null and UM_Receiver = :UM_Receiver
order by UM_CreateTime", CommandBehavior.SingleResult, false))
#endif
            {
                reader.CreateParameter("UM_Receiver", receiver);
                while (reader.Read())
                    result[reader.IsDBNull(1) ? reader.GetInt64ForDecimal(0) : reader.GetDateTime(1).Ticks] = reader.GetNullableString(2);
            }

            return new ReadOnlyDictionary<long, string>(result);
        }

        /// <summary>
        /// 确认收到
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="burn">是否销毁</param>
        public static void AffirmReceived(long id, bool burn = false)
        {
            Database.Execute(AffirmReceived, id, burn);
        }

        private static void AffirmReceived(DbTransaction transaction, long id, bool burn)
        {
            if (burn)
#if PgSQL
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
delete from PH7_UserMessage
where UM_ID = @UM_ID and UM_SendTime is null"))
#endif
#if MsSQL
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
delete from PH7_UserMessage
where UM_ID = @UM_ID and UM_SendTime is null"))
#endif
#if MySQL
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
delete from PH7_UserMessage
where UM_ID = ?UM_ID and UM_SendTime is null"))
#endif
#if ORA
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
delete from PH7_UserMessage
where UM_ID = :UM_ID and UM_SendTime is null"))
#endif
            {
                DbCommandHelper.CreateParameter(command, "UM_ID", id);
                    if (DbCommandHelper.ExecuteNonQuery(command, false) == 1)
                        return;
            }
#if PgSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_ReceivedTime = now()
where UM_ID = @UM_ID and UM_SendTime is null or UM_SendTime = @UM_SendTime and UM_SendTime is not null"))
#endif
#if MsSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_ReceivedTime = getdate()
where UM_ID = @UM_ID and UM_SendTime is null or UM_SendTime = @UM_SendTime and UM_SendTime is not null"))
#endif
#if MySQL
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_ReceivedTime = now()
where UM_ID = ?UM_ID and UM_SendTime is null or UM_SendTime = ?UM_SendTime and UM_SendTime is not null"))
#endif
#if ORA
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction, @"
update PH7_UserMessage set
  UM_ReceivedTime = sysdate
where UM_ID = :UM_ID and UM_SendTime is null or UM_SendTime = :UM_SendTime and UM_SendTime is not null"))
#endif
            {
                DbCommandHelper.CreateParameter(command, "UM_ID", id);
                DbCommandHelper.CreateParameter(command, "UM_SendTime", new DateTime(id));
                DbCommandHelper.ExecuteNonQuery(command, false);
            }
        }

        #region Initialize

        private static void InitializeTable(Database database)
        {
            database.AddTimedTask("Clear PH7_UserMessage", Clear, 20);
        }

        private static void Clear(DbConnection connection)
        {
#if PgSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
delete from PH7_UserMessage
where UM_CreateTime <= @UM_CreateTime"))
#endif
#if MsSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
delete from PH7_UserMessage
where UM_CreateTime <= @UM_CreateTime"))
#endif
#if MySQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
delete from PH7_UserMessage
where UM_CreateTime <= ?UM_CreateTime"))
#endif
#if ORA
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
delete from PH7_UserMessage
where UM_CreateTime <= :UM_CreateTime"))
#endif
            {
                DbCommandHelper.CreateParameter(command, "UM_CreateTime", DateTime.Now.AddMonths(-ClearMessageDeferMonths));
                DbCommandHelper.ExecuteNonQuery(command, false);
            }
        }
        
        #endregion

        #endregion
    }
}
