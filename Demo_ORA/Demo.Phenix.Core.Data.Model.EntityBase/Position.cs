using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Log;
using Phenix.Core.SyncCollections;

namespace Demo
{
    /// <summary>
    /// 岗位资料
    /// </summary>
    [Serializable]
    public sealed class Position : EntityBase<Position>
    {
        private Position()
        {
            //for fetch
        }

        [Newtonsoft.Json.JsonConstructor]
        private Position(long id, string name, IList<string> roles)
        {
            _id = id;
            _name = name;
            _roles = roles != null ? new ReadOnlyCollection<string>(roles) : null;
        }

        #region 工厂

        private static readonly object _lock = new object();
        private static SynchronizedDictionary<long, Position> _cache;

        private static void Initialize()
        {
            if (_cache == null)
            {
                lock (_lock)
                    if (_cache == null)
                    {
                        InitializeTable();
                        _cache = new SynchronizedDictionary<long, Position>();
                        AddRenovatorTrigger(p => p.Id,
                            (tableName, primaryKeyValue, executeTime, executeAction) => { _cache.Remove(primaryKeyValue); });
                    }

                Thread.MemoryBarrier();
            }
        }

        /// <summary>
        /// 新增岗位资料
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="roles">角色数组</param>
        /// <returns>岗位资料</returns>
        public static Position New(string name, string[] roles)
        {
            Initialize();

            Position result = new Position(Sequence.Value, name, roles);
            result.Insert(p => p.Id);
            _cache.Add(result.Id, result);
            return result;
        }

        /// <summary>
        /// 获取岗位资料
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        /// <returns>岗位资料</returns>
        public static Position Fetch(long id, int resetHoursLater = 8)
        {
            Initialize();

            return _cache.GetValue(id,
                () =>
                {
                    Position result = Select(p => p.Id == id).SingleOrDefault();
                    if (result != null)
                        result._invalidTime = resetHoursLater == 0 ? (DateTime?) null : DateTime.Now.AddHours(resetHoursLater);
                    return result;
                },
                value => value == null || resetHoursLater < 0 || resetHoursLater > 0 && (!value._invalidTime.HasValue || value._invalidTime < DateTime.Now));
        }

        /// <summary>
        /// 获取全部岗位资料
        /// </summary>
        /// <returns>岗位资料清单</returns>
        public static IList<Position> FetchAll()
        {
            Initialize();

            return Select(Ascending(p => p.Name));
        }

        /// <summary>
        /// 重置所有缓存
        /// </summary>
        public static void ResetAll()
        {
            _cache.Clear();
        }

        #endregion

        #region 属性

        private long _id;

        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (Update(p => p.Id, SetProperty(p => p.Name, value)) == 1)
                {
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }

        private ReadOnlyCollection<string> _roles;

        /// <summary>
        /// 角色清单
        /// </summary>
        public IList<string> Roles
        {
            get { return _roles; }
            set
            {
                if (Update(p => p.Id, SetProperty(p => p.Roles, value)) == 1)
                {
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }

        [NonSerialized]
        private DateTime? _invalidTime;

        /// <summary>
        /// 失效时间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime? InvalidTime
        {
            get { return _invalidTime; }
        }

        #endregion

        #region 方法
        
        /// <summary>
        /// 删除岗位资料
        /// </summary>
        public void Delete()
        {
            if (DeleteRecord(CriteriaExpression.Where<Position>(p => p.Id == Id).NotExists<User>(p => p.PositionId)) == 0)
                throw new InvalidOperationException(String.Format("未能删除 {0} 岗位, 可能已被用在了用户管理上", Name));
            Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Delete));
            _cache.Remove(Id);
        }

        private static void InitializeTable()
        {
            if (Sheet == null)
            {
                Database.Execute(InitializeTable);
                Database.ClearCache();
            }
        }

        private static void InitializeTable(DbConnection connection)
        {
            try
            {
                DbCommandHelper.ExecuteNonQuery(connection, @"
CREATE TABLE PH7_Position (
  PT_ID NUMERIC(15) NOT NULL,
  PT_Name VARCHAR(100) NOT NULL,
  PT_Roles VARCHAR(4000) NOT NULL,
  PRIMARY KEY(PT_ID),
  UNIQUE(PT_Name)
)", false);
            }
            catch (Exception ex)
            {
                Task.Run(() => EventLog.SaveLocal("Initialize PH7_Position Table", ex));
                throw;
            }
        }

        #endregion
    }
}