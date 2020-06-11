using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

namespace Phenix.Services.Host
{
    public static class Program
    {
        #region 属性

        private static readonly object _lock = new object();
        private static IHost _host;
        private static bool _hostStopping;
        private static readonly ManualResetEvent _hostStopped = new ManualResetEvent(false);

        #endregion

        #region 方法

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Phenix.Core.Log.EventLog.SaveLocal("An unhandled exception occurred in the current domain", (Exception) eventArgs.ExceptionObject);
            };
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                if (!_hostStopping)
                    lock (_lock)
                        if (!_hostStopping)
                        {
                            _hostStopping = true;
                            Task.Run(() =>
                            {
                                _host.StopAsync();
                                _hostStopped.Set();
                            }).Ignore();
                        }
            };

            /*
             * 注册用户资料代理工厂，将身份验证等功能模块部署到Orleans服务集群
             */
            Phenix.Core.Security.Identity.RegisterFactory(new Phenix.Services.Plugin.UserProxyFactory());

            /*
             * 构建Host并启动服务
             * 如第一次启动，可在wwwroot\test目录里打开测试网页，验证服务环境是否正常
             */
            _host = CreateHostBuilder(args).Build();
            _host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.AddConsole())
                .UseContentRoot(Phenix.Core.AppRun.BaseDirectory)
                /*
                 * 启动Orleans服务集群
                 * 请事先在数据库中手工添加Orleans配置库，默认是Phenix.Core.Data.Database.Default指向的数据库
                 * Orleans配置库的脚本文件，见Orleans Database Script目录，分为PostgreSQL、MySQL、Oracle三组，建议按需顺序批处理执行
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
                    .ConfigureCluster(OrleansConfig.ClusterId, OrleansConfig.ServiceId, OrleansConfig.ConnectionString)
                    /*
                     * 设置Silo端口：EndpointOptions.DEFAULT_SILO_PORT
                     * 设置Gateway端口：EndpointOptions.DEFAULT_SILO_PORT
                     */
                    .ConfigureEndpoints(OrleansConfig.DefaultSiloPort, OrleansConfig.DefaultGatewayPort)
                    /*
                     * 使用Dashboard插件
                     * 本地打开可视化监控工具：http://localhost:8080/
                     * 建议仅向内网开放
                     */
                    .UseDashboard(options =>
                    {
                        options.Username = OrleansConfig.DashboardUsername; //设置用于访问Dashboard的用户名（基本身份验证）
                        options.Password = OrleansConfig.DashboardPassword; //设置用于访问Dashboard的用户口令（基本身份验证）
                        options.Host = OrleansConfig.DashboardHost; //将Web服务器绑定到的主机名（默认为*）
                        options.Port = OrleansConfig.DashboardPort; //设置Dashboard可视化页面访问的端口（默认为8088）
                        options.HostSelf = OrleansConfig.DashboardHostSelf; //将Dashboard设置为托管自己的http服务器（默认为true）
                        options.CounterUpdateIntervalMs = OrleansConfig.DashboardCounterUpdateIntervalMs; //采样计数器之间的更新间隔（以毫秒为单位，默认为1000）
                    }))
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

        #endregion
    }
}