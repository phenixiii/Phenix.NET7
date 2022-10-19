using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Phenix.Core.Security;

namespace Phenix.Core.Net.Api
{
    /// <summary>
    /// 控制器基类
    /// </summary>
    [EnableCors]
    public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        #region 属性

        /// <summary>
        /// 用户身份
        /// </summary>
        public new Principal User
        {
            get { return Principal.CurrentPrincipal; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 加密
        /// Key/IV=登录口令/动态口令
        /// 对应 phAjax.decrypt 函数
        /// </summary>
        /// <param name="sourceData">需加密的对象/字符串</param>
        protected async Task<string> EncryptAsync(object sourceData)
        {
            if (User == null || User.Identity == null)
                throw new AuthenticationException();

            return await User.Identity.Encrypt(sourceData);
        }

        #endregion
    }
}
