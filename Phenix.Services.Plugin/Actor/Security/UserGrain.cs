using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core;
using Phenix.Core.Security.Auth;
using Phenix.Core.Threading;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security;

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
        /// <param name="service">注入用户资料服务</param>
        public UserGrain(IUserService service)
        {
            _service = service;
        }

        #region 属性

        #region 配置项
        
        private static int? _keyPairDiscardIntervalSeconds;

        /// <summary>
        /// 公钥私钥对丢弃间隔(秒)
        /// 默认：60
        /// </summary>
        public static int KeyPairDiscardIntervalSeconds
        {
            get { return AppSettings.GetProperty(ref _keyPairDiscardIntervalSeconds, 60); }
            set { AppSettings.SetProperty(ref _keyPairDiscardIntervalSeconds, value); }
        }

        #endregion

        /// <summary>
        /// 公司名
        /// </summary>
        protected string CompanyName
        {
            get { return PrimaryKeyString; }
        }

        /// <summary>
        /// 登录名
        /// </summary>
        protected string UserName
        {
            get { return PrimaryKeyExtension; }
        }

        private long? _rootTeamsId;

        /// <summary>
        /// 所属公司ID
        /// </summary>
        protected long RootTeamsId
        {
            get { return _rootTeamsId ??= AsyncHelper.RunSync(() => ClusterClient.GetKernelPropertyAsync<ITeamsGrain, Phenix.Core.Security.Teams, long>(CompanyName, p => p.Id)); }
        }

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override User Kernel
        {
            get { return base.Kernel ??= User.FetchRoot(Database, p => p.RootTeamsId == RootTeamsId && p.Name == UserName); }
        }

        private readonly IUserService _service;

        #endregion

        #region 方法

        async Task<string> IUserGrain.CheckIn(string phone, string eMail, string regAlias, string requestAddress)
        {
            string result;
            if (Kernel == null)
            {
                string initialPassword = UserName;
                Kernel = User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, RootTeamsId, RootTeamsId, null, ref initialPassword);
                result = _service != null ? await _service.OnRegistered(Kernel, initialPassword) : null;
            }
            else
            {
                string dynamicPassword = Kernel.ApplyDynamicPassword(requestAddress);
                result = _service != null ? await _service.OnCheckIn(Kernel, dynamicPassword) : null;
            }

            return result;
        }

        async Task<bool> IUserGrain.IsValidLogon(string timestamp, string signature, string tag, string requestAddress, string requestSession, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            if (Kernel.IsValidLogon(timestamp, signature, requestAddress, requestSession, throwIfNotConform))
            {
                if (_service != null)
                    await _service.OnLogon(Kernel, Kernel.Decrypt(tag));
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

        Task<bool> IUserGrain.ResetPassword()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.ResetPassword());
        }

        Task<bool> IUserGrain.ChangePassword(string password, string newPassword, string requestAddress, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.ChangePassword(password, newPassword, true, requestAddress, Kernel.RequestSession, throwIfNotConform));
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

        #region CompanyAdmin 操作功能

        async Task<IList<User>> IUserGrain.FetchCompanyUsers()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Kernel.FetchCompanyUsers(await ClusterClient.GetGrain<ITeamsGrain>(CompanyName).FetchKernel());
        }

        async Task<string> IUserGrain.Register(string phone, string eMail, string regAlias, string requestAddress, long teamsId, long positionId)
        {
            if (Kernel != null)
                throw new InvalidOperationException("登录名已被他人注册!");

            if (!await ClusterClient.GetGrain<ITeamsGrain>(CompanyName).HaveNode(teamsId, false))
                throw new InvalidOperationException("注册用户的团队不存在!");

            string initialPassword = UserName;
            Kernel = User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, teamsId, teamsId, positionId, ref initialPassword);
            return _service != null ? await _service.OnRegistered(Kernel, initialPassword) : null;
        }

        #endregion

        #endregion
    }
}