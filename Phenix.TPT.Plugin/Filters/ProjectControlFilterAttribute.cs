using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Phenix.Core.Net.Filters;
using Phenix.Core.Security;
using Phenix.TPT.Business.Norm;

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
            if (!await identity.IsInRole(ProjectRoles.经营管理, ProjectRoles.项目管理))
                throw new SecurityException("仅限公司管理人员操作本功能!");
        }
    }
}
