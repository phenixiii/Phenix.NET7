﻿using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Phenix.Core.DependencyInjection;
using Phenix.Core.Reflection;

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
             * 注入分组/用户消息服务，响应 phAjax.subscribeMessage() 请求 
             */
            services.AddSingleton<Phenix.Services.Plugin.Message.GroupMessageHub>();
            services.AddSingleton<Phenix.Services.Plugin.Message.UserMessageHub>();

            /*
             * 装配扩展服务
             * 扩展程序集都应该统一采用"*.Extend.dll"作为文件名的后缀
             * 扩展程序集都应该部署到本服务容器的执行目录下
             * 扩展服务都应该用"*Service"为类名和接口名的后缀
             * 扩展服务如非默认ServiceLifetime.Singleton可用ServiceAttribute标记
             */
            foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Extend.dll"))
            foreach (Type classType in Utilities.LoadExportedClassTypes(fileName, false))
                if (classType.Name.EndsWith("Service"))
                    foreach (Type interfaceType in classType.GetInterfaces())
                        if (interfaceType.Name.EndsWith("Service"))
                        {
                            ServiceAttribute serviceAttribute = (ServiceAttribute) Attribute.GetCustomAttribute(classType, typeof(ServiceAttribute));
                            services.Add(new ServiceDescriptor(interfaceType, classType, serviceAttribute != null ? serviceAttribute.Lifetime : ServiceLifetime.Singleton));
                        }

            /*
             * 配置Controller策略
             */
            services.AddControllers(options =>
                {
                    /*
                     * 如果想要的格式不支持，那么就会返回 406 NotAcceptable
                     */
                    options.ReturnHttpNotAcceptable = true;

                    /*
                     * 正常处理返回的空对象
                     */
                    options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();

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
                     * 程序运行时 Phenix.Core.Net.Filters.AuthorizationFilter 会尝试把 Controller/Action 上的标签 Roles 写入 PH7_Controller_Role 表中，曾写入过的不会被覆盖
                     * PH7_Controller_Role 表 CR_Roles 字段存储的是[Authorize]标签 Roles 属性值，如有多个[Authorize]就用‘,’分隔它们，字段为 null 时等同于[AllowAnonymous]
                     * 访问授权过滤器会优先采纳 PH7_Controller_Role 表的记录（一旦写入表后，代码里的标签会被忽略掉不再有效用，除非删除对应的那条 PH7_Controller_Role 表记录）
                     * 你可以基于 PH7_Controller_Role 表，开发自己系统的 Controller/Action 访问权限的配置管理模块
                     *
                     * 验证失败的话返回 context.Response.StatusCode = 403 Forbidden
                     */
                    options.Filters.Add<Phenix.Core.Net.Filters.AuthorizationFilter>();
                    /*
                     * 注册数据验证过滤器
                     * 与 Action 参数上的数据验证（[Required]、[StringLength] 等 ValidationAttribute）标签配合完成服务访问参数校验功能
                     *
                     * 验证失败的话返回 context.Response.StatusCode = 400 BadRequest，context.Response.Content 是 ValidationMessage 对象，其 StatusCode 属性为 400，ErrorMessage 属性为验证错误消息
                     */
                    options.Filters.Add<Phenix.Core.Net.Filters.ValidationFilter>();
                })
                .ConfigureApplicationPartManager(parts =>
                {
                    /*
                     * 装配Controller插件
                     * 插件程序集都应该统一采用"*.Plugin.dll"作为文件名的后缀
                     * 插件程序集都应该被部署到本服务容器的执行目录下
                     */
                    foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Plugin.dll"))
                        parts.ApplicationParts.Add(new AssemblyPart(Assembly.LoadFrom(fileName)));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.DateFormatString = Utilities.JsonDateFormatString;
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    options.UseMemberCasing();
                });
             
            /*
             * 关闭自动验证——模型验证
             */
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            /*
             * 配置转接头中间件（代理服务器和负载均衡器）
             * 如果设备使用 X-Forwarded-For 和 X-Forwarded-Proto 以外的其他标头名称，请设置 ForwardedForHeaderName 和 ForwardedProtoHeaderName 选项，使其与设备所用的标头名称相匹配
             * 本配置可让 Phenix.Services.Plugin.AuthenticationMiddleware 为系统记录下发起访问的客户端IP地址（见 Phenix.Core.Security.User.RequestAddress 属性值）
             */
            services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            //app.UseRequestLocalization(new RequestLocalizationOptions
            //{
            //    DefaultRequestCulture = new RequestCulture("zh"),
            //    SupportedCultures = new[]
            //    {
            //        new CultureInfo("zh"),
            //        new CultureInfo("en")
            //    },
            //    SupportedUICultures = new[]
            //    {
            //        new CultureInfo("zh"),
            //        new CultureInfo("en")
            //    }
            //});

            /*
             * 使用转接头中间件（代理服务器和负载均衡器）
             * 策略见 ConfigureServices 函数里的 services.Configure<ForwardedHeadersOptions>() 以适应部署环境
             */
            app.UseForwardedHeaders();

            /*
             * 使用异常处理中间件
             */
            app.UseMiddleware<Phenix.Services.Plugin.Middleware.ExceptionHandlerMiddleware>();

            /*
             * 使用身份验证中间件
             */
            app.UseMiddleware<Phenix.Services.Plugin.Middleware.AuthenticationMiddleware>();

            /*
             * 必要的话，请注册第三方客户端IP限流控制中间件
             * 所有打上[AllowAnonymous]标签的 Controller/Action 都应该被限流
             */

            /*
             * 以下代码务必放在最后执行
             */
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute().RequireAuthorization();

                /*
                 * 使用分组/用户消息服务，响应 phAjax.subscribeMessage() 请求
                 * 如果部署环境使用了 Nginx 等代理服务器或负载均衡器，类似 proxy_set_header Connection 配置项要从请求头里面获取，比如 proxy_set_header Connection $http_connection;
                 * 负载均衡器应该开启会话保持功能（客户端登录后的请求要一直落到同一台服务器上），配置会话保持类型为源IP（按访问IP的hash结果分配响应的应用服务器）
                 */
                endpoints.MapHub<Phenix.Services.Plugin.Message.GroupMessageHub>(Phenix.Services.Plugin.ApiConfig.ApiMessageGroupMessageHubPath);
                endpoints.MapHub<Phenix.Services.Plugin.Message.UserMessageHub>(Phenix.Services.Plugin.ApiConfig.ApiMessageUserMessageHubPath);
            });
        }
    }
}