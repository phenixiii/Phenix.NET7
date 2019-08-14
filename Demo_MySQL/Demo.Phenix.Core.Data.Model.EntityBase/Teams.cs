using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Validity;
using Phenix.Core.Log;
using Phenix.Core.SyncCollections;

namespace Demo
{
    /// <summary>
    /// 团体资料
    /// </summary>
    [Serializable]
    public sealed class Teams : EntityBase<Teams>, IValidation
    {
        private Teams()
        {
            //for fetch
        }

        [Newtonsoft.Json.JsonConstructor]
        private Teams(long id, string name, long rootId, long parentId, IList<Teams> allSubTeams)
        {
            _id = id;
            _name = name;
            _rootId = rootId;
            if (id == rootId)
            {
                _root = this;
                if (allSubTeams != null)
                {
                    _allSubTeams = new List<Teams>(allSubTeams) {this};
                    foreach (Teams item in allSubTeams)
                        item._root = this;
                }
                else
                    _allSubTeams = new List<Teams>() {this};
            }
            else
                _parentId = parentId;
        }

        private Teams(long id, string name, long rootId, long parentId, IList<Teams> allSubTeam, DateTime? invalidTime)
            : this(id, name, rootId, parentId, allSubTeam)
        {
            _invalidTime = invalidTime;
        }

        private Teams(long id, string name, Teams parent)
            : this(id, name, parent._rootId, parent._id, null)
        {
            _root = parent._root;
            _parent = parent;
        }

        #region 工厂

        private static readonly object _lock = new object();
        private static SynchronizedDictionary<long, Teams> _rootCache;

        private static void Initialize()
        {
            if (_rootCache == null)
            {
                lock (_lock)
                    if (_rootCache == null)
                    {
                        InitializeTable();
                        _rootCache = new SynchronizedDictionary<long, Teams>();
                        AddRenovatorTrigger(p => p.Id,
                            (tableName, primaryKeyValue, executeTime, executeAction) =>
                            {
                                if (!_rootCache.Remove(primaryKeyValue))
                                    _rootCache.Clear();
                            });
                    }

                Thread.MemoryBarrier();
            }
        }

