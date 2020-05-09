using System.Security;
using Microsoft.AspNetCore.Http;
using Phenix.Core.Net.Filters;
using Phenix.Core.Security;

namespace Phenix.Services.Plugin.Api
{
    /// <summary>
    /// 公司管理员过滤器
    /// </summary>
    public class CompanyAdminFilterAttribute : AuthorizationFilterAttribute
    {
        /// <summary>
        /// 检查有效性
        /// </summary>
        /// <param name="identity">用户身份</param>
        /// <param name="context">HttpContext</param>
        public override void CheckValidity(Identity identity, HttpContext context)
        {
            if (!identity.UserProxy.GetKernelProperty<bool>(p => p.IsCompanyAdmin).Result)
                throw new SecurityException("仅允许拥有顶层级别的公司管理员操作本功能!");
        }
    }
}
