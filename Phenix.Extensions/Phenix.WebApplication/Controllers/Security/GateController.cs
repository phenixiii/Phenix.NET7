using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Net;
using Phenix.Core.Security;

namespace Phenix.WebApplication.Controllers.Security
{
    /// <summary>
    /// 系统入口控制器
    /// 系统必备（配套 Phenix.Core.Net.AuthenticationMiddleware & phAjax 使用）
    /// </summary>
    [EnableCors]
    [Route(NetConfig.SecurityGatePath)]
    [ApiController]
    public sealed class GateController : Phenix.Core.Net.ControllerBase
    {
        // GET: /api/security/gate?name=林冲&phone=13966666666&eMail=13966666666@139.com&regAlias=豹子头
        // phAjax.checkIn()
        /// <summary>
        /// 登记/注册(获取动态口令)
        /// </summary>
        /// <param name="name">登录名(未注册则自动注册)</param>
        /// <param name="phone">手机(注册用可为空)</param>
        /// <param name="eMail">邮箱(注册用可为空)</param>
        /// <param name="regAlias">注册昵称(注册用可为空)</param>
        /// <returns>返回信息</returns>
        [AllowAnonymous]
        [HttpGet]
        public string CheckIn(string name, string phone, string eMail, string regAlias)
        {
            User result = Phenix.Core.Security.User.Fetch(name);
            if (result != null)
            {
                string dynamicPassword = result.ApplyDynamicPassword(Request.GetRemoteAddress());
                /*
                 * 以下代码，将动态口令 dynamicPassword 保存到日志中，供你自己测试用
                 * 生产环境下，请替换这段代码，改成通过第三方渠道（邮箱或短信）推送给到用户
                 */
                Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0} 的动态口令是'{1}'，有效期 {2} 分钟", result.Name, dynamicPassword, Phenix.Core.Security.User.DynamicPasswordValidityMinutes));
            }
            else
            {
                string password;
                string dynamicPassword;
                result = Phenix.Core.Security.User.New(name, phone, eMail, regAlias, Request.GetRemoteAddress(), out password, out dynamicPassword);
                /*
                 * 以下代码，将初始登录口令 password 和动态口令 dynamicPassword 保存到日志中，供你自己测试用
                 * 生产环境下，请替换这段代码，改成通过第三方渠道（邮箱或短信）推送给到用户
                 */
                Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0} 的初始登录口令是'{1}'，动态口令是'{2}'", result.Name, password, dynamicPassword));
            }

            /*
             * 以下代码，供你自己测试用
             * 生产环境下，请替换这段代码，改成提示用户留意查看邮箱或短信以收取动态口令
             */
            return String.Format("动态口令存放于 {0} 目录下的日志文件里", Phenix.Core.Log.EventLog.LocalDirectory);
        }

        // POST: /api/security/gate
        // phAjax.logon()
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns>当前登录用户资料</returns>
        [Authorize]
        [HttpPost]
        public User Logon()
        {
            /*
             * 代码执行到此，只要 User.Identity.IsAuthenticated 为 true 说明已顺利通过了 Phenix.Core.Net.AuthenticationMiddleware 身份验证
             * 除非你没有正确注册上 Phenix.Core.Net.AuthenticationMiddleware，需要检查 Startup 类
             *
             * User 是为了方便编程而提供的 Phenix.Core.Net.ControllerBase 的属性，指向的是 Phenix.Core.Security.Principal.CurrentPrincipal 对象
             * User.Identity 即 Phenix.Core.Security.Principal.CurrentPrincipal.Identity 对象和 Phenix.Core.Security.Identity.CurrentIdentity 是一个对象
             */

            /*
             * 这段代码，演示了如何解密客户端传来的 tag，用到了 Phenix.Core.Net.ControllerBase 提供的加解密系列函数 Decrypt()、Encrypt()，会自动把当前用户的二次MD5登录口令/动态口令作为密钥
             * tag 是客户端捎带数据（默认是客户端当前时间），你可以利用它顺带在登录时做些其他事情，扩展登录功能
             * 如果没有必要，你可以注释掉这段代码
             */
            string tag = DecryptBody();

            /*
             * User.Identity.User 对象提供了当前登录用户的详细信息
             * 你可以在客户端利用 User.Identity.User.Position.Roles 做些界面的权限控制
             * 如果客户端不需要用户资料，可以返回空值
             */
            return User.Identity.User;
        }
    }
}