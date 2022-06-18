using System;
using System.Text;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Core.Security.Auth;
using Phenix.Core.Threading;
using Phenix.Mapper.Expressions;
using Phenix.Services.Library.Message;
using Phenix.Services.Library.Security.Myself;

namespace Phenix.Services.Library.Security
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
        public UserGrain()
        {
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
            get { return GrainFactory.GetGrain<ICompanyGrain>(CompanyName); }
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
                        base.Kernel = Phenix.Services.Library.Security.User.FetchRoot(Database, p => p.RootTeamsId == RootTeamsId && p.Name == UserName);
                }

                return base.Kernel;
            }
        }

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
                if (!await GrainFactory.GetGrain<IPositionGrain>(positionId.Value).ExistKernel())
                    throw new System.ComponentModel.DataAnnotations.ValidationException("设置的岗位不存在!");

                string initialPassword = UserName;
                Kernel = Phenix.Services.Library.Security.User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, RootTeamsId, teamsId.Value, positionId, ref initialPassword);
                goto Label;
            }

            if (await CompanyTeamsGrain.IsValid())
                throw new System.ComponentModel.DataAnnotations.ValidationException("公司已被其他组织使用!");

            await CompanyTeamsGrain.PutKernel(NameValue.Set<Teams>(p => p.Name, CompanyName), true, true);
            try
            {
                string initialPassword = UserName;
                Kernel = Phenix.Services.Library.Security.User.Register(Database, UserName, phone, eMail, regAlias, requestAddress, RootTeamsId, RootTeamsId, null, ref initialPassword);
            }
            catch
            {
                await CompanyTeamsGrain.DeleteKernel();
                throw;
            }

            Label:
            return String.Format("注册成功。在首次登录前请将登录口令(初始同登录名)改为强口令(长度需大于等于{0}个字符且至少包含数字、大小写字母、特殊字符之{1}种)。",
                Principal.PasswordLengthMinimum, Principal.PasswordComplexityMinimum);
        }

        async Task<string> IUserGrain.CheckIn(string requestAddress)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            string dynamicPassword = Kernel.ApplyDynamicPassword(requestAddress);
            return await OnCheckIn(Kernel, dynamicPassword);
        }

        private async Task<string> OnCheckIn(User user, string dynamicPassword)
        {
            if (!String.IsNullOrEmpty(user.EMail))
            {
                StringBuilder mailBody = new StringBuilder();
                mailBody.Append("亲爱的&nbsp;" + user.RegAlias + "&nbsp;会员：<br/>");
                mailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;您好！<br/>");
                mailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;您于&nbsp;" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                mailBody.Append("&nbsp;申领的动态口令为：" + dynamicPassword + "<br/>");
                mailBody.Append("&nbsp;&nbsp;<font color=red>(" + Principal.DynamicPasswordValidityMinutes + "分钟内有效)</font><br/>");
                mailBody.Append("&nbsp;如非本人操作，请忽略本邮件。<br/>");
                try
                {
                    await GrainFactory.GetGrain<IEmailGrain>("PH").Send(user.RegAlias ?? user.Name, user.EMail, "获取动态口令", true, mailBody.ToString());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("获取动态口令失败!", ex);
                }

                return String.Format("本次申领的动态口令已发送到您登记的邮箱，请注意查收，并请在{0}分钟内使用动态口令登录系统。", Principal.DynamicPasswordValidityMinutes);
            }

            return "您未曾登记过邮箱，无法收到动态口令。您可以联系系统管理员，为您重置登录口令。";
        }

        Task IUserGrain.Logon(string signature, string tag, string requestAddress)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            Kernel.Logon(signature, ref tag, requestAddress);
            return Task.CompletedTask;
        }

        Task IUserGrain.Verify(string signature, string requestAddress)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            Kernel.Verify(signature, requestAddress);
            return Task.CompletedTask;
        }

        Task IUserGrain.Logout()
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            Kernel.Logout();
            return Task.CompletedTask;
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