using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// HttpContext扩展
    /// 需与 Phenix.Core.Net.HttpContext 配套使用
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// 程序 Startup 时 Configure() 的 IApplicationBuilder app 参数中获取 IHttpContextAccessor 并挂载到 Phenix.Core.Net.HttpContext.Current 静态属性上: app.UseStaticHttpContext();
        /// 需事先在 ConfigureServices() 的 IServicesCollection services 参数中注入 HttpContextAccessor 服务为 IHttpContextAccessor: services.AddHttpContextAccessor();
        /// </summary>
        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder builder)
        {
            IHttpContextAccessor httpContextAccessor = builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            Phenix.Core.Net.HttpContext.Configure(httpContextAccessor);
            return builder;
        }
    }
}
