using System.Security;
using System.Threading.Tasks;
using Phenix.Core.Security;

namespace Phenix.Core.Net.Filters
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
        public override Task CheckValidity(IIdentity identity, Microsoft.AspNetCore.Http.HttpContext context)
        {
            if (!identity.IsCompanyAdmin)
                throw new SecurityException(AppSettings.GetValue("仅允许拥有顶层级别的公司管理员操作本功能!"));
            return Task.CompletedTask;
        }
    }
}
