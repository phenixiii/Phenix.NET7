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
        protected ICompanyGrain CompanyTeamsGrain
        {
            get { return ClusterClient.GetGrain<ICompanyGrain>(CompanyName); }
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
                    if (AsyncHelper.RunSync(() => CompanyTeamsGrain.ExistKernel()))
                        base.Kernel = Phenix.Services.Business.Security.User.FetchRoot(Database, p => p.RootTeamsId == RootTeamsId && p.Name == UserName);
                }

                return base.Kernel;
            }
        }

        private readonly IUserService _service;

        #endregion

        #region 方法

        async Task<string> IUserGrain.Register(string phone, string eMail, string regAlias, string requestAddress, long? teamsId, long? positionId)
        {
            if (Kernel != null)
                throw new System.ComponentModel.DataAnnotations.ValidationException("不允许重复注册!");

            if (teamsId.HasValue && teamsId != RootTeamsId)
            {
                if (!await CompanyTeamsGrain.HaveNode(teamsId.Value, false))
                    throw new System.ComponentModel.DataAnnotations.ValidationException("设置的团队在公司里不存在!");
                if (!positionId.HasValue)
                    throw new System.ComponentModel.DataAnnotations.ValidationException("公司普通用户必须设置岗位!");
                if (!await ClusterClient.GetGrain<IPositionGrain>(positionId.Value).ExistKernel())
                    throw new System.ComponentModel.DataAnnotations.ValidationException("设置的岗位不存在!");

                string initialPassword = UserName;
                Kernel = Phenix.Services.Business.Security.User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, RootTeamsId, teamsId.Value, positionId, ref initialPassword);
                return _service != null ? await _service.OnRegistered(Kernel, initialPassword) : null;
            }

            if (await ClusterClient.GetGrain<ICompanyGrain>(CompanyName).ExistKernel())
                throw new System.ComponentModel.DataAnnotations.ValidationException("公司名已被其他用户注册!");

            await CompanyTeamsGrain.CreateKernel(NameValue.Set<Teams>(p => p.Name, CompanyName));
            try
            {
                string initialPassword = UserName;
                Kernel = Phenix.Services.Business.Security.User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, RootTeamsId, RootTeamsId, null, ref initialPassword);
                return _service != null ? await _service.OnRegistered(Kernel, initialPassword) : null;
            }
            catch
            {
                await CompanyTeamsGrain.DeleteKernel();
                throw;
            }
        }

        async Task<string> IUserGrain.CheckIn(string requestAddress)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            string dynamicPassword = Kernel.ApplyDynamicPassword(requestAddress);
            return _service != null ? await _service.OnCheckIn(Kernel, dynamicPassword) : null;
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

        Task<User> IUserGrain.FetchMyself()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel);
        }

        #endregion
    }
}