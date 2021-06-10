﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Phenix.Core;
using Phenix.Core.Data;

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
            
            Console.WriteLine("设为调试状态（正式环境下请注释掉）");
            AppRun.Debugging = true;

            Console.WriteLine("填充缺省数据库元数据缓存");
            Database.Default.MetaData.FillingCache();

            Console.WriteLine("注册用户资料管理代理工厂");
            Phenix.Core.Security.Identity.RegisterFactory(new Phenix.Services.Plugin.UserProxyFactory());

            Console.WriteLine("构建Host并启动Orleans服务和WebAPI服务");
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
                 * Orleans配置库的脚本文件，见Orleans Database Script目录，分为PostgreSQL、MySQL、Oracle、SQLServer四组，建议按需顺序批处理执行
                 */
                .UseOrleans((context, builder) => builder
                    .ConfigureLogging(logging => logging.AddConsole())
                    /*
                     * 配置Orleans服务集群
                     * 配置项见Phenix.Actor.OrleansConfig
                     * 设置集群ID、服务ID：Phenix.Core.Data.Database.Default.DataSourceKey
                     * 设置默认的激活体垃圾收集年龄限为2天
                     * 设置Clustering、GrainStorage、Reminder数据库：Phenix.Core.Data.Database.Default
                     * 设置Silo端口：EndpointOptions.DEFAULT_SILO_PORT
                     * 设置Gateway端口：EndpointOptions.DEFAULT_GATEWAY_PORT
                     * 设置SimpleMessageStreamProvider：Phenix.Actor.StreamProviderExtension.StreamProviderName
                     *
                     * 装配Actor插件
                     * 插件程序集都应该统一采用"*.Plugin.dll"、"*.Contract.dll"作为文件名的后缀
                     * 插件程序集都应该被部署到本服务容器的执行目录下
                     */
                    .ConfigureCluster(Database.Default)
                    /*
                     * 使用Dashboard插件
                     * 本地打开可视化监控工具：http://localhost:8088/
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
                .ConfigureWebHostDefaults(builder => builder
                    .ConfigureLogging(logging => logging.AddConsole())
                    /*
                     * 使用轻量级跨平台服务 KestrelServer
                     * 请根据自己系统的运行要求配置 KestrelServer 选项
                     */
                    .UseKestrel(options =>
                    {
                        options.Limits.MaxConcurrentConnections = 1000 * Environment.ProcessorCount; //客户端最大连接数
                        options.Limits.MaxConcurrentUpgradedConnections = 1000 * Environment.ProcessorCount; //客户端最大连接数（其他协议如Websocket）
                        options.Limits.MaxRequestBodySize = Int32.MaxValue; //请求正文最大大小
                        options.Limits.MaxRequestBufferSize = Int32.MaxValue; //请求缓存最大大小
                        options.Limits.MaxResponseBufferSize = Int32.MaxValue; //响应缓存最大大小
                        //options.Limits.MinRequestBodyDataRate =
                        //    new MinDataRate(bytesPerSecond: 240, gracePeriod: TimeSpan.FromSeconds(5)); //请求正文最小数据速率
                        //options.Limits.MinResponseDataRate =
                        //    new MinDataRate(bytesPerSecond: 240, gracePeriod: TimeSpan.FromSeconds(5)); //响应正文最小数据速率
                        //options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30); //请求标头超时
                        //options.AllowSynchronousIO = true; //是否允许对请求和响应使用同步 IO
                        //options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2); //保持活动状态超时
                    })
                    .UseUrls(WebHostConfig.Urls) //不部署到IIS环境时请改写为自己系统的端口
                    .UseIISIntegration() //当部署到IIS环境时可以自动搭接ANCM和IIS
                    .UseStartup<Startup>()
                );
        }

        #endregion
    }
}