using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Data.Model;
using Phenix.Core.Log;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.SyncCollections;

namespace Demo
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public sealed class User : EntityBase<User>
    {
        private User()
        {
            //for fetch
        }

        [Newtonsoft.Json.JsonConstructor]
        private User(long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            string requestAddress, int requestFailureCount, DateTime? requestFailureTime,
            long? rootTeamsId, long? teamsId, long? positionId,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime)
        {
            _id = id;
            _name = name;
            _phone = phone;
            _eMail = eMail;
            _regAlias = regAlias;
            _regTime = regTime;
            _requestAddress = requestAddress;
            _requestFailureCount = requestFailureCount;
            _requestFailureTime = requestFailureTime;
            _rootTeamsId = rootTeamsId;
            _teamsId = teamsId;
            _positionId = positionId;
            _locked = locked;
            _lockedTime = lockedTime;
            _disabled = disabled;
            _disabledTime = disabledTime;
        }

        private User(long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            string password, string dynamicPassword, DateTime dynamicPasswordCreateTime,
            string requestAddress, int requestFailureCount, DateTime? requestFailureTime,
            long? rootTeamsId, long? teamsId, long? positionId,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime,
            DateTime? invalidTime)
            : this(id, name, phone, eMail, regAlias, regTime, requestAddress, requestFailureCount, requestFailureTime, rootTeamsId, teamsId, positionId, locked, lockedTime, disabled, disabledTime)
        {
            _password = password;
            _dynamicPassword = dynamicPassword;
            _dynamicPasswordCreateTime = dynamicPasswordCreateTime;
            _invalidTime = invalidTime;
        }

        #region 工厂

        private static readonly object _lock = new object();
        private static SynchronizedDictionary<long, User> _cache;

        private static void Initialize()
        {
            if (_cache == null)
            {
                lock (_lock)
                    if (_cache == null)
                    {
                        InitializeTable();
                        _cache = new SynchronizedDictionary<long, User>();
                        AddRenovatorTrigger(p => p.Id,
                            (tableName, primaryKeyValue, executeTime, executeAction) => { _cache.Remove(primaryKeyValue); });
                    }

                Thread.MemoryBarrier();
            }
        }

        /// <summary>
        /// 新增用户资料
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="phone">手机</param>
        /// <param name="eMail">邮箱</param>
        /// <param name="regAlias">注册昵称</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="password">登录口令(一般通过邮箱发送给到用户)</param>
        /// <param name="dynamicPassword">动态口令(6位数字一般作为验证码用短信发送给到用户)</param>
        /// <returns>用户资料</returns>
        public static User New(string name, string phone, string eMail, string regAlias, string requestAddress, out string password, out string dynamicPassword)
        {
            Initialize();

            password = Guid.NewGuid().ToString().Substring(0, 10);
            dynamicPassword = new Random().Next(100000, 1000000).ToString();
            User result = new User(Sequence.Value, name, phone, eMail, regAlias, DateTime.Now,
                MD5CryptoTextProvider.ComputeHash(password), MD5CryptoTextProvider.ComputeHash(dynamicPassword), DateTime.Now,
                requestAddress, 0, null,
                null, null, null,
                false, null, false, null, null);
            result.Insert(p => p.Id);
            _cache.Add(result.Id, result);
            return result;
        }

        /// <summary>
        /// 获取用户资料
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        /// <returns>用户资料</returns>
        public static User Fetch(long id, int resetHoursLater = 8)
        {
            Initialize();

            return _cache.GetValue(id,
                () =>
                {
                    User result = Select(p => p.Id == id).SingleOrDefault();
                    if (result != null)
                        result._invalidTime = resetHoursLater == 0 ? (DateTime?) null : DateTime.Now.AddHours(resetHoursLater);
                    return result;
                },
                value => value == null || resetHoursLater < 0 || resetHoursLater > 0 && (!value._invalidTime.HasValue || value._invalidTime < DateTime.Now));
        }

        /// <summary>
        /// 获取用户资料
        /// </summary>
        /// <param name="name">登录名</param>
        /// <returns>用户资料</returns>
        public static User Fetch(string name)
        {
            Initialize();

            return Select(p => p.Name == name).SingleOrDefault();
        }

        /// <summary>
        /// 重置用户资料所有缓存
        /// 默认8小时自动重置一次
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
        /// 登录名
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        private string _phone;

        /// <summary>
        /// 手机
        /// </summary>
        public string Phone
        {
            get { return _phone; }
            set
            {
                if (UpdateProperty(p => p.Id, SetProperty(p => p.Phone, value)) == 1)
                {
                    _phone = value;
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }


        private string _eMail;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string EMail
        {
            get { return _eMail; }
            set
            {
                if (UpdateProperty(p => p.Id, SetProperty(p => p.EMail, value)) == 1)
                {
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }

        private string _regAlias;

        /// <summary>
        /// 注册昵称
        /// </summary>
        public string RegAlias
        {
            get { return _regAlias; }
            set
            {
                if (UpdateProperty(p => p.Id, SetProperty(p => p.RegAlias, value)) == 1)
                {
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }

        private DateTime _regTime;

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime RegTime
        {
            get { return _regTime; }
        }

        [NonSerialized]
        private string _password;

        /// <summary>
        /// 登录口令(散列值)
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Password
        {
            get { return _password; }
            private set { _password = value; }
        }

        [NonSerialized]
        private string _dynamicPassword;

        /// <summary>
        /// 动态口令(散列值)
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string DynamicPassword
        {
            get { return _dynamicPassword; }
        }

        [NonSerialized]
        private DateTime _dynamicPasswordCreateTime;

        /// <summary>
        /// 动态口令生成时间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime DynamicPasswordCreateTime
        {
            get { return _dynamicPasswordCreateTime; }
        }

        private string _requestAddress;

        /// <summary>
        /// 服务请求方IP地址
        /// </summary>
        public string RequestAddress
        {
            get { return _requestAddress; }
        }

        private int _requestFailureCount;

        /// <summary>
        /// 服务请求失败次数
        /// </summary>
        public int RequestFailureCount
        {
            get { return _requestFailureCount; }
        }

        private DateTime? _requestFailureTime;

        /// <summary>
        /// 服务请求失败时间
        /// </summary>
        public DateTime? RequestFailureTime
        {
            get { return _requestFailureTime; }
        }

        private long? _rootTeamsId;

        /// <summary>
        /// 所属顶层团体ID
        /// </summary>
        public long? RootTeamsId
        {
            get { return _rootTeamsId; }
        }

        [NonSerialized]
        private Teams _rootTeams;

        /// <summary>
        /// 顶层团体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Teams RootTeams
        {
            get
            {
                if (_rootTeams == null && _rootTeamsId.HasValue)
                    _rootTeams = Teams.FetchRoot(_rootTeamsId.Value);
                return _rootTeams;
            }
        }

        private long? _teamsId;

        /// <summary>
        /// 所属团体ID
        /// </summary>
        public long? TeamsId
        {
            get { return _teamsId; }
        }

        [NonSerialized]
        private Teams _teams;

        /// <summary>
        /// 所属团体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Teams Teams
        {
            get
            {
                if (_teams == null && _teamsId.HasValue)
                    _teams = RootTeams.FindSubTeams(_teamsId.Value);
                return _teams;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "不允许空挂所属团体!");

                if (UpdateProperty(p => p.Id,
                        SetProperty(p => p.RootTeamsId, value.RootId),
                        SetProperty(p => p.TeamsId, value.Id)) == 1)
                {
                    _rootTeams = value.Root;
                    _teams = value;
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }

        private long? _positionId;

        /// <summary>
        /// 担任岗位ID
        /// </summary>
        public long? PositionId
        {
            get { return _positionId; }
        }

        private Position _position;

        /// <summary>
        /// 担任岗位
        /// </summary>
        public Position Position
        {
            get
            {
                if (_position == null && _positionId.HasValue)
                    _position = Position.Fetch(_positionId.Value);
                return _position;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "不允许空挂担任岗位!");

                if (UpdateProperty(p => p.Id, SetProperty(p => p.PositionId, value.Id)) == 1)
                {
                    _position = value;
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }

        /// <summary>
        /// 是否企业组织架构管理员
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsOrganizationalArchitectureManager
        {
            get { return _rootTeamsId == _teamsId; }
        }

        private bool _locked;

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool Locked
        {
            get { return _locked; }
            set
            {
                if (UpdateProperty(p => p.Id,
                        SetProperty(p => p.Locked, value),
                        SetProperty(p => p.LockedTime, DateTime.Now)) == 1)
                {
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }

        private DateTime? _lockedTime;

        /// <summary>
        /// 锁定时间
        /// </summary>
        public DateTime? LockedTime
        {
            get { return _lockedTime; }
        }

        private bool _disabled;

        /// <summary>
        /// 是否注销
        /// </summary>
        public bool Disabled
        {
            get { return _disabled; }
            set
            {
                if (UpdateProperty(p => p.Id,
                        SetProperty(p => p.Disabled, value),
                        SetProperty(p => p.DisabledTime, DateTime.Now)) == 1)
                {
                    Task.Run(() => SaveRenovateLog(p => p.Id, ExecuteAction.Update));
                    _cache.Remove(Id);
                }
            }
        }

        private DateTime? _disabledTime;

        /// <summary>
        /// 注销时间
        /// </summary>
        public DateTime? DisabledTime
        {
            get { return _disabledTime; }
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
#if MySQL
                DbCommandHelper.ExecuteNonQuery(connection, @"
CREATE TABLE PH7_User (
  US_ID NUMERIC(15) NOT NULL,
  US_Name VARCHAR(100) NOT NULL,
  US_Phone VARCHAR(13) NULL,
  US_eMail VARCHAR(100) NULL,
  US_RegAlias VARCHAR(100) NULL,
  US_RegTime DATETIME NOT NULL,
  US_Password VARCHAR(500) NOT NULL,
  US_DynamicPassword VARCHAR(500) NOT NULL,
  US_DynamicPasswordCreateTime DATETIME NOT NULL,
  US_RequestAddress VARCHAR(39) NOT NULL,
  US_RequestFailureCount NUMERIC(2) default 0 NOT NULL,
  US_RequestFailureTime DATETIME NULL,
  US_Root_Teams_ID NUMERIC(15) NULL,
  US_Teams_ID NUMERIC(15) NULL,
  US_Position_ID NUMERIC(15) NULL,
  US_Locked NUMERIC(1) default 0 NOT NULL,
  US_LockedTime DATETIME NULL,
  US_Disabled NUMERIC(1) default 0 NOT NULL,
  US_DisabledTime DATETIME NULL,
  PRIMARY KEY(US_ID),
  UNIQUE(US_Name)
)", false);
#endif
#if ORA
                DbCommandHelper.ExecuteNonQuery(connection, @"
CREATE TABLE PH7_User (
  US_ID NUMERIC(15) NOT NULL,
  US_Name VARCHAR(100) NOT NULL,
  US_Phone VARCHAR(13) NULL,
  US_eMail VARCHAR(100) NULL,
  US_RegAlias VARCHAR(100) NULL,
  US_RegTime DATE NOT NULL,
  US_Password VARCHAR(500) NOT NULL,
  US_DynamicPassword VARCHAR(500) NOT NULL,
  US_DynamicPasswordCreateTime DATE NOT NULL,
  US_RequestAddress VARCHAR(39) NOT NULL,
  US_RequestFailureCount NUMERIC(2) default 0 NOT NULL,
  US_RequestFailureTime DATE NULL,
  US_Root_Teams_ID NUMERIC(15) NULL,
  US_Teams_ID NUMERIC(15) NULL,
  US_Position_ID NUMERIC(15) NULL,
  US_Locked NUMERIC(1) default 0 NOT NULL,
  US_LockedTime DATE NULL,
  US_Disabled NUMERIC(1) default 0 NOT NULL,
  US_DisabledTime DATE NULL,
  PRIMARY KEY(US_ID),
  UNIQUE(US_Name)
)", false);
#endif
            }
            catch (Exception ex)
            {
                Task.Run(() => EventLog.SaveLocal("Initialize PH7_User Table", ex));
                throw;
            }
        }

        #endregion
    }
}