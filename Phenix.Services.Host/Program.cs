using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;

namespace Phenix.Services.Host
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => { Phenix.Core.Log.EventLog.SaveLocal("An unhandled exception occurred in the current domain", (Exception) eventArgs.ExceptionObject); };

            /*
             * 可在此注册系统用到的各数据库的连接串
             * 但建议通过SQLite库Phenix.Core.db文件的PH7_Database表预先配置，系统在执行到Phenix.Core.Data.Database.Fetch()时会被自动加载
             * 以下注释掉的代码，是注册缺省数据库连接串，也就是Phenix.Core.Data.Database.Default的内容，相当于PH7_Database表中那条DataSourceKey字段值为'*'的记录
             */
            //Phenix.Core.Data.Database.RegisterDefault("192.168.248.52", null, "TEST", "SHBPMO", "SHBPMO");

            /*
             * 注册用户资料工厂，以打通封装在Phenix.Services.Plugin的UserGrain中的用户身份验证等功能
             */
            Phenix.Core.Security.Identity.RegisterFactory(new Phenix.Services.Plugin.UserProxyFactory());

            /*
             * 构建Host并启动服务
             * 如第一次启动，可在wwwroot\test目录里打开各个测试网页，验证服务环境是否正常
             */
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.AddConsole())
                .UseContentRoot(Phenix.Core.AppRun.BaseDirectory)
                /*
                 * 启动Orleans服务集群
                 * 请事先在数据库中手工添加Orleans配置库，默认是Phenix.Core.Data.Database.Default指向的数据库
                 * Orleans配置库的脚本文件，见Orleans Database Script目录，分为MySQL和Oracle两组，建议按需顺序执行
                 */
                .UseOrleans((context, builder) => builder
                    /*
                     * 设置集群ID：Phenix.Core.Data.Database.Default.DataSourceKey
                     * 设置服务ID：Phenix.Core.Data.Database.Default.DataSourceKey
                     * 设置Clustering、GrainStorage、Reminder数据库：Phenix.Core.Data.Database.Default
                     * 设置SimpleMessageStreamProvider：Phenix.Actor.StreamProvider.Name
                     *
                     * 装配Actor插件
                     * 系统的Actor都应该按照领域划分，分别开发各自的插件
                     * 插件程序集的部署，都存放在本服务容器的当前执行目录下
                     * 插件程序集的命名，都应该统一采用"*.Plugin.dll"作为文件名的后缀
                     * 用户的身份验证和访问授权等功能，由Actor插件Phenix.Services.Plugin中的UserGrain提供
                     */
                    .ConfigureCluster(OrleansConfig.ClusterId, OrleansConfig.ServiceId)
                    /*
                     * 设置Silo端口：EndpointOptions.DEFAULT_SILO_PORT
                     * 设置Gateway端口：EndpointOptions.DEFAULT_SILO_PORT
                     */
                    .ConfigureEndpoints(OrleansConfig.DefaultSiloPort, OrleansConfig.DefaultGatewayPort))
                /*
                 * 启动WebAPI服务
                 */
                .ConfigureWebHostDefaults(builder =>
                {
                    /*
                     * 使用轻量级跨平台服务 KestrelServer
                     * 请根据自己系统的运行要求配置 KestrelServer 选项
                     */
                    builder.UseKestrel(options =>
                        {
                            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2); //保持活动状态超时
                            options.Limits.MaxConcurrentConnections = 1000; //客户端最大连接数
                            options.Limits.MaxConcurrentUpgradedConnections = 1000; //客户端最大连接数（其他协议如Websocket）
                            options.Limits.MaxRequestBodySize = Int32.MaxValue; //请求正文最大大小
                            options.Limits.MaxRequestBufferSize = Int32.MaxValue; //请求缓存最大大小
                            options.Limits.MinRequestBodyDataRate =
                                new MinDataRate(bytesPerSecond: 240, gracePeriod: TimeSpan.FromSeconds(5)); //请求正文最小数据速率
                            options.Limits.MaxResponseBufferSize = Int32.MaxValue; //响应缓存最大大小
                            options.Limits.MinResponseDataRate =
                                new MinDataRate(bytesPerSecond: 240, gracePeriod: TimeSpan.FromSeconds(5)); //响应正文最小数据速率
                            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30); //请求标头超时
                            //options.AllowSynchronousIO = true; //是否允许对请求和响应使用同步 IO
                        })
                        .ConfigureLogging(logging => logging.AddConsole())
                        .UseUrls(WebHostConfig.Urls) //不部署到IIS环境时请改写为自己系统的端口
                        .UseIISIntegration() //当部署到IIS环境时可以自动搭接ANCM和IIS
                        .UseStartup<Startup>();
                });
        }
    }
}