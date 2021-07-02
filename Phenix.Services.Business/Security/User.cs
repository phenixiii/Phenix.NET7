using System;
using System.Collections.Generic;
using Phenix.Core.Data;
using Phenix.Core.Security.Auth;
using Phenix.Core.Security.Cryptography;

namespace Phenix.Services.Business.Security
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
        private User()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private User(string dataSourceKey,
            long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            string requestAddress, int requestFailureCount, DateTime? requestFailureTime,
            long rootTeamsId, long teamsId, long? positionId,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime,
            string teamsName, string positionName)
            : base(dataSourceKey,
                id, name, phone, eMail, regAlias, regTime,
                requestAddress, requestFailureCount, requestFailureTime,
                rootTeamsId, teamsId, positionId,
                locked, lockedTime, disabled, disabledTime)
        {
            _teamsName = teamsName;
            _positionName = positionName;
        }

        #region 属性

        private string _teamsName;

        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string TeamsName
        {
            get { return _teamsName; }
        }

        private string _positionName;

        /// <summary>
        /// 担任岗位名称
        /// </summary>
        public string PositionName
        {
            get { return _positionName; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取自己用户资料
        /// </summary>
        public User FetchMyself(Teams teams, Position position)
        {
            _teamsName = teams != null ? teams.Name : null;
            _positionName = position != null ? position.Name : null;
            return this;
        }

        /// <summary>
        /// 获取公司用户资料
        /// </summary>
        public IList<User> FetchCompanyUsers(Teams company, IDictionary<long, Position> positions)
        {
            IList<User> result = FetchList(Database, p => p.RootTeamsId == RootTeamsId && p.RootTeamsId != p.TeamsId);
            foreach (User item in result)
            {
                Teams teams = company.FindInBranch(p => p.Id == item.TeamsId);
                if (teams != null)
                    item._teamsName = teams.Name;
                if (item.PositionId.HasValue && positions.TryGetValue(item.PositionId.Value, out Position position))
                    item._positionName = position.Name;
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public abstract class User<T> : Phenix.Core.Security.User<T>
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
        /// 初始化
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected User(string dataSourceKey,
            long id, string name, string phone, string eMail, string regAlias, DateTime regTime,
            string requestAddress, int requestFailureCount, DateTime? requestFailureTime,
            long rootTeamsId, long teamsId, long? positionId,
            bool locked, DateTime? lockedTime, bool disabled, DateTime? disabledTime)
            : base(dataSourceKey,
                id, name, phone, eMail, regAlias, regTime,
                requestAddress, requestFailureCount, requestFailureTime,
                rootTeamsId, teamsId, positionId,
                locked, lockedTime, disabled, disabledTime)
        {
        }

        #region 方法

        /// <summary>
        /// 核对动态口令有效性
        /// </summary>
        /// <param name="timestamp">时间戳(9位长随机数+ISO格式当前时间)</param>
        /// <param name="dynamicPassword">动态口令</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="requestSession">服务请求会话签名</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令错误或口令失效时会抛出UserNotFoundException/UserLockedException/UserVerifyException异常而不是返回false</param>
        /// <returns>是否正确</returns>
        protected override bool IsValidDynamicPassword(string timestamp, string dynamicPassword, string requestAddress, string requestSession, bool throwIfNotConform = true)
        {
            if (UpdateSelf(p => p.Id == Id && p.DynamicPassword == dynamicPassword &&
                                p.DynamicPasswordCreateTime >= DateTime.Now.AddMinutes(-DynamicPasswordValidityMinutes),
                    SetProperty(p => p.RequestAddress, requestAddress).
                        Set(p => p.RequestSession, requestSession).
                        Set(p => p.RequestFailureCount, 0).
                        Set(p => p.RequestFailureTime, null)) == 1)
            {
                DynamicPassword = dynamicPassword;
                SetAuthenticated(timestamp, true);
                return true;
            }
            UpdateSelf(SetProperty(p => p.RequestAddress, requestAddress).
                Set(p => p.RequestSession, requestSession).
                Set(p => p.RequestFailureCount, p => p.RequestFailureCount + 1).
                Set(p => p.RequestFailureTime, DateTime.Now));
            if (throwIfNotConform)
                throw new UserVerifyException();
            return false;
        }

        /// <summary>
        /// 核对登录口令有效性
        /// </summary>
        /// <param name="timestamp">时间戳(9位长随机数+ISO格式当前时间)</param>
        /// <param name="password">登录口令</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="requestSession">服务请求会话签名</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令错误或口令失效时会抛出UserNotFoundException/UserLockedException/UserVerifyException异常而不是返回false</param>
        /// <returns>是否正确</returns>
        protected override bool IsValidPassword(string timestamp, string password, string requestAddress, string requestSession, bool throwIfNotConform = true)
        {
            if (UpdateSelf(p => p.Id == Id && p.Password == password,
                    SetProperty(p => p.RequestAddress, requestAddress).
                        Set(p => p.RequestSession, requestSession).
                        Set(p => p.RequestFailureCount, 0).
                        Set(p => p.RequestFailureTime, null)) == 1)
            {
                Password = password;
                DynamicPassword = null;
                SetAuthenticated(timestamp, true);
                return true;
            }

            UpdateSelf(SetProperty(p => p.RequestAddress, requestAddress).
                Set(p => p.RequestSession, requestSession).
                Set(p => p.RequestFailureCount, p => p.RequestFailureCount + 1).
                Set(p => p.RequestFailureTime, DateTime.Now));
            if (throwIfNotConform)
                throw new UserVerifyException();
            return false;
        }

        /// <summary>
        /// 重置登录口令
        /// </summary>
        /// <returns>是否成功</returns>
        public override bool ResetPassword()
        {
            return UpdateSelf(SetProperty(p => p.Password, MD5CryptoTextProvider.ComputeHash(Name))) == 1;
        }

        /// <summary>
        /// 修改登录口令
        /// </summary>
        /// <param name="password">登录口令</param>
        /// <param name="newPassword">新登录口令</param>
        /// <param name="hashPassword">需HASH登录口令</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="requestSession">服务请求会话签名</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令不规范会抛出UserNotFoundException/UserLockedException/UserVerifyException/UserPasswordComplexityException异常而不是返回false</param>
        /// <returns>是否成功</returns>
        public override bool ChangePassword(ref string password, ref string newPassword, bool hashPassword, string requestAddress, string requestSession, bool throwIfNotConform = true)
        {
            if (!base.ChangePassword(ref password, ref newPassword, hashPassword, requestAddress, requestSession, throwIfNotConform))
                return false;
            return UpdateSelf(SetProperty(p => p.Password, newPassword)) == 1;
        }

        /// <summary>
        /// 申请动态口令
        /// </summary>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 核对有效性不符会抛出UserNotFoundException/UserLockedException异常而不是返回false</param>
        /// <returns>动态口令(6位数字一般作为验证码用短信发送给到用户)</returns>
        public override string ApplyDynamicPassword(string requestAddress, bool throwIfNotConform = true)
        {
            string result = base.ApplyDynamicPassword(requestAddress, throwIfNotConform);
            if (result != null)
                UpdateSelf(SetProperty(p => p.DynamicPassword, MD5CryptoTextProvider.ComputeHash(result)).
                    Set(p => p.DynamicPasswordCreateTime, DateTime.Now).
                    Set(p => p.RequestAddress, requestAddress));
            return result ;
        }

        /// <summary>
        /// 激活
        /// </summary>
        public override void Activate()
        {
            if (Disabled)
                UpdateSelf(SetProperty(p => p.Disabled, false).
                    Set(p => p.DisabledTime, DateTime.Now));
        }

        /// <summary>
        /// 注销
        /// </summary>
        public override void Disable()
        {
            if (!Disabled)
                UpdateSelf(SetProperty(p => p.Disabled, true).
                    Set(p => p.DisabledTime, DateTime.Now));
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