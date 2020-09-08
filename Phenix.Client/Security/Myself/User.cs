using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;
using Phenix.Core.Net.Api;
using Phenix.Core.Reflection;
using Phenix.Core.Security.Auth;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.Threading;

namespace Phenix.Client.Security.Myself
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public class User : DataBase<User>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private User()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private User(string dataSourceKey, long id,
            string name, string phone, string eMail, string regAlias, DateTime regTime,
            string requestAddress, int requestFailureCount, DateTime? requestFailureTime,
            long? rootTeamsId, Teams rootTeams, long? teamsId, long? positionId, Position position,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime)
            : base(dataSourceKey, id)
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

        internal User(HttpClient httpClient, string name, string password)
        {
            _httpClient = httpClient;
            _name = name;
            _password = MD5CryptoTextProvider.ComputeHash(password);
        }

        #region 属性

        [NonSerialized]
        private HttpClient _httpClient;

        [NonSerialized]
        private bool? _isMyself;

        /// <summary>
        /// 是否登录用户
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsMyself
        {
            get
            {
                if (!_isMyself.HasValue)
                    _isMyself = _httpClient != null && String.CompareOrdinal(_httpClient.Identity.Name, Name) == 0;
                return _isMyself.Value;
            }
        }

        private readonly string _name;

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
                Patch(Set(p => p.Phone, value));
                _phone = value;
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
                Patch(Set(p => p.EMail, value));
                _eMail = value;
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
                Patch(Set(p => p.RegAlias, value));
                _regAlias = value;
            }
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

        [NonSerialized]
        private string _requestSession;

        /// <summary>
        /// 服务请求会话签名
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string RequestSession
        {
            get { return _requestSession; }
            private set { _requestSession = value; }
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
        /// 所属顶层团体
        /// </summary>
        public Teams RootTeams
        {
            get { return _rootTeams ?? (_rootTeams = Teams.Fetch(_httpClient)); }
        }

        private long? _teamsId;

        /// <summary>
        /// 所属团体ID
        /// </summary>
        public long? TeamsId
        {
            get { return _teamsId ?? _rootTeamsId; }
        }

        [NonSerialized]
        private Teams _teams;

        /// <summary>
        /// 所属团体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Teams Teams
        {
            get { return _teams ?? (_teams = _teamsId.HasValue ? RootTeams.FindInBranch(p => p.Id == _teamsId.Value) : RootTeams); }
            set
            {
                Patch(Set(p => p.TeamsId, value != null ? value.Id : (long?) null));
                _teamsId = value != null ? value.Id : (long?) null;
                _teams = value;
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
                if (_position == null)
                {
                    if (_positionId.HasValue)
                        _position = AsyncHelper.RunSync(() => _httpClient.CallAsync<Position>(HttpMethod.Get, ApiConfig.ApiSecurityPositionPath, false,
                            Position.Set(p => p.Id, _positionId.Value)));
                }

                return _position;
            }
            set
            {
                Patch(Set(p => p.PositionId, value != null ? value.Id : (long?) null));
                _positionId = value != null ? value.Id : (long?) null;
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
            set
            {
                Patch(Set(p => p.Locked, value).
                    Set(p => p.LockedTime, DateTime.Now));
                _locked = value;
                _lockedTime = DateTime.Now;
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
                Patch(Set(p => p.Disabled, value).
                    Set(p => p.DisabledTime, DateTime.Now));
                _disabled = value;
                _disabledTime = DateTime.Now;
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
        private string _password;

        /// <summary>
        /// 登录口令/动态口令(散列值)
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Password
        {
            get { return _password; }
        }

        /// <summary>
        /// 是否公司管理员?
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsCompanyAdmin
        {
            get { return TeamsId == RootTeamsId; }
        }

        [NonSerialized]
        private bool _isAuthenticated;

        /// <summary>
        /// 已身份验证?
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsAuthenticated
        {
            get { return _isAuthenticated && !_disabled && !_locked; }
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
            return RijndaelCryptoTextProvider.Encrypt(_password, data is string ds ? ds : Utilities.JsonSerialize(data));
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
                return RijndaelCryptoTextProvider.Decrypt(_password, cipherText);
            }
            catch (SystemException) //FormatException & CryptographicException
            {
                throw new PasswordException();
            }
        }

        /// <summary>
        /// 身份验证token: [登录名],[时间戳(9位长随机数+ISO格式当前时间)],[签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)],[会话签名]
        /// </summary>
        public string FormatComplexAuthorization(bool reset)
        {
            string timestamp = String.Format("{0}{1}", new Random().Next(100000000, 1000000000), DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            if (reset)
                RequestSession = Encrypt(timestamp);
            return String.Format("{0},{1},{2},{3}", Uri.EscapeDataString(_name), timestamp, Encrypt(timestamp), RequestSession);
        }

        internal async Task LogonAsync(string tag)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ApiConfig.ApiSecurityGatePath))
            {
                request.Content = new StringContent(Encrypt(tag ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")), Encoding.UTF8);
                using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                {
                    await response.ThrowIfFailedAsync();
                    _isAuthenticated = true;
                }
            }
        }

        internal async Task<User> ReFetchAsync()
        {
            User result = await _httpClient.CallAsync<User>(HttpMethod.Get, ApiConfig.ApiSecurityMyselfPath, true);
            if (result != null)
            {
                result._httpClient = _httpClient;
                result._password = _password;
                result._isAuthenticated = _isAuthenticated;
            }

            return result;
        }

        /// <summary>
        /// 修改登录口令
        /// </summary>
        /// <param name="password">登录口令</param>
        /// <param name="newPassword">新登录口令</param>
        public async Task ChangePasswordAsync(string password, string newPassword)
        {
            await _httpClient.CallAsync(HttpMethod.Put, ApiConfig.ApiSecurityMyselfPasswordPath,
                String.Format("{0}{1}{2}", password, Standards.RowSeparator, newPassword), true);
            _password = MD5CryptoTextProvider.ComputeHash(newPassword);
        }

        #region CompanyAdmin 操作功能

        /// <summary>
        /// 更新顶层团体
        /// </summary>
        /// <param name="teamsName">公司名</param>
        public async Task PatchRootTeamsAsync(string teamsName)
        {
            if (String.IsNullOrEmpty(teamsName))
                throw new ArgumentNullException(nameof(teamsName));

            _rootTeamsId = await _httpClient.CallAsync<long>(HttpMethod.Patch, ApiConfig.ApiSecurityMyselfRootTeamsPath,
                Set(p => p.Name, teamsName));
            _rootTeams = null;
            _teams = null;
        }

        /// <summary>
        /// 获取公司用户资料
        /// </summary>
        public async Task<IList<User>> FetchCompanyUsersAsync()
        {
            List<User> result = await _httpClient.CallAsync<List<User>>(HttpMethod.Get, ApiConfig.ApiSecurityMyselfCompanyUserPath, true);
            if (result != null)
                foreach (User item in result)
                    item._httpClient = _httpClient;
            return result;
        }

        /// <summary>
        /// 登记/注册公司用户
        /// </summary>
        /// <param name="name">登录名</param>
        /// <param name="phone">手机(注册用可为空)</param>
        /// <param name="eMail">邮箱(注册用可为空)</param>
        /// <param name="regAlias">注册昵称(注册用可为空)</param>
        /// <param name="teams">所属团体</param>
        /// <param name="position">担任岗位</param>
        /// <returns>返回信息</returns>
        public async Task<string> RegisterCompanyUserAsync(string name, string phone, string eMail, string regAlias, Teams teams, Position position)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (teams == null)
                throw new ArgumentNullException(nameof(teams));
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            return await _httpClient.CallAsync<string>(HttpMethod.Put, ApiConfig.ApiSecurityMyselfCompanyUserPath, false,
                Set(p => p.Name, name).
                    Set(p => p.Phone, phone).
                    Set(p => p.EMail, eMail).
                    Set(p => p.RegAlias, regAlias).
                    Set(p => p.TeamsId, teams.Id).
                    Set(p => p.PositionId, position.Id));
        }

        private void Patch(params NameValue[] propertyValues)
        {
            if (IsMyself)
                AsyncHelper.RunSync(() => _httpClient.CallAsync(HttpMethod.Patch, ApiConfig.ApiSecurityMyselfPath,
                    propertyValues, true));
            else
                AsyncHelper.RunSync(() => _httpClient.CallAsync(HttpMethod.Patch, ApiConfig.ApiSecurityMyselfCompanyUserPath,
                    propertyValues, true, Set(p => p.Name, Name)));
        }

        #endregion

        #endregion
    }
}