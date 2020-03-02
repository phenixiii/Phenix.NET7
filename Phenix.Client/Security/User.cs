using System;
using System.Security.Cryptography;
using Phenix.Core.Reflection;
using Phenix.Core.Security.Auth;
using Phenix.Core.Security.Cryptography;

namespace Phenix.Client.Security
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public class User
    {
        [Newtonsoft.Json.JsonConstructor]
        private User(long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            string requestAddress, int requestFailureCount, DateTime? requestFailureTime,
            long? rootTeamsId, Teams rootTeams, long? teamsId, long? positionId, Position position,
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
            _rootTeams = rootTeams;
            _teamsId = teamsId;
            _positionId = positionId;
            _position = position;
            _locked = locked;
            _lockedTime = lockedTime;
            _disabled = disabled;
            _disabledTime = disabledTime;
        }

        internal User(string name, string password)
        {
            _name = name;
            _password = password;
        }

        #region 属性

        private readonly long _id;

        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private readonly string _name;

        /// <summary>
        /// 登录名
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        private readonly string _phone;

        /// <summary>
        /// 手机
        /// </summary>
        public string Phone
        {
            get { return _phone; }
        }

        private readonly string _eMail;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string EMail
        {
            get { return _eMail; }
        }

        private readonly string _regAlias;

        /// <summary>
        /// 注册昵称
        /// </summary>
        public string RegAlias
        {
            get { return _regAlias; }
        }

        private readonly DateTime _regTime;

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime RegTime
        {
            get { return _regTime; }
        }

        private readonly string _requestAddress;

        /// <summary>
        /// 服务请求方IP地址
        /// </summary>
        public string RequestAddress
        {
            get { return _requestAddress; }
        }

        private readonly int _requestFailureCount;

        /// <summary>
        /// 服务请求失败次数
        /// </summary>
        public int RequestFailureCount
        {
            get { return _requestFailureCount; }
        }

        private readonly DateTime? _requestFailureTime;

        /// <summary>
        /// 服务请求失败时间
        /// </summary>
        public DateTime? RequestFailureTime
        {
            get { return _requestFailureTime; }
        }

        private readonly long? _rootTeamsId;

        /// <summary>
        /// 所属顶层团体ID
        /// </summary>
        public long? RootTeamsId
        {
            get { return _rootTeamsId; }
        }

        private readonly Teams _rootTeams;

        /// <summary>
        /// 顶层团体
        /// </summary>
        public Teams RootTeams
        {
            get { return _rootTeams; }
        }

        private readonly long? _teamsId;

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
        }

        private readonly long? _positionId;

        /// <summary>
        /// 担任岗位ID
        /// </summary>
        public long? PositionId
        {
            get { return _positionId; }
        }

        private readonly Position _position;

        /// <summary>
        /// 担任岗位
        /// </summary>
        public Position Position
        {
            get { return _position; }
        }

        private readonly bool _locked;

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool Locked
        {
            get { return _locked; }
        }

        private readonly DateTime? _lockedTime;

        /// <summary>
        /// 锁定时间
        /// </summary>
        public DateTime? LockedTime
        {
            get { return _lockedTime; }
        }

        private readonly bool _disabled;

        /// <summary>
        /// 是否注销
        /// </summary>
        public bool Disabled
        {
            get { return _disabled; }
        }

        private readonly DateTime? _disabledTime;

        /// <summary>
        /// 注销时间
        /// </summary>
        public DateTime? DisabledTime
        {
            get { return _disabledTime; }
        }

        [NonSerialized]
        private string _password;

        /// <summary>
        /// 登录口令/动态口令(散列值)
        /// </summary>
        protected internal string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 加密
        /// Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="data">需加密的对象/字符串</param>
        /// <returns>密文(Base64字符串)</returns>
        public string Encrypt(object data)
        {
            return RijndaelCryptoTextProvider.Encrypt(Password, data is string ds ? ds : Utilities.JsonSerialize(data));
        }

        /// <summary>
        /// 解密
        ///  Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="cipherText">密文(Base64字符串)</param>
        /// <returns>原文</returns>
        public string Decrypt(string cipherText)
        {
            try
            {
                return RijndaelCryptoTextProvider.Decrypt(Password, cipherText);
            }
            catch (CryptographicException)
            {
                throw new PasswordException();
            }
        }

        /// <summary>
        /// 身份验证token: [登录名],[时间戳(9位长随机数+ISO格式当前时间)],[签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)]
        /// </summary>
        public string FormatComplexAuthorization()
        {
            string timestamp = String.Format("{0}{1}", new Random().Next(100000000, 1000000000), DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            return String.Format("{0},{1},{2}", Uri.EscapeDataString(Name), timestamp, Encrypt(timestamp));
        }
        
        #endregion
    }
}