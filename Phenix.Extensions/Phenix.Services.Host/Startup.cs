using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Phenix.Services.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /*
             * 配置跨域请求响应策略
             * 请根据自己系统的安全需要再精细化管控访问限制
             */
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => builder
                    .SetIsOriginAllowed(origin => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            /*
             * 开启SignalR服务
             */
            services.AddSignalR(options => { options.MaximumReceiveMessageSize = Int16.MaxValue; }).AddMessagePackProtocol();

            /*
             * 注入用户消息服务 
             */
            services.AddUserMessageHub();

            /*
             * 注入系统入口服务 
             */
            services.AddGateService<Phenix.Services.GateService>();

            /*
             * 注入文件存取服务 
             */
            services.AddFileService<Phenix.Services.FileService>();

            /*
             * 配置Controller策略
             */
            services.AddControllers(options =>
                {
                    /*
                     * 如果想要的格式不支持，那么就会返回 406 NotAcceptable
                     * 默认返回 application/json，如果不希望支持 application/xml 请注释掉 options.OutputFormatters.Add 代码，当然也可以添加其他的格式
                     */
                    options.ReturnHttpNotAcceptable = true;
                    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                    /*
                     * 注册访问授权过滤器 
                     * 与 Controller/Action 上的[AllowAnonymous]/[Authorize(Roles="角色1|角色2")]标签配合完成用户的访问授权功能
                     * 按照就近原则 Action 上的标签优先于 Controller 上的标签（即忽略 Controller 上的标签）
                     *
                     * [AllowAnonymous]标签等同于不打[Authorize]标签
                     * [Authorize]标签声明的 Roles 应该与 Phenix.Core.Security.Identity.CurrentIdentity.User.Position.Roles（即 PH7_Position 表的 PT_Roles 字段）处于一套语境（注意大小写敏感）
                     * [Authorize]标签无参数（即 Roles 属性值为 null）时，不允许匿名用户访问但允许所有注册用户访问
                     * [Authorize]标签 Roles 声明的多个角色用‘|’分隔，互相为 or 关系
                     * 在 Controller/Action 上允许打多个[Authorize]标签，互相为 and 关系（也可以写在一个标签里用‘,’分隔）
                     *
                     * 程序运行时 Phenix.Core.Net.AuthorizationFilter 会尝试把 Controller/Action 上的标签 Roles 写入 PH7_Controller_Role 表中，曾写入过的不会被覆盖
                     * PH7_Controller_Role 表 CR_Roles 字段存储的是[Authorize]标签 Roles 属性值，如有多个[Authorize]就用‘,’分隔它们，字段为 null 时等同于[AllowAnonymous]
                     * 你可以基于 PH7_Controller_Role 表，开发自己系统的 Controller/Action 访问权限的配置管理模块（本工程提供了基本的控制器代码），然后开放权限给到系统Admin管理员使用
                     * 访问授权过滤器会优先采纳 PH7_Controller_Role 表的记录（一旦写入表后，代码里的标签其实就被忽略掉了，除非删除相应的记录）
                     *
                     * 验证失败的话 context.Response.StatusCode = 403 Forbidden
                     *
                     * 系统Admin管理员的角色是‘Admin’（Phenix.Core.Security.User.AdminRoleName），权限范围仅限于访问带[Authorize(Roles=Phenix.Core.Security.User.AdminRoleName)]标签的 Controller/Action
                     */
                    options.Filters.AddAuthorizationFilter();
                })
                .ConfigureApplicationPartManager(options =>
                {
                    /*
                     * 装配核心控制器，包含：
                     *   Phenix.Core.Net.Api.GateController 响应 phAjax.checkIn()、phAjax.logon() 请求
                     *   Phenix.Core.Net.Api.MyselfController 响应 phAjax.getMyself()、phAjax.changePassword() 请求
                     *   Phenix.Core.Net.Api.SequenceController 响应 phAjax.getSequence() 请求
                     *   Phenix.Core.Net.Api.IncrementController 响应 phAjax.getIncrement() 请求
                     *   Phenix.Core.Net.Api.UserMessageController 响应 phAjax.sendMessage()、phAjax.receiveMessage()、phAjax.affirmReceivedMessage() 请求
                     */
                    options.AddKernelPart();
                    /*
                     * 装配插件控制器
                     * 系统的 Controller 都应该按照领域划分开发各自的插件程序集，部署到本服务容器的执行目录下
                     * 插件程序集的命名，都应该统一采用"*.Plugin.dll"作为文件名的后缀
                     */
                    options.AddPluginPart();
                });

            /*
             * 配置转接头中间件（代理服务器和负载均衡器）
             * 如果设备使用 X-Forwarded-For 和 X-Forwarded-Proto 以外的其他标头名称，请设置 ForwardedForHeaderName 和 ForwardedProtoHeaderName 选项，使其与设备所用的标头名称相匹配
             * 本配置可让 Phenix.Core.Net.AuthenticationMiddleware 为系统记录下发起访问的客户端IP地址（见 Phenix.Core.Security.Identity.CurrentIdentity.User.RequestAddress 属性值）
             */
            services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            /*
             * 使用CORS中间件（响应跨域请求）
             * 策略见 ConfigureServices 函数里的 services.AddCors() 限制条件
             */
            app.UseCors();

            /*
             * 使用转接头中间件（代理服务器和负载均衡器）
             * 策略见 ConfigureServices 函数里的 services.Configure<ForwardedHeadersOptions>() 以适应部署环境
             */
            app.UseForwardedHeaders();

            /*
             * 使用异常处理中间件
             * 拦截异常并转译成 context.Response.StatusCode，异常详情见报文体：
             *   System.InvalidOperationException 转译为 400 BadRequest
             *   System.Security.Authentication.AuthenticationException 转译为 401 Unauthorized
             *   System.Security.SecurityException 转译为 403 Forbidden
             *   System.ComponentModel.DataAnnotations.ValidationException 转译为 409 Conflict
             *   System.NotSupportedException/System.NotImplementedException 转译为 501 NotImplemented
             * 除以上之外的异常都转译为 500 InternalServerError
             */
            app.UseExceptionHandlerMiddleware();

            /*
             * 使用身份验证中间件
             * 与客户端接口 phenix.js 一起实现用户的身份验证功能
             * 你也可以开发自己的客户端接口（比如桌面端、APP应用端），仅需在报文上添加身份验证 Header
             * 身份验证 Header 格式为 Phenix-Authorization=[登录名],[时间戳(9位长随机数+ISO格式当前时间)],[签名(二次MD5登录口令/动态口令AES加密的时间戳)]
             * 登录口令/动态口令应该通过第三方渠道（邮箱或短信）推送给到用户，由用户输入到系统提供的客户端登录界面上，用于加密时间戳生成报文的签名
             * 用户登录成功后，客户端程序要将二次MD5登录口令/动态口令缓存在本地，以便每次向服务端发起 call 时都能为报文添加上身份验证 Header
             * 如果报文上没有 Phenix-Authorization 身份验证 Header，会被 Phenix.Core.Net.AuthenticationMiddleware 当作是匿名用户
             * 匿名用户的访问经过 Phenix.Core.Net.AuthorizationFilter 后，仅允许访问到带[AllowAnonymous]标签或不打[Authorize]标签的 Controller/Action             * 
             *
             * 验证失败的话 context.Response.StatusCode = 401 Unauthorized，失败详情见报文体
             * 验证成功的话 Phenix.Core.Security.Identity.CurrentIdentity.IsAuthenticated = true 且 context.User 会被赋值为 new ClaimsPrincipal(Phenix.Core.Security.Identity.CurrentIdentity)
             *
             * 系统Admin管理员的登录名是‘ADMIN’，初始登录口令也是‘ADMIN’（注意是大写），在系统部署到生产环境正式上线前，你应该用‘ADMIN’登录一次系统，把口令的复杂度修改成达标的
             */
            app.UseAuthenticationMiddleware();

            /*
             * 必要的话，请注册第三方客户端IP限流控制中间件
             * 所有打上[AllowAnonymous]标签的 Controller/Action 都应该被限流
             */

            /*
             * 以下代码务必被最后执行到
             */
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute().RequireAuthorization();
                /*
                 * 使用用户消息服务
                 * 如果部署环境使用了 Nginx 等代理服务器或负载均衡器，类似 proxy_set_header Connection 配置项要从请求头里面获取，比如 proxy_set_header Connection $http_connection;
                 * 负载均衡器应该开启会话保持功能（客户端登录后的请求要一直落到同一台服务器上），配置会话保持类型为源IP（按访问IP的hash结果分配响应的应用服务器）
                 */
                endpoints.MapUserMessageHub();
            });
        }
    }
}