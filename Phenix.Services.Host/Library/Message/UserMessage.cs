using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using Phenix.Core.Data.Common;

namespace Phenix.Services.Host.Library.Message
{
    /// <summary>
    /// 用户消息
    /// </summary>
    public static class UserMessage
    {
        #region 方法

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="transaction">DbTransaction</param>
        /// <param name="id">ID</param>
        /// <param name="sender">发送用户</param>
        /// <param name="receiver">接收用户</param>
        /// <param name="content">消息内容</param>
        public static void Send(DbTransaction transaction, long id, string sender, string receiver, string content)
        {
            if (String.IsNullOrEmpty(sender))
                throw new ArgumentException("必须指定发送方!", nameof(sender));
            if (String.IsNullOrEmpty(receiver))
                throw new ArgumentException("必须指定接收方!", nameof(receiver));

            bool existed = false;
            using (DbCommand command = DbCommandHelper.CreateCommand(transaction,
#if PgSQL
                       @"
insert into PH7_UserMessage
  (UM_ID, UM_Sender, UM_Receiver, UM_CreateTime)
values
  (@UM_ID, @UM_Sender, @UM_Receiver, now())"
#endif
#if MsSQL
                       @"
insert into PH7_UserMessage
  (UM_ID, UM_Sender, UM_Receiver, UM_CreateTime)
values
  (@UM_ID, @UM_Sender, @UM_Receiver, getdate())"
#endif
#if MySQL
                       @"
insert into PH7_UserMessage
  (UM_ID, UM_Sender, UM_Receiver, UM_CreateTime)
values
  (?UM_ID, ?UM_Sender, ?UM_Receiver, now())"
#endif
#if ORA
                       @"
insert into PH7_UserMessage
  (UM_ID, UM_Sender, UM_Receiver, UM_CreateTime)
values
  (:UM_ID, :UM_Sender, :UM_Receiver, sysdate)"
#endif
                   ))
            {
                DbCommandHelper.CreateParameter(command, "UM_ID", id);
                DbCommandHelper.CreateParameter(command, "UM_Sender", sender);
                DbCommandHelper.CreateParameter(command, "UM_Receiver", receiver);
                try
                {
                    DbCommandHelper.ExecuteNonQuery(command);
                }
                catch (Exception)
                {
                    existed = true;
                }
            }

            if (existed)
            {
                using (DataReader reader = new DataReader(transaction,
#if PgSQL
                           @"
select UM_Content
from PH7_UserMessage
where UM_ID = @UM_ID",
#endif
#if MsSQL
                           @"
select UM_Content
from PH7_UserMessage
where UM_ID = @UM_ID",
#endif
#if MySQL
                           @"
select UM_Content
from PH7_UserMessage
where UM_ID = ?UM_ID",
#endif
#if ORA
                           @"
select UM_Content
from PH7_UserMessage
where UM_ID = :UM_ID",
#endif
                           CommandBehavior.SingleRow))
                {
                    reader.CreateParameter("UM_ID", id);
                    if (reader.Read())
                        if (String.CompareOrdinal(reader.GetNullableString(0), content) == 0)
                            return;
                }

                using (DbCommand command = DbCommandHelper.CreateCommand(transaction,
#if PgSQL
                           @"
update PH7_UserMessage set
  UM_Sender = @UM_Sender,
  UM_Receiver = @UM_Receiver,
  UM_CreateTime = now(),
  UM_SendTime = now(),
  UM_ReceivedTime = null
where UM_ID = @UM_ID"
#endif
#if MsSQL
                           @"
update PH7_UserMessage set
  UM_Sender = @UM_Sender,
  UM_Receiver = @UM_Receiver,
  UM_CreateTime = getdate(),
  UM_SendTime = getdate(),
  UM_ReceivedTime = null
where UM_ID = @UM_ID"
#endif
#if MySQL
                           @"
update PH7_UserMessage set
  UM_Sender = ?UM_Sender,
  UM_Receiver = ?UM_Receiver,
  UM_CreateTime = now(),
  UM_SendTime = now(),
  UM_ReceivedTime = null
where UM_ID = ?UM_ID"
#endif
#if ORA
                           @"
update PH7_UserMessage set
  UM_Sender = :UM_Sender,
  UM_Receiver = :UM_Receiver,
  UM_CreateTime = sysdate,
  UM_SendTime = sysdate,
  UM_ReceivedTime = null
where UM_ID = :UM_ID"
#endif
                       ))
                {
                    DbCommandHelper.CreateParameter(command, "UM_Sender", sender);
                    DbCommandHelper.CreateParameter(command, "UM_Receiver", receiver);
                    DbCommandHelper.CreateParameter(command, "UM_ID", id);
                    DbCommandHelper.ExecuteNonQuery(command);
                }
            }

            using (DbCommand command = DbCommandHelper.CreateCommand(transaction,
#if PgSQL
                       @"
update PH7_UserMessage set
  UM_Content = @UM_Content
where UM_ID = @UM_ID"
#endif
#if MsSQL
                       @"
update PH7_UserMessage set
  UM_Content = @UM_Content
where UM_ID = @UM_ID"
#endif
#if MySQL
                       @"
update PH7_UserMessage set
  UM_Content = ?UM_Content
where UM_ID = ?UM_ID"
#endif
#if ORA
                       @"
update PH7_UserMessage set
  UM_Content = :UM_Content
where UM_ID = :UM_ID"
#endif
                   ))
            {
                DbCommandHelper.CreateParameter(command, "UM_Content", content);
                DbCommandHelper.CreateParameter(command, "UM_ID", id);
                if (DbCommandHelper.ExecuteNonQuery(command) == 0)
                    throw new InvalidOperationException(String.Format("未能发送消息: {0}-{1}", id, content));
            }
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
                throw new ArgumentException("必须指定接收方!", nameof(receiver));

            Dictionary<long, string> result = new Dictionary<long, string>();
            using (DataReader reader = new DataReader(connection,
#if PgSQL
                       @"
select UM_ID, UM_SendTime, UM_Content
from PH7_UserMessage
where UM_ReceivedTime is null and UM_Receiver = @UM_Receiver
order by UM_CreateTime",
#endif
#if MsSQL
                       @"
select UM_ID, UM_SendTime, UM_Content
from PH7_UserMessage
where UM_ReceivedTime is null and UM_Receiver = @UM_Receiver
order by UM_CreateTime",
#endif
#if MySQL
                       @"
select UM_ID, UM_SendTime, UM_Content
from PH7_UserMessage
where UM_ReceivedTime is null and UM_Receiver = ?UM_Receiver
order by UM_CreateTime",
#endif
#if ORA
                       @"
select UM_ID, UM_SendTime, UM_Content
from PH7_UserMessage
where UM_ReceivedTime is null and UM_Receiver = :UM_Receiver
order by UM_CreateTime",
#endif
                       CommandBehavior.SingleResult))
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
        /// <param name="transaction">DbTransaction</param>
        /// <param name="id">消息ID</param>
        /// <param name="burn">是否销毁</param>
        public static void AffirmReceived(DbTransaction transaction, long id, bool burn)
        {
            if (burn)
                using (DbCommand command = DbCommandHelper.CreateCommand(transaction,
#if PgSQL
                           @"
delete from PH7_UserMessage
where UM_ID = @UM_ID and UM_SendTime is null"
#endif
#if MsSQL
                           @"
delete from PH7_UserMessage
where UM_ID = @UM_ID and UM_SendTime is null"
#endif
#if MySQL
                           @"
delete from PH7_UserMessage
where UM_ID = ?UM_ID and UM_SendTime is null"
#endif
#if ORA
                           @"
delete from PH7_UserMessage
where UM_ID = :UM_ID and UM_SendTime is null"
#endif
                       ))
                {
                    DbCommandHelper.CreateParameter(command, "UM_ID", id);
                    if (DbCommandHelper.ExecuteNonQuery(command) == 1)
                        return;
                }

            using (DbCommand command = DbCommandHelper.CreateCommand(transaction,
#if PgSQL
                       @"
update PH7_UserMessage set
  UM_ReceivedTime = now()
where UM_ID = @UM_ID and UM_SendTime is null or UM_SendTime = @UM_SendTime and UM_SendTime is not null"
#endif
#if MsSQL
                       @"
update PH7_UserMessage set
  UM_ReceivedTime = getdate()
where UM_ID = @UM_ID and UM_SendTime is null or UM_SendTime = @UM_SendTime and UM_SendTime is not null"
#endif
#if MySQL
                       @"
update PH7_UserMessage set
  UM_ReceivedTime = now()
where UM_ID = ?UM_ID and UM_SendTime is null or UM_SendTime = ?UM_SendTime and UM_SendTime is not null"
#endif
#if ORA
                       @"
update PH7_UserMessage set
  UM_ReceivedTime = sysdate
where UM_ID = :UM_ID and UM_SendTime is null or UM_SendTime = :UM_SendTime and UM_SendTime is not null"
#endif
                   ))
            {
                DbCommandHelper.CreateParameter(command, "UM_ID", id);
                DbCommandHelper.CreateParameter(command, "UM_SendTime", new DateTime(id));
                DbCommandHelper.ExecuteNonQuery(command);
            }
        }

        /// <summary>
        /// 清理报文
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="sender">发送用户</param>
        /// <param name="clearMessageDeferMonths">清理几个月前的消息</param>
        public static void Clear(DbConnection connection, string sender, int clearMessageDeferMonths)
        {
            using (DbCommand command = DbCommandHelper.CreateCommand(connection,
#if PgSQL
                       @"
delete from PH7_UserMessage
where UM_Sender = @UM_Sender and UM_CreateTime <= @UM_CreateTime"
#endif
#if MsSQL
                       @"
delete from PH7_UserMessage
where UM_Sender = @UM_Sender and UM_CreateTime <= @UM_CreateTime"
#endif
#if MySQL
                       @"
delete from PH7_UserMessage
where UM_Sender = ?UM_Sender and UM_CreateTime <= ?UM_CreateTime"
#endif
#if ORA
                       @"
delete from PH7_UserMessage
where UM_Sender = :UM_Sender and UM_CreateTime <= :UM_CreateTime"
#endif
                   ))
            {
                DbCommandHelper.CreateParameter(command, "UM_Sender", sender);
                DbCommandHelper.CreateParameter(command, "UM_CreateTime", DateTime.Now.AddMonths(-clearMessageDeferMonths));
                DbCommandHelper.ExecuteNonQuery(command);
            }
        }

        #endregion
    }
}