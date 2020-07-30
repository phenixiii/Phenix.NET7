using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;
using Phenix.Core;
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

        #region 配置项

        private static long? _keyPairDiscardIntervalSeconds;

        /// <summary>
        /// 公钥私钥对丢弃间隔(秒)
        /// 默认：60
        /// </summary>
        public static long KeyPairDiscardIntervalSeconds
        {
            get { return AppSettings.GetProperty(ref _keyPairDiscardIntervalSeconds, 60); }
            set { AppSettings.SetProperty(ref _keyPairDiscardIntervalSeconds, value); }
        }

        #endregion

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name ?? (_name = this.GetPrimaryKeyString()); }
        }

        /// <summary>
        /// ID(映射表ID字段)
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
            get { return _kernel ?? (_kernel = User.FetchRoot(Database, p => p.Name == Name)); }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        private string Register(string phone, string eMail, string regAlias, string requestAddress, 
            string initialPassword = null, string dynamicPassword = null, bool hashPassword = true,
            long? rootTeamsId = null, long? teamsId = null, long? positionId = null)
        {
            Kernel = User.Register(Database, Name, phone, eMail, regAlias, requestAddress, rootTeamsId, teamsId, positionId, ref initialPassword, ref dynamicPassword, hashPassword);
            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为通过第三方渠道（邮箱或短信）推送给到用户
             */
            Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0}({1}) 的初始口令是'{2}'，动态口令是'{3}'(有效期 {4} 分钟)", Kernel.RegAlias, Kernel.Name, initialPassword, dynamicPassword, User.DynamicPasswordValidityMinutes));
            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为提示用户留意查看邮箱或短信以收取动态口令
             */
            return String.Format("{0}({1}) 的动态口令存放于 {2} 目录下的日志文件里", Kernel.RegAlias, Kernel.Name, Phenix.Core.Log.EventLog.LocalDirectory);
        }

        Task<string> IUserGrain.CheckIn(string phone, string eMail, string regAlias, string requestAddress)
        {
            if (Kernel != null)
            {
                string dynamicPassword = Kernel.ApplyDynamicPassword(requestAddress, true);
                /*
                 * 以下代码供你自己测试用
                 * 生产环境下，请替换为通过第三方渠道（邮箱或短信）推送给到用户
                 */
                Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0}({1}) 的动态口令是'{2}'(有效期 {2} 分钟)", Kernel.RegAlias, Kernel.Name, dynamicPassword, User.DynamicPasswordValidityMinutes));
                /*
                 * 以下代码供你自己测试用
                 * 生产环境下，请替换为提示用户留意查看邮箱或短信以收取动态口令
                 */
                return Task.FromResult(String.Format("{0}({1}) 的动态口令存放于 {2} 目录下的日志文件里", Kernel.RegAlias, Kernel.Name, Phenix.Core.Log.EventLog.LocalDirectory));
            }

            return Task.FromResult(Register(phone, eMail, regAlias, requestAddress));
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

        async Task<bool> IUserGrain.IsValidLogon(string timestamp, string signature, string tag, string requestAddress, bool throwIfNotConform)
        {
            Phenix.Services.Plugin.P6C.HttpClient httpClient = Phenix.Services.Plugin.P6C.HttpClient.Default;
            if (httpClient != null)
            {
                string password = await ClusterClient.Default.GetGrain<IOneOffKeyPairGrain>(KeyPairDiscardIntervalSeconds).Decrypt(Name, await httpClient.LogonAsync(Name, timestamp, signature, tag), true);
                if (String.IsNullOrEmpty(password))
                {
                    if (Kernel != null)
                        Kernel.Disable();
                    throw new UserNotFoundException();
                }

                if (Kernel != null)
                {
                    Kernel.Activate();
                    Kernel.ChangePassword(Kernel.Password, password, false);
                }
                else
                    Register(null, null, Name, requestAddress, password, null, false);

                return true;
            }

            if (Kernel == null)
                if (User.IsReservedUserName(Name))
                {
                    Register(null, null, Name, requestAddress);
                    return true;
                }
                else
                    throw new UserNotFoundException();

            bool result = Kernel.IsValidLogon(timestamp, signature, requestAddress, throwIfNotConform);
            if (result)
            {
                /*
                 * 可利用客户端传过来的 tag 扩展出系统自己的用户登录功能
                 */
                tag = Kernel != null ? Kernel.Decrypt(tag) : null;
            }

            return result;
        }

        Task<bool> IUserGrain.IsValid(string timestamp, string signature, string requestAddress, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.IsValid(timestamp, signature, requestAddress, throwIfNotConform));
        }

        Task<bool> IUserGrain.ChangePassword(string password, string newPassword, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.ChangePassword(password, newPassword, true, throwIfNotConform));
        }

        #region CompanyAdmin 操作功能

        private void CheckCompanyAdmin()
        {
            if (Kernel == null)
                throw new UserNotFoundException();
            if (!Kernel.IsCompanyAdmin)
                throw new SecurityException("仅可供公司管理员操作!");
        }

        private void CheckCompanyTeams()
        {
            if (Kernel == null)
                throw new UserNotFoundException();
            if (!Kernel.IsCompanyAdmin)
                throw new SecurityException("仅可供公司管理员操作!");
            if (Kernel.RootTeamsId == null)
                throw new SecurityException("需事先搭建自己公司的组织架构!");
        }

        private void CheckCompanyTeams(long rootTeamsId)
        {
            if (Kernel == null)
                throw new UserNotFoundException();
            if (Kernel.RootTeamsId != rootTeamsId)
                throw new SecurityException("不允许管理其他公司的用户资料!");
        }

        Task<long> IUserGrain.PatchRootTeams(string name)
        {
            CheckCompanyAdmin();

            long result = Kernel.RootTeamsId.HasValue ? Kernel.RootTeamsId.Value : Database.Sequence.Value;
            ClusterClient.Default.GetGrain<ITeamsGrain>(result).PatchKernel(NameValue.Set<Teams>(p => p.Name, name));
            if (!Kernel.RootTeamsId.HasValue)
                Kernel.UpdateSelf(Kernel.SetProperty(p => p.RootTeamsId, result));
            return Task.FromResult(result);
        }

        Task<IList<User>> IUserGrain.FetchCompanyUsers()
        {
            CheckCompanyTeams();

            return Task.FromResult(User.FetchAll(Database, p => p.RootTeamsId == Kernel.RootTeamsId.Value && p.RootTeamsId != p.TeamsId));
        }

        Task<string> IUserGrain.RegisterCompanyUser(string name, string phone, string eMail, string regAlias, string requestAddress, long teamsId, long positionId)
        {
            CheckCompanyTeams();

            return ClusterClient.Default.GetGrain<IUserGrain>(name).Register(phone, eMail, regAlias, requestAddress, Kernel.RootTeamsId.Value, teamsId, positionId);
        }

        async Task<string> IUserGrain.Register(string phone, string eMail, string regAlias, string requestAddress, long rootTeamsId, long teamsId, long positionId)
        {
            if (Kernel != null)
                throw new SecurityException("登录名已被他人注册!");

            if (await ClusterClient.Default.GetGrain<ITeamsGrain>(rootTeamsId).HaveNode(teamsId, true))
                return Register(phone, eMail, regAlias, requestAddress);
            return null;
        }

        Task<int> IUserGrain.PatchCompanyUser(string name, params NameValue[] propertyValues)
        {
            CheckCompanyTeams();

            return ClusterClient.Default.GetGrain<IUserGrain>(name).Patch(Kernel.RootTeamsId.Value, propertyValues);
        }

        Task<int> IUserGrain.Patch(long rootTeamsId, params NameValue[] propertyValues)
        {
            CheckCompanyTeams(rootTeamsId);

            return Task.FromResult(Kernel.UpdateSelf(propertyValues));
        }

        #endregion

        #endregion
    }
}