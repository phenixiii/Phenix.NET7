using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Phenix.Core.Data.Common;
using Phenix.Core.Net;

namespace Phenix.Core.Data
{
    /// <summary>
    /// 64位序号
    /// 由Database的PH7_SequenceMarker表保证序号的唯一性
    /// </summary>
    public sealed class Sequence
    {
        internal Sequence(Database database)
        {
            _database = database;
        }

        #region 属性

        #region 配置项

        private static int? _clearMarkerDeferMonths;

        /// <summary>
        /// 清理几个月前的Marker
        /// 默认：3(>=3)
        /// </summary>
        public static int ClearMarkerDeferMonths
        {
            get { return new[] { AppSettings.GetProperty(ref _clearMarkerDeferMonths, 3), 3 }.Max(); }
            set { AppSettings.SetProperty(ref _clearMarkerDeferMonths, new[] { value, 3 }.Max()); }
        }

        #endregion

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

        private readonly long _minValue = DateTime.MinValue.AddYears(2000).Ticks;
        private int? _marker;
        private long _value;

        /// <summary>
        /// 值
        /// 小于999999999999999
        /// </summary>
        public long Value
        {
            get
            {
                if (!_marker.HasValue)
                {
                    lock (_database)
                        if (!_marker.HasValue)
                        {
                            _marker = _database.ExecuteGet(LoadMarker);
                            InitializeTable();
                        }

                    Thread.MemoryBarrier();
                }

                long i = (DateTime.Now.Ticks - _minValue) / 10000 * 1000 + _marker.Value;
                lock (_database)
                {
                    _value = i > _value ? i : _value + 1000;
                    return _value;
                }
            }
        }

        #endregion

        #region 方法

        private int LoadMarker(DbConnection connection)
        {
            bool fetched = false;
            while (true)
            {
                using (DataReader reader = new DataReader(connection,
#if PgSQL
                           @"
select SM_ID
from PH7_SequenceMarker
where SM_Address = @SM_Address",
#endif
#if MsSQL
                           @"
select SM_ID
from PH7_SequenceMarker
where SM_Address = @SM_Address",
#endif
#if MySQL
                           @"
select SM_ID
from PH7_SequenceMarker
where SM_Address = ?SM_Address",
#endif
#if ORA
                           @"
select SM_ID
from PH7_SequenceMarker
where SM_Address = :SM_Address",
#endif
                           CommandBehavior.SingleRow))
                {
                    reader.CreateParameter("SM_Address", NetConfig.LocalAddress);
                    if (reader.Read())
                        return reader.GetInt32(0);
                }

                if (fetched)
                    break;

                fetched = true;
                int recordCount = Convert.ToInt32(DbCommandHelper.ExecuteScalar(connection, "select count(*) from PH7_SequenceMarker"));

                using (DbCommand command = DbCommandHelper.CreateCommand(connection,
#if PgSQL
                           @"
insert into PH7_SequenceMarker
  (SM_ID, SM_Address, SM_ActiveTime)
values
  (@SM_ID, @SM_Address, now())"
#endif
#if MsSQL
                           @"
insert into PH7_SequenceMarker
  (SM_ID, SM_Address, SM_ActiveTime)
values
  (@SM_ID, @SM_Address, getdate())"
#endif
#if MySQL
                           @"
insert into PH7_SequenceMarker
  (SM_ID, SM_Address, SM_ActiveTime)
values
  (?SM_ID, ?SM_Address, now())"
#endif
#if ORA
                           @"
insert into PH7_SequenceMarker
  (SM_ID, SM_Address, SM_ActiveTime)
values
  (:SM_ID, :SM_Address, sysdate)"
#endif
                       ))
                {
                    for (int i = recordCount; i < 1000; i++)
                    {
                        DbCommandHelper.CreateParameter(command, "SM_ID", i);
                        DbCommandHelper.CreateParameter(command, "SM_Address", NetConfig.LocalAddress);
                        try
                        {
                            if (DbCommandHelper.ExecuteNonQuery(command) == 1)
                                return i;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    for (int i = 0; i < recordCount; i++)
                    {
                        DbCommandHelper.CreateParameter(command, "SM_ID", i);
                        DbCommandHelper.CreateParameter(command, "SM_Address", NetConfig.LocalAddress);
                        try
                        {
                            if (DbCommandHelper.ExecuteNonQuery(command) == 1)
                                return i;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }

            int result = Math.Abs(NetConfig.LocalAddress.GetHashCode() ^ Process.GetCurrentProcess().Id);
            while (result >= 1000)
                result = result / 10;
            return result;
        }

        private void InitializeTable()
        {
            _database.AddTimedTask("ActiveSequenceMarker", ActiveSequenceMarker, 5);
            _database.AddTimedTask("ClearSequenceMarker", ClearSequenceMarker, 20);
        }

        private static void ActiveSequenceMarker(DbConnection connection)
        {
            using (DbCommand command = DbCommandHelper.CreateCommand(connection,
#if PgSQL
                       @"
update PH7_SequenceMarker set
  SM_ActiveTime = now()
where SM_Address = @SM_Address"
#endif
#if MsSQL
                       @"
update PH7_SequenceMarker set
  SM_ActiveTime = getdate()
where SM_Address = @SM_Address"
#endif
#if MySQL
                       @"
update PH7_SequenceMarker set
  SM_ActiveTime = now()
where SM_Address = ?SM_Address"
#endif
#if ORA
                       @"
update PH7_SequenceMarker set
  SM_ActiveTime = sysdate
where SM_Address = :SM_Address"
#endif
                   ))
            {
                DbCommandHelper.CreateParameter(command, "SM_Address", NetConfig.LocalAddress);
                DbCommandHelper.ExecuteNonQuery(command);
            }
        }

        private void ClearSequenceMarker(DbConnection connection)
        {
            using (DbCommand command = DbCommandHelper.CreateCommand(connection,
#if PgSQL
                       @"
delete from PH7_SequenceMarker
where SM_ActiveTime <= @SM_ActiveTime"
#endif
#if MsSQL
                       @"
delete from PH7_SequenceMarker
where SM_ActiveTime <= @SM_ActiveTime"
#endif
#if MySQL
                       @"
delete from PH7_SequenceMarker
where SM_ActiveTime <= ?SM_ActiveTime"
#endif
#if ORA
                       @"
delete from PH7_SequenceMarker
where SM_ActiveTime <= :SM_ActiveTime"
#endif
                   ))
            {
                DbCommandHelper.CreateParameter(command, "SM_ActiveTime", DateTime.Now.AddMonths(-ClearMarkerDeferMonths));
                DbCommandHelper.ExecuteNonQuery(command);
            }
        }

        #endregion
    }
}