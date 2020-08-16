using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;
using Phenix.Core;
using Phenix.Core.Security;
using Phenix.Core.Security.Auth;

namespace Phenix.Services.Plugin.Actor.Security
{
    /// <summary>
    /// 用户资料Grain
    /// </summary>
    public class UserGrain : EntityGrainBase<User>, IUserGrain
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="service">依赖注入的用户资料服务</param>
        public UserGrain(IUserService service)
        {
            _service = service;
        }

        #region 属性

        private readonly IUserService _service;

        #endregion

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
            return _service.OnRegistered(Kernel, initialPassword, dynamicPassword);
        }

        Task<string> IUserGrain.CheckIn(string phone, string eMail, string regAlias, string requestAddress)
        {
            return Task.FromResult(Kernel != null
                ? _service.OnCheckIn(Kernel, Kernel.ApplyDynamicPassword(requestAddress, true))
                : Register(phone, eMail, regAlias, requestAddress));
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

        async Task<bool> IUserGrain.IsValidLogon(string timestamp, string signature, string tag, string requestAddress, string requestSession, bool throwIfNotConform)
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
                    Kernel.ChangePassword(Kernel.Password, password, false, requestAddress, requestSession);
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

            if (Kernel.IsValidLogon(timestamp, signature, requestAddress, requestSession, throwIfNotConform))
            {
                _service.OnLogon(Kernel, Kernel.Decrypt(tag));
                return true;
            }

            return false;
        }

        Task<bool> IUserGrain.IsValid(string timestamp, string signature, string requestAddress, string requestSession, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.IsValid(timestamp, signature, requestAddress, requestSession, throwIfNotConform));
        }

        Task<bool> IUserGrain.ChangePassword(string password, string newPassword, string requestAddress, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.ChangePassword(password, newPassword, true, requestAddress, Kernel.RequestSession, throwIfNotConform));
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

        private void CheckCompanyTeams(long? rootTeamsId)
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
            ClusterClient.Default.GetGrain<ITeamsGrain>(result).PatchKernel(Teams.Set(p => p.Name, name));
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

        Task IUserGrain.PatchCompanyUser(string name, IDictionary<string, object> propertyValues)
        {
            CheckCompanyTeams();

            ClusterClient.Default.GetGrain<IUserGrain>(name).Patch(Kernel.RootTeamsId, propertyValues);
            return Task.CompletedTask;
        }

        Task IUserGrain.Patch(long? rootTeamsId, IDictionary<string, object> propertyValues)
        {
            CheckCompanyTeams(rootTeamsId);

            Kernel.UpdateSelf(propertyValues);
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}