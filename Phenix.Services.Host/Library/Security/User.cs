using System;
using Newtonsoft.Json.Linq;
using Phenix.Business;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Reflection;
using Phenix.Core.Security;
using Phenix.Core.Security.Auth;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.SyncCollections;

namespace Phenix.Services.Host.Library.Security
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public class User : User<User>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected User()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected User(string dataSourceKey,
            long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            long rootTeamsId, long teamsId, long? positionId,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime)
            : base(dataSourceKey,
                id, name, phone, eMail, regAlias, regTime,
                rootTeamsId, teamsId, positionId,
                locked, lockedTime, disabled, disabledTime)
        {
        }
    }

    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public abstract class User<T> : EntityBase<T>, IUser
        where T : User<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected User()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected User(string dataSourceKey,
            long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            long rootTeamsId, long teamsId, long? positionId,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime)
            : base(dataSourceKey)
        {
            _id = id;
            _name = name;
            _phone = phone;
            _eMail = eMail;
            _regAlias = regAlias;
            _regTime = regTime;
            _rootTeamsId = rootTeamsId;
            _teamsId = teamsId;
            _positionId = positionId;
            _locked = locked;
            _lockedTime = lockedTime;
            _disabled = disabled;
            _disabledTime = disabledTime;
        }

        #region 属性

        private long _id;

        /// <summary>
        /// 主键
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
        }

        private string _eMail;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string EMail
        {
            get { return _eMail; }
        }

        private string _regAlias;

        /// <summary>
        /// 注册昵称
        /// </summary>
        public string RegAlias
        {
            get { return _regAlias; }
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
        private string _requestAddress;

        /// <summary>
        /// 服务请求方IP地址
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string RequestAddress
        {
            get { return _requestAddress; }
        }

        [NonSerialized]
        private string _requestSignature;

        /// <summary>
        /// 服务请求会话签名
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string RequestSignature
        {
            get { return _requestSignature; }
        }

        [NonSerialized]
        private int _requestFailureCount;

        /// <summary>
        /// 服务请求失败次数
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public int RequestFailureCount
        {
            get { return _requestFailureCount; }
        }

        [NonSerialized]
        private DateTime? _requestFailureTime;

        /// <summary>
        /// 服务请求失败时间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime? RequestFailureTime
        {
            get { return _requestFailureTime; }
        }

        private long _rootTeamsId;

        /// <summary>
        /// 所属公司ID
        /// </summary>
        public long RootTeamsId
        {
            get { return _rootTeamsId; }
        }

        private long _teamsId;

        /// <summary>
        /// 所属部门ID
        /// </summary>
        public long TeamsId
        {
            get { return _teamsId; }
        }

        private long? _positionId;

        /// <summary>
        /// 担任岗位ID
        /// </summary>
        public long? PositionId
        {
            get { return _positionId; }
        }

        private bool _locked;

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool Locked
        {
            get { return _locked; }
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
        /// 登录口令(散列值)
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Password
        {
            get { return _password; }
        }

        [NonSerialized]
        private string _dynamicPassword;

        /// <summary>
        /// 动态口令(散列值)
        /// 为空时用登录口令
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string DynamicPassword
        {
            get { return _dynamicPassword; }
        }

        [NonSerialized]
        private DateTime? _dynamicPasswordCreateTime;

        /// <summary>
        /// 动态口令生成时间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime? DynamicPasswordCreateTime
        {
            get { return _dynamicPasswordCreateTime; }
        }

        /// <summary>
        /// 是否初始口令?
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsInitialPassword
        {
            get { return String.CompareOrdinal(MD5CryptoTextProvider.ComputeHash(Name), Password) == 0; }
        }

        /// <summary>
        /// 是否公司管理员?
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsCompanyAdmin
        {
            get { return TeamsId == RootTeamsId; }
        }
        
        /// <summary>
        /// 已身份验证?
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsAuthenticated
        {
            get { return _requestSignature != null && !_disabled && !_locked; }
        }

        [NonSerialized]
        private readonly SynchronizedList<string> _timestamps = new SynchronizedList<string>();

        [NonSerialized]
        private DateTime _timestampClearedDateTime = DateTime.Now;

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
            try
            {
                return RijndaelCryptoTextProvider.Encrypt(RequestSignature, data is string ds ? ds : Utilities.JsonSerialize(data));
            }
            catch (SystemException) //FormatException & CryptographicException
            {
                throw new UserVerifyException();
            }
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
                return RijndaelCryptoTextProvider.Decrypt(RequestSignature, cipherText);
            }
            catch (SystemException) //FormatException & CryptographicException
            {
                throw new UserVerifyException();
            }
        }

        private void VerifyStatus()
        {
            if (Disabled)
                throw new UserNotFoundException();
            if (Locked)
                throw new UserLockedException(Int32.MaxValue);
            if (RequestFailureCount > Principal.RequestFailureCountMaximum &&
                RequestFailureTime.HasValue && RequestFailureTime.Value.AddMinutes(Principal.RequestFailureLockedMinutes) > DateTime.Now)
                throw new UserLockedException(Principal.RequestFailureLockedMinutes);
        }

        private void VerifyTimestamp(string timestamp, bool reset)
        {
            if (String.IsNullOrEmpty(timestamp))
                throw new UserVerifyException();

            try
            {
                DateTime time = Utilities.ChangeType<DateTime>(timestamp.Substring(9));
                if (reset)
                {
                    _timestamps.Clear();
                    _timestampClearedDateTime = DateTime.Now;
                }
                else
                {
                    if (_timestamps.Count >= 10000)
                        try
                        {
                            if (_timestamps.Count / DateTime.Now.Subtract(_timestampClearedDateTime).TotalMinutes >= Principal.BreakRequestIntensityPerMinute)
                                throw new TimestampException();
                        }
                        finally
                        {
                            for (int i = 0; i < _timestamps.Count - 5000; i++)
                                _timestamps.RemoveAt(0);
                            _timestampClearedDateTime = DateTime.Now;
                        }

                    if (DateTime.Now.Subtract(_timestampClearedDateTime).TotalMinutes > Principal.RequestIdleIntervalLimitMinutes)
                    {
                        string lastTimestamp = _timestamps.FindLast((item) => true);
                        if (String.IsNullOrEmpty(lastTimestamp) ||
                            time.Subtract(Utilities.ChangeType<DateTime>(lastTimestamp.Substring(9))).TotalMinutes > Principal.RequestIdleIntervalLimitMinutes)
                            throw new UserVerifyException();
                    }

                    if (!AppRun.Debugging && _timestamps.Contains(timestamp))
                        throw new TimestampException();
                }

                _timestamps.Add(timestamp);

                double clockOffset = Math.Abs(DateTime.Now.Subtract(time).TotalMinutes);
                if (clockOffset > Principal.RequestClockOffsetLimitMinutes)
                    throw new OvertimeRequestException(clockOffset);
            }
            catch (Exception ex)
            {
                throw new UserVerifyException(ex);
            }
        }

        private void VerifyRequestAddress(string requestAddress)
        {
            if (!Principal.AllowMultiAddressRequest && String.CompareOrdinal(RequestAddress, requestAddress) != 0)
                throw new MultiAddressRequestException();
        }
        
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="tag">捎带数据(默认是客户端时间也可以是修改的新密码)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        public virtual void Logon(string signature, ref string tag, string requestAddress)
        {
            VerifyStatus();

            try
            {
                string timestamp = null;
                if (DynamicPassword != null)
                    try
                    {
                        timestamp = RijndaelCryptoTextProvider.Decrypt(DynamicPassword, signature);
                        tag = RijndaelCryptoTextProvider.Decrypt(DynamicPassword, tag);
                        if (!DynamicPasswordCreateTime.HasValue || DynamicPasswordCreateTime <= DateTime.Now.AddMinutes(-Principal.DynamicPasswordValidityMinutes))
                            throw new UserVerifyException();
                    }
                    catch (SystemException) //FormatException & CryptographicException
                    {
                        timestamp = null;
                    }

                if (timestamp == null)
                    try
                    {
                        timestamp = RijndaelCryptoTextProvider.Decrypt(Password, signature);
                        tag = RijndaelCryptoTextProvider.Decrypt(Password, tag);
                    }
                    catch (SystemException) //FormatException & CryptographicException
                    {
                        throw new UserVerifyException();
                    }

                VerifyTimestamp(timestamp, true);

                string newPassword = null;
                try
                {
                    dynamic obj = JObject.Parse(tag);
                    newPassword = (string) obj.newPassword;
                    if (newPassword == null)
                        throw new ArgumentNullException();
                }
                catch
                {
                    if (IsInitialPassword)
                        throw new PasswordComplexityException(String.Format(AppSettings.GetValue("登录前需修改口令以符合复杂性要求(长度需大于等于{0}个字符且至少包含数字、大小写字母、特殊字符之{1}种!)"), Principal.PasswordLengthMinimum, Principal.PasswordComplexityMinimum));
                }

                if (newPassword != null)
                {
                    VerifyPasswordComplexity(Name, newPassword);
                    ChangePassword(newPassword);
                }

                UpdateSelf(Set(p => p.RequestSignature, signature).
                    Set(p => p.RequestAddress, requestAddress).
                    Set(p => p.DynamicPassword, null).
                    Set(p => p.RequestFailureCount, 0));
            }
            catch
            {
                UpdateSelf(Set(p => p.RequestSignature, null).
                    Set(p => p.RequestAddress, requestAddress).
                    Set(p => p.DynamicPassword, null).
                    Set(p => p.RequestFailureCount, p => p.RequestFailureCount + 1).
                    Set(p => p.RequestFailureTime, DateTime.Now));
                throw;
            }
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        public virtual void Verify(string signature, string requestAddress)
        {
            if (!IsAuthenticated)
                throw new UserVerifyException();

            VerifyTimestamp(Decrypt(signature), false);
            VerifyRequestAddress(requestAddress);
        }

        /// <summary>
        /// 登出
        /// </summary>
        public virtual void Logout()
        {
            UpdateSelf(Set(p => p.RequestSignature, null));
        }

        /// <summary>
        /// 重置登录口令
        /// </summary>
        public virtual void ResetPassword()
        {
            UpdateSelf(Set(p => p.Password, MD5CryptoTextProvider.ComputeHash(Name)));
        }

        /// <summary>
        /// 修改登录口令
        /// </summary>
        /// <param name="newPassword">新登录口令</param>
        public virtual void ChangePassword(string newPassword)
        {
            UpdateSelf(Set(p => p.Password, MD5CryptoTextProvider.ComputeHash(newPassword)));
        }

        /// <summary>
        /// 申请动态口令
        /// </summary>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <returns>动态口令(6位数字一般作为验证码用短信发送给到用户)</returns>
        public virtual string ApplyDynamicPassword(string requestAddress)
        {
            if (DynamicPasswordCreateTime.HasValue && DynamicPasswordCreateTime >= DateTime.Now.AddMinutes(-Principal.DynamicPasswordValidityMinutes))
                throw new UserLockedException(Principal.DynamicPasswordValidityMinutes);

            VerifyStatus();

            string result = BuildDynamicPassword();
            UpdateSelf(Set(p => p.RequestAddress, requestAddress).
                Set(p => p.DynamicPassword, MD5CryptoTextProvider.ComputeHash(result)).
                Set(p => p.DynamicPasswordCreateTime, DateTime.Now));
            return result;
        }

        /// <summary>
        /// 激活
        /// </summary>
        public virtual void Activate()
        {
            if (Disabled)
                UpdateSelf(Set(p => p.Disabled, false).
                    Set(p => p.DisabledTime, DateTime.Now));
        }

        /// <summary>
        /// 注销
        /// </summary>
        public virtual void Disable()
        {
            if (!Disabled)
                UpdateSelf(Set(p => p.Disabled, true).
                    Set(p => p.DisabledTime, DateTime.Now));
        }

        /// <summary>
        /// 登录口令是否规范
        /// </summary>
        /// <param name="name">登录名</param>
        /// <param name="password">登录口令</param>
        /// <returns>是否正确</returns>
        public static void VerifyPasswordComplexity(string name, string password)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (password == null ||
                password.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                name.Contains(password, StringComparison.OrdinalIgnoreCase))
                throw new PasswordComplexityException(AppSettings.GetValue("登录口令不能为空或与登录名互为一部分!"));

            if (password.Length >= Principal.PasswordLengthMinimum)
            {
                int numberCount = 0; //数字个数
                int uppercaseCount = 0; //大写字母个数
                int lowercaseCount = 0; //小写字母个数
                int specialCount = 0; //特殊字符个数
                for (int i = 0; i < password.Length; i++)
                {
                    if (password[i] >= 48 && password[i] <= 57)
                        numberCount = numberCount + 1;
                    else if (password[i] >= 65 && password[i] <= 90)
                        uppercaseCount = uppercaseCount + 1;
                    else if (password[i] >= 97 && password[i] <= 122)
                        lowercaseCount = lowercaseCount + 1;
                    else
                        specialCount = specialCount + 1;
                    int times = 0;
                    if (numberCount > 0)
                        times = times + 1;
                    if (uppercaseCount > 0)
                        times = times + 1;
                    if (lowercaseCount > 0)
                        times = times + 1;
                    if (specialCount > 0)
                        times = times + 1;
                    if (times >= Principal.PasswordComplexityMinimum)
                        return;
                }
            }

            throw new PasswordComplexityException(Principal.PasswordLengthMinimum, Principal.PasswordComplexityMinimum);
        }

        /// <summary>
        /// 构建登录口令
        /// </summary>
        /// <returns>登录口令</returns>
        public static string BuildPassword()
        {
            string result = String.Empty;
            Random random = new Random();
            for (int i = 0; i < Principal.PasswordLengthMinimum; i++)
            {
                char c;
                switch (i)
                {
                    case 0: //特殊字符
                        switch (random.Next(1, 5))
                        {
                            case 1:
                                c = (char) random.Next(33, 48);
                                break;
                            case 2:
                                c = (char) random.Next(58, 65);
                                break;
                            case 3:
                                c = (char) random.Next(91, 97);
                                break;
                            default:
                                c = (char) random.Next(123, 127);
                                break;
                        }

                        break;
                    case 1: //大写字母
                        c = (char) random.Next(65, 91);
                        break;
                    case 2: //小写字母
                        c = (char) random.Next(97, 123);
                        break;
                    default: //数字
                        c = (char) random.Next(48, 58);
                        break;
                }

                result = random.Next(2) == 0 ? result + c : c + result;
            }

            return result;
        }

        /// <summary>
        /// 构建动态口令
        /// </summary>
        /// <returns>动态口令</returns>
        public static string BuildDynamicPassword()
        {
            return new Random().Next(100000, 1000000).ToString();
        }
        
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="database">Database</param>
        /// <param name="name">登录名</param>
        /// <param name="phone">手机(注册时可空)</param>
        /// <param name="eMail">邮箱(注册时可空)</param>
        /// <param name="regAlias">注册昵称(注册时可空)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="rootTeamsId">所属公司ID</param>
        /// <param name="teamsId">所属部门ID</param>
        /// <param name="positionId">担任岗位ID</param>
        /// <param name="initialPassword">初始口令(空时自动生成)</param>
        public static T Register(Database database, string name, string phone, string eMail, string regAlias, string requestAddress,
            long rootTeamsId, long teamsId, long? positionId, ref string initialPassword)
        {
            initialPassword ??= BuildPassword();
            T result = New(database,
                Set(p => p.Name, name).
                    Set(p => p.Phone, phone).
                    Set(p => p.EMail, eMail).
                    Set(p => p.RegAlias, regAlias ?? name).
                    Set(p => p.RegTime, DateTime.Now).
                    Set(p => p.RequestAddress, requestAddress).
                    Set(p => p.Password, MD5CryptoTextProvider.ComputeHash(initialPassword)).
                    Set(p => p.DynamicPassword, MD5CryptoTextProvider.ComputeHash(BuildDynamicPassword())).
                    Set(p => p.DynamicPasswordCreateTime, DateTime.Now).
                    Set(p => p.RootTeamsId, rootTeamsId).
                    Set(p => p.TeamsId, teamsId).
                    Set(p => p.PositionId, positionId));
            result.InsertSelf();
            return result;
        }

        #endregion
    }
}