using System;
using Phenix.Core.Data;
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
    public abstract class User<T> : Phenix.Business.Security.User<T>
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
            : base(dataSourceKey,
                id, name, phone, eMail, regAlias, regTime,
                rootTeamsId, teamsId, positionId,
                locked, lockedTime, disabled, disabledTime)
        {
        }

        #region 方法

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="tag">捎带数据(默认是客户端时间也可以是修改的新密码)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        public override void Logon(string signature, ref string tag, string requestAddress)
        {
            try
            {
                base.Logon(signature, ref tag, requestAddress);
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
        /// 登出
        /// </summary>
        public override void Logout()
        {
            UpdateSelf(Set(p => p.RequestSignature, null));
        }

        /// <summary>
        /// 重置登录口令
        /// </summary>
        public override void ResetPassword()
        {
            UpdateSelf(Set(p => p.Password, MD5CryptoTextProvider.ComputeHash(Name)));
        }

        /// <summary>
        /// 修改登录口令
        /// </summary>
        /// <param name="newPassword">新登录口令</param>
        public override void ChangePassword(string newPassword)
        {
            UpdateSelf(Set(p => p.Password, MD5CryptoTextProvider.ComputeHash(newPassword)));
        }

        /// <summary>
        /// 申请动态口令
        /// </summary>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <returns>动态口令(6位数字一般作为验证码用短信发送给到用户)</returns>
        public override string ApplyDynamicPassword(string requestAddress)
        {
            string result = base.ApplyDynamicPassword(requestAddress);
            UpdateSelf(Set(p => p.RequestAddress, requestAddress).
                Set(p => p.DynamicPassword, MD5CryptoTextProvider.ComputeHash(result)).
                Set(p => p.DynamicPasswordCreateTime, DateTime.Now));
            return result;
        }

        /// <summary>
        /// 激活
        /// </summary>
        public override void Activate()
        {
            if (Disabled)
                UpdateSelf(Set(p => p.Disabled, false).
                    Set(p => p.DisabledTime, DateTime.Now));
        }

        /// <summary>
        /// 注销
        /// </summary>
        public override void Disable()
        {
            if (!Disabled)
                UpdateSelf(Set(p => p.Disabled, true).
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