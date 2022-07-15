using System;
using System.Data;
using System.Data.Common;
using Phenix.Core.Data.Common;

namespace Phenix.Core.Data
{
    /// <summary>
    /// 64位增量
    /// </summary> 
    public sealed class Increment
    {
        internal Increment(Database database)
        {
            _database = database;
        }

        #region 属性

        #region 数据源

        private readonly Database _database;

        /// <summary>
        /// 数据库入口
        /// </summary>
        public Database Database
        {
            get { return _database; }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 取下一个
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="initialValue">初值</param>
        /// <returns>值</returns>
        public long GetNext(string key, long initialValue = 1)
        {
            return _database.ExecuteGet(LoadNext, key, initialValue);
        }

        private long LoadNext(DbConnection connection, string key, long initialValue)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("必须指定key值!", nameof(key));

            long? oldValue = null;
            using (DataReader reader = new DataReader(connection,
#if PgSQL
                       @"
select IC_Value
from PH7_Increment
where IC_Key = @IC_Key",
#endif
#if MsSQL
                       @"
select IC_Value
from PH7_Increment
where IC_Key = @IC_Key",
#endif
#if MySQL
                       @"
select IC_Value
from PH7_Increment
where IC_Key = ?IC_Key",
#endif
#if ORA
                       @"
select IC_Value
from PH7_Increment
where IC_Key = :IC_Key",
#endif
                       CommandBehavior.SingleRow))
            {
                reader.CreateParameter("IC_Key", key);
                if (reader.Read())
                    oldValue = reader.GetInt64ForDecimal(0);
            }

            if (!oldValue.HasValue)
            {
                using (DbCommand command = DbCommandHelper.CreateCommand(connection,
#if PgSQL
                           @"
insert into PH7_Increment
  (IC_Key, IC_Value, IC_Time)
values
  (@IC_Key, @IC_Value, now())"
#endif
#if MsSQL
                           @"
insert into PH7_Increment
  (IC_Key, IC_Value, IC_Time)
values
  (@IC_Key, @IC_Value, getdate())"
#endif
#if MySQL
                           @"
insert into PH7_Increment
  (IC_Key, IC_Value, IC_Time)
values
  (?IC_Key, ?IC_Value, now())"
#endif
#if ORA
                           @"
insert into PH7_Increment
  (IC_Key, IC_Value, IC_Time)
values
  (:IC_Key, :IC_Value, sysdate)"
#endif
                       ))
                {
                    DbCommandHelper.CreateParameter(command, "IC_Key", key);
                    DbCommandHelper.CreateParameter(command, "IC_Value", initialValue);
                    try
                    {
                        DbCommandHelper.ExecuteNonQuery(command);
                        return initialValue;
                    }
                    catch (Exception)
                    {
                        oldValue = initialValue;
                    }
                }
            }

            using (DbCommand command = DbCommandHelper.CreateCommand(connection,
#if PgSQL
                       @"
update PH7_Increment set
  IC_Value = @New_IC_Value,
  IC_Time = now()
where IC_Key = @IC_Key
  and IC_Value = @Old_IC_Value"
#endif
#if MsSQL
                       @"
update PH7_Increment set
  IC_Value = @New_IC_Value,
  IC_Time = getdate()
where IC_Key = @IC_Key
  and IC_Value = @Old_IC_Value"
#endif
#if MySQL
                       @"
update PH7_Increment set
  IC_Value = ?New_IC_Value,
  IC_Time = now()
where IC_Key = ?IC_Key
  and IC_Value = ?Old_IC_Value"
#endif
#if ORA
                       @"
update PH7_Increment set
  IC_Value = :New_IC_Value,
  IC_Time = sysdate
where IC_Key = :IC_Key
  and IC_Value = :Old_IC_Value"
#endif
                   ))
            {
                long newValue = oldValue.Value + 1;
                DbParameter newValueParameter = DbCommandHelper.CreateParameter(command, "New_IC_Value", newValue);
                DbCommandHelper.CreateParameter(command, "IC_Key", key);
                DbParameter oldValueParameter = DbCommandHelper.CreateParameter(command, "Old_IC_Value", oldValue);
                do
                {
                    if (DbCommandHelper.ExecuteNonQuery(command) == 1)
                        return newValue;
                    oldValue = newValue;
                    newValue = newValue + 1;
                    newValueParameter.Value = newValue;
                    oldValueParameter.Value = oldValue;
                } while (true);
            }
        }

        #endregion
    }
}