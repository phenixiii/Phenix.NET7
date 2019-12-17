using System;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Net;
using Phenix.Core.Reflection;
using Phenix.Core.Security;
using Phenix.Core.Security.Cryptography;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public class User : EntangledRootEntityBase<User, IUserGrain>, IUser
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private User()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private User(long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            string requestAddress, int requestFailureCount, DateTime? requestFailureTime,
            long? rootTeamsId, Teams rootTeams, long? teamsId, long? positionId, Position position,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime)
            : base(id)
        {
            _name = name;
            _phone = phone;
            _eMail = eMail;
            _regAlias = regAlias;
            _regTime = regTime;
            _requestAddress = requestAddress;
            _requestFailureCount = requestFailureCount;
            _requestFailureTime = requestFailureTime;
            _rootTeamsId = rootTeamsId;
            _rootTeams = rootTeams;
            _teamsId = teamsId;
            _positionId = positionId;
            _position = position;
            _locked = locked;
            _lockedTime = lockedTime;
            _disabled = disabled;
            _disabledTime = disabledTime;
        }

        private User(long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            string requestAddress,
            string password, string dynamicPassword, DateTime dynamicPasswordCreateTime)
            : this(id, name, phone, eMail, regAlias, regTime, 
                requestAddress, 0, null,
                null, null, null, null, null,
                false, null, false, null)
        {
            _password = password;
            _dynamicPassword = dynamicPassword;
            _dynamicPasswordCreateTime = dynamicPasswordCreateTime;
        }

        #region 工厂

        /// <summary>
        /// 新增用户资料
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="phone">手机</param>
        /// <param name="eMail">邮箱</param>
        /// <param name="regAlias">注册昵称</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="initialPassword">初始口令(一般通过邮箱发送给到用户)</param>
        /// <param name="dynamicPassword">动态口令(6位数字一般作为验证码用短信发送给到用户)</param>
        /// <returns>用户资料</returns>
        public static User New(string name, string phone, string eMail, string regAlias, string requestAddress, out string initialPassword, out string dynamicPassword)
        {
            initialPassword = Phenix.Core.Security.User.BuildPassword(name);
            dynamicPassword = new Random().Next(100000, 1000000).ToString();
            User result = new User(Database.Sequence.Value, name, phone, eMail, regAlias, DateTime.Now,
                requestAddress,
                MD5CryptoTextProvider.ComputeHash(initialPassword), MD5CryptoTextProvider.ComputeHash(dynamicPassword), DateTime.Now);
            result.InsertSelf();
            return result;
        }

        /// <summary>
        /// 新增用户资料
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="password">登录口令(一般通过邮箱发送给到用户)</param>
        /// <returns>用户资料</returns>
        public static User New(string name, string password)
        {
            string dynamicPassword = new Random().Next(100000, 1000000).ToString();
            User result = new User(Database.Sequence.Value, name, null, null, null, DateTime.Now,
                NetConfig.LocalAddress,
                MD5CryptoTextProvider.ComputeHash(password), MD5CryptoTextProvider.ComputeHash(dynamicPassword), DateTime.Now);
            result.InsertSelf();
            return result;
        }

        #endregion

        #region 属性

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
            set { UpdateSelf(SetProperty(p => p.Phone, value)); }
        }

        private string _eMail;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string EMail
        {
            get { return _eMail; }
            set { UpdateSelf(SetProperty(p => p.EMail, value)); }
        }

        private string _regAlias;

        /// <summary>
        /// 注册昵称
        /// </summary>
        public string RegAlias
        {
            get { return _regAlias; }
            set { UpdateSelf(SetProperty(p => p.RegAlias, value)); }
        }

        private DateTime _regTime;

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime RegTime
        {
            get { return _regTime; }
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

        private Teams _rootTeams;

        /// <summary>
        /// 顶层团体
        /// </summary>
        public Teams RootTeams
        {
            get
            {
                if (_rootTeams == null && _rootTeamsId.HasValue)
                    _rootTeams = Teams.Fetch(_rootTeamsId.Value);
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
                    _teams = RootTeams.FindInBranch(_teamsId.Value);
                return _teams;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "不允许空挂所属团体");

                if (UpdateSelf(SetProperty(p => p.RootTeamsId, value.RootId),
                        SetProperty(p => p.TeamsId, value.Id)).Result == 1)
                {
                    _rootTeams = value.Root;
                    _teams = value;
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
                    throw new ArgumentNullException(nameof(value), "不允许空挂担任岗位");

                if (UpdateSelf(SetProperty(p => p.PositionId, value.Id)).Result == 1)
                    _position = value;
            }
        }

        private bool _locked;

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool Locked
        {
            get { return _locked; }
            set { UpdateSelf(SetProperty(p => p.Locked, value), SetProperty(p => p.LockedTime, DateTime.Now)); }
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
            set { UpdateSelf(SetProperty(p => p.Disabled, value), SetProperty(p => p.DisabledTime, DateTime.Now)); }
        }

        private DateTime? _disabledTime;

        /// <summary>
        /// 注销时间
        /// </summary>
        public DateTime? DisabledTime
        {
            get { return _disabledTime; }
        }

        /// <summary>
        /// 登录口令(散列值)
        /// </summary>
        [NonSerialized]
        private string _password;

        /// <summary>
        /// 登录口令(散列值)
        /// </summary>
        protected string Password
        {
            get { return _password; }
        }

        /// <summary>
        /// 动态口令(散列值)
        /// 为空时用登录口令
        /// </summary>
        [NonSerialized]
        private string _dynamicPassword;

        /// <summary>
        /// 动态口令(散列值)
        /// 为空时用登录口令
        /// </summary>
        protected string DynamicPassword
        {
            get { return _dynamicPassword; }
        }

        /// <summary>
        /// 动态口令生成时间
        /// </summary>
        [NonSerialized]
        private DateTime _dynamicPasswordCreateTime;

        /// <summary>
        /// 动态口令生成时间
        /// </summary>
        protected DateTime DynamicPasswordCreateTime
        {
            get { return _dynamicPasswordCreateTime; }
        }

        /// <summary>
        /// 是否企业组织架构管理员
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsOrganizationalArchitectureManager
        {
            get { return _rootTeamsId == _teamsId; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 加密
        /// Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="data">需加密的对象/字符串</param>
        public Task<string> Encrypt(object data)
        {
            return Grain.Encrypt(data is string ds ? ds : Utilities.JsonSerialize(data));
        }

        /// <summary>
        /// 解密
        ///  Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="cipherText">密文(Base64字符串)</param>
        public Task<string> Decrypt(string cipherText)
        {
            return Grain.Decrypt(cipherText);
        }

        /// <summary>
        /// 核对登录有效性
        /// </summary>
        /// <param name="timestamp">时间戳(9位长随机数+ISO格式当前时间)</param>
        /// <param name="signature">签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令错误或口令失效时会抛出UserNotFoundException/UserLockedException/UserVerifyException异常而不是返回false</param>
        /// <returns>是否正确</returns>
        public Task<bool> IsValidLogon(string timestamp, string signature, string requestAddress, bool throwIfNotConform = true)
        {
            return Grain.IsValidLogon(timestamp, signature, requestAddress, throwIfNotConform);
        }

        /// <summary>
        /// 核对有效性
        /// </summary>
        /// <param name="timestamp">时间戳(9位长随机数+ISO格式当前时间)</param>
        /// <param name="signature">签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令错误或禁止多终端登录时会抛出UserNotFoundException/UserLockedException/UserVerifyException/UserMultiAddressRequestException异常而不是返回false</param>
        /// <returns>是否正确</returns>
        public Task<bool> IsValid(string timestamp, string signature, string requestAddress, bool throwIfNotConform = true)
        {
            return Grain.IsValid(timestamp, signature, requestAddress, throwIfNotConform);
        }

        /// <summary>
        /// 修改登录口令
        /// </summary>
        /// <param name="newPassword">新登录口令</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令不规范会抛出UserNotFoundException/UserLockedException/UserVerifyException/UserPasswordComplexityException异常而不是返回false</param>
        /// <returns>是否成功</returns>
        public Task<bool> ChangePassword(string newPassword, bool throwIfNotConform = true)
        {
            return Grain.ChangePassword(newPassword, throwIfNotConform);
        }

        /// <summary>
        /// 申请动态口令
        /// </summary>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 核对有效性不符会抛出UserNotFoundException/UserLockedException异常而不是返回false</param>
        /// <returns>动态口令(6位数字一般作为验证码用短信发送给到用户)</returns>
        public Task<string> ApplyDynamicPassword(string requestAddress, bool throwIfNotConform = true)
        {
            return Grain.ApplyDynamicPassword(requestAddress, throwIfNotConform);
        }

        #endregion
    }
}