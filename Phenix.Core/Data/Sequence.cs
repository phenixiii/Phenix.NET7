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
    /// 64λ���
    /// ��Database��PH7_SequenceMarker��֤��ŵ�Ψһ��
    /// </summary>
    public sealed class Sequence
    {
        internal Sequence(Database database)
        {
            _database = database;
        }

        #region ����

        #region ������

        private static int? _clearMarkerDeferMonths;

        /// <summary>
        /// ��������ǰ��Marker
        /// Ĭ�ϣ�3(>=3)
        /// </summary>
        public static int ClearMarkerDeferMonths
        {
            get { return new[] {AppSettings.GetProperty(ref _clearMarkerDeferMonths, 3), 3}.Max(); }
            set { AppSettings.SetProperty(ref _clearMarkerDeferMonths, new[] {value, 3}.Max()); }
        }

        #endregion

        #region ����Դ

        private readonly Database _database;

        /// <summary>
        /// ���ݿ����
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
        /// ֵ
        /// С��999999999999999
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

        #region ����

        private int LoadMarker(DbConnection connection)
        {
            bool fetched = false;
            while (true)
            {
#if PgSQL
                using (DataReader reader = new DataReader(connection, @"
select SM_ID
from PH7_SequenceMarker
where SM_Address = @SM_Address", CommandBehavior.SingleRow, false))
#endif
#if MsSQL
                using (DataReader reader = new DataReader(connection, @"
select SM_ID
from PH7_SequenceMarker
where SM_Address = @SM_Address", CommandBehavior.SingleRow, false))
#endif
#if MySQL
                using (DataReader reader = new DataReader(connection, @"
select SM_ID
from PH7_SequenceMarker
where SM_Address = ?SM_Address", CommandBehavior.SingleRow, false))
#endif
#if ORA
                using (DataReader reader = new DataReader(connection, @"
select SM_ID
from PH7_SequenceMarker
where SM_Address = :SM_Address", CommandBehavior.SingleRow, false))
#endif
                {
                    reader.CreateParameter("SM_Address", NetConfig.LocalAddress);
                    if (reader.Read())
                        return reader.GetInt32(0);
                }

                if (fetched)
                    break;

                fetched = true;
                int recordCount = Convert.ToInt32(DbCommandHelper.ExecuteScalar(connection, "select count(*) from PH7_SequenceMarker"));

#if PgSQL
                using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
insert into PH7_SequenceMarker
  (SM_ID, SM_Address, SM_ActiveTime)
values
  (@SM_ID, @SM_Address, now())"))
#endif
#if MsSQL
                using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
insert into PH7_SequenceMarker
  (SM_ID, SM_Address, SM_ActiveTime)
values
  (@SM_ID, @SM_Address, getdate())"))
#endif
#if MySQL
                using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
insert into PH7_SequenceMarker
  (SM_ID, SM_Address, SM_ActiveTime)
values
  (?SM_ID, ?SM_Address, now())"))
#endif
#if ORA
                using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
insert into PH7_SequenceMarker
  (SM_ID, SM_Address, SM_ActiveTime)
values
  (:SM_ID, :SM_Address, sysdate)"))
#endif
                {
                    for (int i = recordCount; i < 1000; i++)
                    {
                        DbCommandHelper.CreateParameter(command, "SM_ID", i);
                        DbCommandHelper.CreateParameter(command, "SM_Address", NetConfig.LocalAddress);
                        try
                        {
                            if (DbCommandHelper.ExecuteNonQuery(command, false) == 1)
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
                            if (DbCommandHelper.ExecuteNonQuery(command, false) == 1)
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
#if PgSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
update PH7_SequenceMarker set
  SM_ActiveTime = now()
where SM_Address = @SM_Address"))
#endif
#if MsSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
update PH7_SequenceMarker set
  SM_ActiveTime = getdate()
where SM_Address = @SM_Address"))
#endif
#if MySQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
update PH7_SequenceMarker set
  SM_ActiveTime = now()
where SM_Address = ?SM_Address"))
#endif
#if ORA
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
update PH7_SequenceMarker set
  SM_ActiveTime = sysdate
where SM_Address = :SM_Address"))
#endif
            {
                DbCommandHelper.CreateParameter(command, "SM_Address", NetConfig.LocalAddress);
                DbCommandHelper.ExecuteNonQuery(command, false);
            }
        }

        private void ClearSequenceMarker(DbConnection connection)
        {
#if PgSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
delete from PH7_SequenceMarker
where SM_ActiveTime <= @SM_ActiveTime"))
#endif
#if MsSQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
delete from PH7_SequenceMarker
where SM_ActiveTime <= @SM_ActiveTime"))
#endif
#if MySQL
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
delete from PH7_SequenceMarker
where SM_ActiveTime <= ?SM_ActiveTime"))
#endif
#if ORA
            using (DbCommand command = DbCommandHelper.CreateCommand(connection, @"
delete from PH7_SequenceMarker
where SM_ActiveTime <= :SM_ActiveTime"))
#endif
            {
                DbCommandHelper.CreateParameter(command, "SM_ActiveTime", DateTime.Now.AddMonths(-ClearMarkerDeferMonths));
                DbCommandHelper.ExecuteNonQuery(command, false);
            }
        }

        #endregion
    }
}