using System;
using System.Text;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Message;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Extend.Security
{
    /// <summary>
    /// 用户资料服务
    /// </summary>
    public sealed class UserService : IUserService
    {
        #region 方法

        Task<string> IUserService.OnRegistered(User user, string initialPassword)
        {
            return Task.FromResult(String.Format("注册成功。在首次登录前请将登录口令(初始同登录名)改为强口令(长度需大于等于{0}个字符且至少包含数字、大小写字母、特殊字符之{1}种)。",
                Principal.PasswordLengthMinimum, Principal.PasswordComplexityMinimum));
        }

        async Task<string> IUserService.OnCheckIn(User user, string dynamicPassword)
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
                    await ClusterClient.Default.GetGrain<IEmailGrain>("PH").Send(user.RegAlias ?? user.Name, user.EMail, "获取动态口令", true, mailBody.ToString());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("获取动态口令失败!", ex);
                }
                return String.Format("本次申领的动态口令已发送到您登记的邮箱，请注意查收，并请在{0}分钟内使用动态口令登录系统。", Principal.DynamicPasswordValidityMinutes);
            }

            return "您未曾登记过邮箱，无法收到动态口令。您可以联系系统管理员，为您重置登录口令。";
        }

        Task IUserService.OnLogon(User user, string tag)
        {
            /*
             * 本函数被执行到，说明用户已通过身份验证
             * 可利用客户端传过来的 tag 扩展出系统自己的用户登录功能
             */
            return Task.CompletedTask;
        }

        Task IUserService.OnLogout(User user)
        {
            /*
             * 本函数被执行到，说明用户已退出系统
             */
            return Task.CompletedTask;
        }

        #endregion
    }
}