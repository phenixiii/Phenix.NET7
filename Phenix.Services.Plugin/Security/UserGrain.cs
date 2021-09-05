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

        /// <summary>
        /// 公司团队Grain接口
        /// </summary>
        protected ICompanyTeamsGrain CompanyTeamsGrain
        {
            get { return ClusterClient.GetGrain<ICompanyTeamsGrain>(CompanyName); }
        }

        private long? _rootTeamsId;

        /// <summary>
        /// 所属公司ID
        /// </summary>
        protected long RootTeamsId
        {
            get { return _rootTeamsId ??= AsyncHelper.RunSync(() => CompanyTeamsGrain.GetKernelPropertyValue(p => p.Id)); }
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
                    if (AsyncHelper.RunSync<bool>(() => CompanyTeamsGrain.ExistKernel()))
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
            if (Kernel != null)
            {
                List<NameValue<User>> nameValues = new List<NameValue<User>>(3);
                if (!String.IsNullOrEmpty(phone))
                    nameValues.Add(NameValue.Set<User>(p => p.Phone, phone));
                if (!String.IsNullOrEmpty(eMail))
                    nameValues.Add(NameValue.Set<User>(p => p.EMail, eMail));
                if (!String.IsNullOrEmpty(regAlias))
                    nameValues.Add(NameValue.Set<User>(p => p.RegAlias, regAlias));
                if (nameValues.Count > 0)
                    Kernel.UpdateSelf(nameValues.ToArray());

                if (Kernel.IsInitialPassword)
                    return _service != null ? await _service.OnRegistered(Kernel, Kernel.Name) : null;
                string dynamicPassword = Kernel.ApplyDynamicPassword(requestAddress);
                return _service != null ? await _service.OnCheckIn(Kernel, dynamicPassword) : null;
            }

            if (await ClusterClient.GetGrain<ICompanyTeamsGrain>(CompanyName).ExistKernel())
                throw new InvalidOperationException(String.Format("请更换公司名, '{0}'已被其他用户注册!", CompanyName));

            string initialPassword = UserName;
            await CompanyTeamsGrain.PatchKernel(NameValue.Set<Teams>(p => p.Name, CompanyName));
            try
            {
                Kernel = User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, RootTeamsId, RootTeamsId, null, ref initialPassword);
            }
            catch
            {
                await CompanyTeamsGrain.DeleteKernel();
                throw;
            }

            return _service != null ? await _service.OnRegistered(Kernel, initialPassword) : null;
        }

        async Task IUserGrain.Logon(string signature, string tag, string requestAddress)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            Kernel.Logon(signature, ref tag, requestAddress);
            if (_service != null)
                await _service.OnLogon(Kernel, tag);
        }

        Task IUserGrain.Verify(string signature, string requestAddress)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            Kernel.Verify(signature, requestAddress);
            return Task.CompletedTask;
        }

        async Task IUserGrain.Logout()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            Kernel.Logout();
            if (_service != null)
                await _service.OnLogout(Kernel);
        }

        Task IUserGrain.ResetPassword()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            Kernel.ResetPassword();
            return Task.CompletedTask;
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

            Teams teams = await CompanyTeamsGrain.GetNode(Kernel.TeamsId);
            Position position = Kernel.PositionId.HasValue ? await ClusterClient.GetGrain<IPositionGrain>(Kernel.PositionId.Value).FetchKernel() : null;
            return Kernel.FetchMyself(teams, position);
        }

        #region CompanyAdmin 操作功能

        async Task<IList<User>> IUserGrain.FetchCompanyUsers()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            IList<User> result = User.FetchList(Database, p => p.RootTeamsId == RootTeamsId && p.RootTeamsId != p.TeamsId);
            Teams companyTeams = await CompanyTeamsGrain.FetchKernel();
            IDictionary<long, Position> positions = Position.FetchKeyValues(Database, p => p.Id);
            foreach (User item in result)
                item.FetchMyself(companyTeams.FindInBranch(p => p.Id == item.TeamsId),
                    item.PositionId.HasValue && positions.TryGetValue(item.PositionId.Value, out Position position) ? position : null);
            return result;
        }

        async Task<string> IUserGrain.Register(string phone, string eMail, string regAlias, string requestAddress, long teamsId, long positionId)
        {
            if (Kernel != null)
                throw new InvalidOperationException("登录名已被他人注册!");
            if (!await CompanyTeamsGrain.HaveNode(teamsId, false))
                throw new InvalidOperationException("注册用户的团队不存在!");

            string initialPassword = UserName;
            Kernel = User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, teamsId, teamsId, positionId, ref initialPassword);
            return _service != null ? await _service.OnRegistered(Kernel, initialPassword) : null;
        }

        #endregion

        #endregion
    }
}