﻿using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Statistics;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Log;
using Phenix.Core.Plugin;
using Phenix.Mapper.Schema;
using Serilog;
using Serilog.Events;

namespace Phenix.Services.Host
{
    public static class Program
    {
        #region 方法

        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("zh-CN", true)
            {
                DateTimeFormat = { ShortDatePattern = "yyyy-MM-dd", FullDateTimePattern = "yyyy-MM-dd HH:mm:ss", LongTimePattern = "HH:mm:ss" } //兼容Linux（CentOS）环境
            };

#if DEBUG
            AppRun.Debugging = true;
#endif

            Log.Logger = LogHelper.InitializeConfiguration()
                .WriteTo.Exceptionless(
                    restrictedToMinimumLevel: AppRun.Debugging ? LogEventLevel.Debug : LogEventLevel.Information, //捕获的最小日志级别
                    includeProperties: true, //包含Serilog属性
                    serverUrl: ExceptionlessConfig.ServerUrl,
                    apiKey: ExceptionlessConfig.ApiKey)
                .CreateLogger();

            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => LogHelper.Error((Exception)eventArgs.ExceptionObject, "An unhandled exception occurred in the current domain");

                LogHelper.Warning("Phenix.Core.AppRun.Debugging = {Debugging}", AppRun.Debugging);
#if !DEBUG
                if (AppRun.Debugging)
                    LogHelper.Warning("Please set this Phenix.Core.AppRun.Debugging parameter to false in the production environment");
#endif

                LogHelper.Warning("Phenix.Core.Data.Database.Default.DataSource = {@DataSource}", Database.Default.DataSource);
                MetaData.Fetch().FillingCache();

                using (IHost host = CreateHostBuilder(args).Build())
                {
                    host.Start();
                    LogHelper.Warning("Starting host");

                    foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Plugin.dll"))
                        try
                        {
                            Assembly assembly = Assembly.LoadFrom(fileName);
                            PluginHost.Default.GetPlugin(assembly, (_, message) =>
                            {
                                LogHelper.Information("{Message}", message);
                                return null;
                            }).Start();
                            LogHelper.Information("Loading plugin {@FileName}", fileName);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Warning("Loading plugin {@FileName}: {Message}", fileName, ex.Message);
                        }

                    LogHelper.Warning("Running host");
                    host.WaitForShutdown();
                    LogHelper.Warning("Exit host");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseSerilog(dispose: true)
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseContentRoot(Phenix.Core.AppRun.BaseDirectory)
                /*
                 * 启动Orleans服务集群
                 * 请事先在数据库中手工添加Orleans配置库，默认是Phenix.Core.Data.Database.Default指向的数据库
                 * Orleans配置库的脚本文件，见Orleans Database Script目录，分为PostgreSQL、MySQL、Oracle、SQLServer四组，建议按需顺序批处理执行
                 */
                .UseOrleans((context, builder) =>
                {
                    /*
                     * 配置Orleans服务集群
                     */
                    builder.ConfigureCluster(Database.Default);
                    /*
                     * 使用Dashboard插件
                     * 本地打开可视化监控工具：http://localhost:8088/
                     * 建议仅向内网开放
                     */
                    builder.UseDashboard(options =>
                    {
                        options.Username = DashboardConfig.Username; //设置用于访问Dashboard的用户名（基本身份验证）
                        options.Password = DashboardConfig.Password; //设置用于访问Dashboard的用户口令（基本身份验证）
                        options.Host = DashboardConfig.Host; //将Web服务器绑定到的主机名（默认为*）
                        options.Port = DashboardConfig.Port; //设置Dashboard可视化页面访问的端口（默认为8088）
                        options.HostSelf = DashboardConfig.HostSelf; //将Dashboard设置为托管自己的http服务器（默认为true）
                        options.CounterUpdateIntervalMs = DashboardConfig.CounterUpdateIntervalMs; //采样计数器之间的更新间隔（以毫秒为单位，默认为1000）
                    });
                    /*
                     * 为Dashboard提供操作系统平台下的性能计数器服务
                     */
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        builder.UsePerfCounterEnvironmentStatistics();
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        builder.UseLinuxEnvironmentStatistics();
                })
                /*
                 * 启动WebAPI服务
                 */
                .ConfigureWebHostDefaults(builder => builder
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