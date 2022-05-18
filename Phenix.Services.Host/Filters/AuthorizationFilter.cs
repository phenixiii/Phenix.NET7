using System.Security;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Phenix.Core.Security;
using Phenix.Core.Threading;

namespace Phenix.Services.Host.Filters
{
    /// <summary>
    /// 访问授权过滤器
    /// </summary>
    public class AuthorizationFilter : IAuthorizationFilter
    {
        #region 属性

        private static bool _paused;

        /// <summary>
        /// 暂停
        /// 缺省为 false
        /// </summary>
        public static bool Paused
        {
            get { return _paused; }
        }

        private static string _pauseReason;

        /// <summary>
        /// 暂停原因
        /// </summary>
        public static string PauseReason
        {
            get { return _pauseReason; }
        }
        
        #endregion

        #region 方法

        /// <summary>
        /// 处理访问授权
        /// </summary>
        /// <param name="context">上下文</param>
        public virtual void OnAuthorization(AuthorizationFilterContext context)
        {
            if (Paused)
                throw new SecurityException(PauseReason);

            if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                ControllerRole controllerRole = ControllerRole.Fetch(descriptor.ControllerTypeInfo, descriptor.MethodInfo);
                if (controllerRole != null)
                    AsyncHelper.RunSync(() => controllerRole.CheckValidityAsync(Principal.CurrentIdentity, context.HttpContext));
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public static void Pause(string reason)
        {
            _paused = true;
            _pauseReason = reason;
        }

        /// <summary>
        /// 激活
        /// </summary>
        public static void Activate()
        {
            _paused = false;
            _pauseReason = null;
        }

        #endregion
    }
}