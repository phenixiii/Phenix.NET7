using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Phenix.Core.Net.Filters;
using Phenix.Core.Security;

namespace Phenix.TPT.Plugin.Filters
{
    /// <summary>
    /// 项目管控过滤器
    /// </summary>
    public class ProjectControlFilterAttribute : AuthorizationFilterAttribute
    {
        /// <summary>
        /// 检查有效性
        /// </summary>
        /// <param name="identity">用户身份</param>
        /// <param name="context">HttpContext</param>
        public override async Task CheckValidity(IIdentity identity, HttpContext context)
        {
            if (!await identity.IsInRole(Roles.经营管理.ToString(), Roles.项目管理.ToString()))
                throw new SecurityException("仅允许管理人员操作本功能!");
        }
    }
}
