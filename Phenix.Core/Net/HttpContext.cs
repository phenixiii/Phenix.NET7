using Microsoft.AspNetCore.Http;

namespace Phenix.Core.Net
{
    /// <summary>
    /// 封装 IHttpContextAccessor.HttpContext 为静态属性
    /// 需与 HttpContextExtensions 配套使用
    /// </summary>
    public static class HttpContext
    {
        #region 属性

        private static IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// 当前 HttpContext
        /// </summary>
        public static Microsoft.AspNetCore.Http.HttpContext Current
        {
            get { return _contextAccessor.HttpContext; }
        }
        
        #endregion

        #region 方法

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        #endregion
    }
}