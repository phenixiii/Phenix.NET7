using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Security.Auth;
using Phenix.Core.Threading;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security;
using Phenix.Services.Contract.Security.Myself;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 用户资料Grain
    /// key: CompanyName
    /// keyExtension: UserName
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
            get { return _rootTeamsId ??= AsyncHelper.RunSync(() => ClusterClient.GetKernelPropertyValueAsync<ICompanyTeamsGrain, Teams, long>(CompanyName, p => p.Id)); }
        }

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override User Kernel
        {
            get
            {
                if (base.Kernel == null)
                {
                    if (AsyncHelper.RunSync<bool>(() => ClusterClient.ExistKernelAsync<ICompanyTeamsGrain>(CompanyName)))
                        base.Kernel = User.FetchRoot(Database, p => p.RootTeamsId == RootTeamsId && p.Name == UserName);
                }
                return base.Kernel;
            }
        }

        private readonly IUserService _service;

        #endregion

        #region 方法

        async Task<string> IUserGrain.CheckIn(string phone, string eMail, string regAlias, string requestAddress)
        {
            string result;
            if (Kernel == null)
            {
                if (await ClusterClient.ExistKernelAsync<ICompanyTeamsGrain>(CompanyName))
                    throw new InvalidOperationException(String.Format("请更换公司名, '{0}'已被其他用户注册!", CompanyName));
                else
                    await ClusterClient.PatchKernelAsync<ICompanyTeamsGrain>(CompanyName,
                        NameValue.Set<Teams>(p => p.Name, CompanyName));

                string initialPassword = UserName;
                try
                {
                    Kernel = User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, RootTeamsId, RootTeamsId, null, ref initialPassword);
                }
                catch
                {
                    await ClusterClient.DeleteKernelAsync<ICompanyTeamsGrain>(CompanyName);
                    throw;
                }
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

        async Task IUserGrain.Logout()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            Kernel.Logout();
            if (_service != null)
                await _service.OnLogout();
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

            return Task.FromResult(Kernel.ChangePassword(ref password, ref newPassword, true, requestAddress, Kernel.RequestSession, throwIfNotConform));
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

        async Task<User> IUserGrain.FetchMyself()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Kernel.FetchMyself((await ClusterClient.GetGrain<ICompanyTeamsGrain>(CompanyName).FetchKernel()).FindInBranch(p => p.Id == Kernel.TeamsId),
                Kernel.PositionId.HasValue ? await ClusterClient.GetGrain<IPositionGrain>(Kernel.PositionId.Value).FetchKernel() : null);
        }

        #region CompanyAdmin 操作功能

        async Task<IList<User>> IUserGrain.FetchCompanyUsers()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Kernel.FetchCompanyUsers(await ClusterClient.GetGrain<ICompanyTeamsGrain>(CompanyName).FetchKernel(), Position.FetchKeyValues(Database, p => p.Id));
        }

        async Task<string> IUserGrain.Register(string phone, string eMail, string regAlias, string requestAddress, long teamsId, long positionId)
        {
            if (Kernel != null)
                throw new InvalidOperationException("登录名已被他人注册!");

            if (!await ClusterClient.GetGrain<ICompanyTeamsGrain>(CompanyName).HaveNode(teamsId, false))
                throw new InvalidOperationException("注册用户的团队不存在!");

            string initialPassword = UserName;
            Kernel = User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, teamsId, teamsId, positionId, ref initialPassword);
            return _service != null ? await _service.OnRegistered(Kernel, initialPassword) : null;
        }

        #endregion

        #endregion
    }
}