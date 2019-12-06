using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Phenix.Services.Host
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => { Phenix.Core.Log.EventLog.SaveLocal("An unhandled exception occurred in the current domain", (Exception) eventArgs.ExceptionObject); };

            /*
             * 启动WebAPI服务
             * 启动Orleans服务集群
             *
             * Phenix.Core.Data.Database的配置方法，可通过Phenix.Core.db的PH7_Database表，也可以通过类似下面的代码进行注册（缺省数据库Phenix.Core.Data.Database.Default的连接串，相当于PH7_Database表DataSourceKey字段值为'*'的记录）：
             * Phenix.Core.Data.Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
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
                 */
                .UseOrleans((context, builder) => builder
                    /*
                     * 设置集群ID：Phenix.Core.Data.Database.Default.DataSourceKey
                     * 设置服务ID：Phenix.Core.Data.Database.Default.DataSourceKey
                     * 设置Clustering、GrainStorage、Reminder数据库：Phenix.Core.Data.Database.Default
                     *
                     * 装配Actor核心，包含：
                     *   Phenix.Core.Data.Grains.SequenceGrain 响应 Phenix.Core.Data.Sequence 请求
                     *   Phenix.Core.Data.Grains.IncrementGrain 响应 Phenix.Core.Data.Increment 请求
                     *   Phenix.Core.Security.Grains.UserGrain 响应 Phenix.Core.Security.User 请求
                     *
                     * 装配Actor插件
                     * 系统的 Actor 都应该按照领域划分开发各自的插件程序集，部署到本服务容器的执行目录下
                     * 插件程序集的命名，都应该统一采用"*.Plugin.dll"作为文件名的后缀
                     */
                    .ConfigureCluster()
                    /*
                     * 设置Silo端口：EndpointOptions.DEFAULT_SILO_PORT
                     * 设置Gateway端口：EndpointOptions.DEFAULT_SILO_PORT
                     */
                    .ConfigureEndpoints(EndpointOptions.DEFAULT_SILO_PORT, EndpointOptions.DEFAULT_GATEWAY_PORT))
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
                        .UseUrls("http://*:5000") //不部署到IIS环境时请改写为自己系统的端口
                        .UseIISIntegration() //当部署到IIS环境时可以自动搭接ANCM和IIS
                        .UseStartup<Startup>();
                });
        }
    }
}