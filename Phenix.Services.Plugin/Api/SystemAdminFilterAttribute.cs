using System.Security;
using Microsoft.AspNetCore.Http;
using Phenix.Core.Net.Filters;
using Phenix.Core.Security;

namespace Phenix.Services.Plugin.Api
{
    /// <summary>
    /// 系统管理员过滤器
    /// </summary>
    public class SystemAdminFilterAttribute : AuthorizationFilterAttribute
    {
        /// <summary>
        /// 检查有效性
        /// </summary>
        /// <param name="identity">用户身份</param>
        /// <param name="context">HttpContext</param>
        public override void CheckValidity(Identity identity, HttpContext context)
        {
            if (!identity.UserProxy.GetKernelProperty<bool>(p => p.IsSystemAdmin).Result)
                throw new SecurityException("仅允许系统管理员操作本功能!");
        }
    }
}
