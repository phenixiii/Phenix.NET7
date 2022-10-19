using System;
using System.Threading.Tasks;
using Phenix.Core.Security;

namespace Phenix.Core.Net.Filters
{
    /// <summary>
    /// 访问授权过滤器标签
    /// </summary>

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class AuthorizationFilterAttribute : Attribute
    {
        #region 方法

        /// <summary>
        /// 检查有效性
        /// </summary>
        /// <param name="identity">用户身份</param>
        /// <param name="context">HttpContext</param>
        public abstract Task CheckValidity(IIdentity identity, Microsoft.AspNetCore.Http.HttpContext context);

        #endregion
    }
}