        /// <summary>
        /// 新增团体资料
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="parent">父层团体</param>
        /// <returns>团体资料</returns>
        public static Teams New(string name, Teams parent = null)
        {
            Initialize();

            long id = Sequence.Value;
            Teams result = parent != null
                ? new Teams(id, name, parent)
                : new Teams(id, name, id, 0, null);
            result.Insert(p => p.Id);
            if (parent != null)
            {
                parent._root._allSubTeams.Add(result);
                parent._subTeams = null;
                Task.Run(() => parent._root.SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                _rootCache.Remove(parent._root.Id);
            }
            else
                _rootCache.Add(result.Id, result);

            return result;
        }

        /// <summary>
        /// 获取顶层团体资料
        /// </summary>
        /// <param name="rootId">顶层团体ID</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        /// <returns>团体资料</returns>
        public static Teams FetchRoot(long rootId, int resetHoursLater = 8)
        {
            Initialize();

            return _rootCache.GetValue(rootId,
                () =>
                {
                    Teams result = null;
                    List<Teams> allSubTeams = new List<Teams>();
                    foreach (Teams item in Select(p => p.RootId == rootId, Ascending(p => p.ParentId)))
                        if (item.Id == item.RootId && item.RootId == rootId)
                            result = item;
                        else
                            allSubTeams.Add(item);
                    if (result == null)
                        throw new ArgumentException(String.Format("未检索到ID为 {0} 的顶层团体资料", rootId), nameof(rootId));
                    return new Teams(result.Id, result.Name, result.RootId, result.ParentId, allSubTeams, resetHoursLater == 0 ? (DateTime?) null : DateTime.Now.AddHours(resetHoursLater));
                },
                value => value == null || resetHoursLater < 0 || resetHoursLater > 0 && (!value._invalidTime.HasValue || value._invalidTime < DateTime.Now));
        }

        /// <summary>
        /// 获取顶层团体名称
        /// </summary>
        /// <param name="likeName">类似名称</param>
        /// <returns>顶层团体名称及ID清单</returns>
        public static IDictionary<string, long> FetchRootNames(string likeName = null)
        {
            Initialize();

            Dictionary<string, long> result = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (Teams item in Select(p => p.Id == p.RootId && p.Name.Contains(likeName), Ascending(p => p.Name)))
                result.Add(item.Name, item.Id);
            return result;
        }

        /// <summary>
        /// 重置团体资料所有缓存
        /// 默认8小时自动重置一次
        /// </summary>
        public static void ResetAll()
        {
            _rootCache.Clear();
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
                if (UpdateProperty(p => p.Id, SetProperty(p => p.Name, value)) == 1)
                {
                    Task.Run(() => _root.SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _rootCache.Remove(_root.Id);
                }
            }
        }

        private long _rootId;

        /// <summary>
        /// 顶层团体ID
        /// </summary>
        public long RootId
        {
            get { return _rootId; }
        }

        [NonSerialized]
        private Teams _root;

        /// <summary>
        /// 顶层团体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Teams Root
        {
            get { return _root; }
        }

        private long _parentId;

        /// <summary>
        /// 父层团体ID
        /// </summary>
        public long ParentId
        {
            get { return _parentId; }
        }

        [NonSerialized]
        private Teams _parent;

        /// <summary>
        /// 父层团体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Teams Parent
        {
            get
            {
                if (_id == _rootId)
                    return null;
                return _parent ?? (_parent = _root.FindSubTeams(_parentId));
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "不允许空挂父层团体");
                if (value._rootId != _rootId)
                    throw new ArgumentException("仅允许在同一顶层团体下切挂", nameof(value));
                if (IsSubTeams(value))
                    throw new ArgumentException("不允许挂在自己下层的团体下", nameof(value));

                Teams oldValue = _parent;
                if (UpdateProperty(p => p.Id, SetProperty(p => p.ParentId, value.Id)) == 1)
                {
                    _parent = value;
                    value._subTeams = null;
                    oldValue._subTeams = null;
                    Task.Run(() => _root.SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _rootCache.Remove(_root.Id);
                }
            }
        }

        private readonly List<Teams> _allSubTeams;

        /// <summary>
        /// 子团体(仅限this=Root时)
        /// </summary>
        public IList<Teams> AllSubTeams
        {
            get { return _allSubTeams != null ? _allSubTeams.AsReadOnly() : null; }
        }

        [NonSerialized]
        private ReadOnlyCollection<Teams> _subTeams;

        /// <summary>
        /// 子层团体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IList<Teams> SubTeams
        {
            get
            {
                if (_subTeams == null)
                {
                    List<Teams> result = new List<Teams>();
                    foreach (Teams item in _root._allSubTeams)
                        if (item._parentId == _id)
                        {
                            item._parent = this;
                            result.Add(item);
                        }

                    _subTeams = result.AsReadOnly();
                }

                return _subTeams;
            }
        }

        [NonSerialized]
        private readonly DateTime? _invalidTime;

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
        /// 是否子层团体
        /// </summary>
        /// <param name="teams">团体</param>
        public bool IsSubTeams(Teams teams)
        {
            return FindSubTeams(teams.Id) != null;
        }

        /// <summary>
        /// 寻找子层团体
        /// </summary>
        /// <param name="id">团体ID</param>
        public Teams FindSubTeams(long id)
        {
            foreach (Teams item in SubTeams)
            {
                if (item.Id == id)
                    return item;
                Teams result = item.FindSubTeams(id);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// 寻找子层团体
        /// </summary>
        /// <param name="name">名称</param>
        public Teams FindSubTeams(string name)
        {
            foreach (Teams item in SubTeams)
            {
                if (item.Name == name)
                    return item;
                Teams result = item.FindSubTeams(name);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// 删除团体资料
        /// </summary>
        public void Delete()
        {
            if (DeleteRecord(CriteriaExpression.Where<Teams>(p => p.Id == Id).NotExists<User>(p => p.TeamsId)) == 0)
                throw new InvalidOperationException(String.Format("未能删除团体({0}), 可能已被用在了用户管理上", Name));

            _root._allSubTeams.Remove(this);
            if (_parent != null)
                _parent._subTeams = null;
            Task.Run(() => _root.SaveRenovateLog(p => p.Id, ExecuteAction.Update));
            _rootCache.Remove(_root.Id);
        }

        private static void InitializeTable()
        {
            if (DefaultSheet == null)
            {
                DefaultDatabase.Execute(InitializeTable);
                DefaultDatabase.ClearCache();
            }
        }

        private static void InitializeTable(DbConnection connection)
        {
            try
            {
                DbCommandHelper.ExecuteNonQuery(connection, @"
CREATE TABLE PH7_Teams (
  TM_ID NUMERIC(15) NOT NULL,
  TM_Name VARCHAR(100) NOT NULL,
  TM_Root_ID NUMERIC(15) NOT NULL,
  TM_Parent_ID NUMERIC(15) NOT NULL,
  PRIMARY KEY(TM_ID),
  UNIQUE(TM_Root_ID, TM_Name),
  UNIQUE(TM_Parent_ID, TM_Name)
)", false);
            }
            catch (Exception ex)
            {
                Task.Run(() => EventLog.SaveLocal("Initialize PH7_Teams Table", ex));
                throw;
            }
        }

        ValidationResult IValidation.Validate(ExecuteAction executeAction)
        {
            if (executeAction == ExecuteAction.Delete)
            {
                if (SubTeams.Count > 0)
                    return new ValidationResult(String.Format("不允许删除中层团体({0})", Name));
            }

            return null;
        }
        
        #endregion
    }
}