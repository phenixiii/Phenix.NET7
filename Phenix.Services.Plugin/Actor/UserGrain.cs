using System;
using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Data.Schema;
using Phenix.Core.Security;
using Phenix.Core.Security.Auth;

namespace Phenix.Services.Plugin.Actor
{
    /// <summary>
    /// 用户资料Grain
    /// </summary>
    public class UserGrain : EntityGrainBase<User>, IUserGrain
    {
        #region 属性

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name ?? (_name = this.GetPrimaryKeyString()); }
        }

        /// <summary>
        /// ID(映射表XX_ID字段)
        /// </summary>
        protected override long Id
        {
            get { return Kernel.Id; }
        }

        private User _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override User Kernel
        {
            get { return _kernel ?? (_kernel = User.FetchRoot(Database.Default, p => p.Name == Name)); }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        Task<string> IUserGrain.CheckIn(string name, string phone, string eMail, string regAlias, string requestAddress)
        {
            User user = Kernel;
            if (user != null)
            {
                string dynamicPassword = user.ApplyDynamicPassword(requestAddress, true);
                /*
                 * 以下代码供你自己测试用
                 * 生产环境下，请替换为通过第三方渠道（邮箱或短信）推送给到用户
                 */
                Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0} 的动态口令是'{1}'(有效期 {2} 分钟)", user.Name, dynamicPassword, User.DynamicPasswordValidityMinutes));
            }
            else
            {
                string initialPassword = User.BuildPassword(name);
                string dynamicPassword = User.BuildDynamicPassword();
                user = User.New(Database.Default,
                    NameValue.Set<User>(p => p.Name, name),
                    NameValue.Set<User>(p => p.Phone, phone),
                    NameValue.Set<User>(p => p.EMail, eMail),
                    NameValue.Set<User>(p => p.RegAlias, regAlias),
                    NameValue.Set<User>(p => p.RequestAddress, requestAddress),
                    NameValue.Set<User>(p => p.Password, initialPassword),
                    NameValue.Set<User>(p => p.DynamicPassword, dynamicPassword));
                user.InsertSelf();
                /*
                 * 以下代码供你自己测试用
                 * 生产环境下，请替换为通过第三方渠道（邮箱或短信）推送给到用户
                 */
                Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0} 的初始口令是'{1}'，动态口令是'{2}'(有效期 {3} 分钟)", user.Name, initialPassword, dynamicPassword, User.DynamicPasswordValidityMinutes));
            }

            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为提示用户留意查看邮箱或短信以收取动态口令
             */
            return Task.FromResult(String.Format("{0} 的动态口令存放于 {1} 目录下的日志文件里", user.Name, Phenix.Core.Log.EventLog.LocalDirectory));
        }

        Task IUserGrain.Logon(string tag)
        {
            Kernel = null;
            /*
             * 本函数被执行到，说明当前用户 user 已经登录成功
             * 可利用客户端传过来的 tag 扩展出系统自己的用户登录功能
             */
            return Task.CompletedTask;
        }

        Task<bool> IUserGrain.IsInRole(string role)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.IsInRole(role));
        }

        Task<string> IUserGrain.Encrypt(string sourceText)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.Encrypt(sourceText));
        }

        Task<string> IUserGrain.Decrypt(string cipherText)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.Decrypt(cipherText));
        }

        Task<bool> IUserGrain.IsValidLogon(string timestamp, string signature, string requestAddress, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.IsValidLogon(timestamp, signature, requestAddress, throwIfNotConform));
        }

        Task<bool> IUserGrain.IsValid(string timestamp, string signature, string requestAddress, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.IsValid(timestamp, signature, requestAddress, throwIfNotConform));
        }

        Task<bool> IUserGrain.ChangePassword(string newPassword, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.ChangePassword(newPassword, throwIfNotConform));
        }

        #endregion
    }
}