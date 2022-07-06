using System;
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
using Phenix.Core.Plugin;
using Phenix.Core.Security;
using Phenix.Mapper.Schema;
using Phenix.Services.Library.Security;

namespace Phenix.Services.Host
{
    public static class Program
    {
        #region 方法

        public static void Main(string[] args)
        {
            Principal.FetchIdentity = Identity.Fetch;

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("zh-CN", true)
            {
                DateTimeFormat = { ShortDatePattern = "yyyy-MM-dd", FullDateTimePattern = "yyyy-MM-dd HH:mm:ss", LongTimePattern = "HH:mm:ss" } //兼容Linux（CentOS）环境
            };

#if DEBUG
            AppRun.Debugging = true;
            Console.WriteLine("调试状态（Phenix.Core.AppRun.Debugging)为: {0}（正式环境下请关闭）", AppRun.Debugging);
#endif

            try
            {
                Console.WriteLine("正在从缺省数据库加载数据字典到本地以便加快服务的响应速度...");
                Console.WriteLine("缺省数据库（Phenix.Core.Data.Database.Default.DataSource）为：{0}", Database.Default.DataSource);
                MetaData.Fetch().FillingCache();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("请检查当前目录下配置库（Phenix.Core.db文件）中数据库连接串(DataSourceKey：{0})是否正确！", Database.Default.DataSourceKey);
                Console.ReadLine();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => { Phenix.Core.Log.EventLog.SaveLocal("An unhandled exception occurred in the current domain", (Exception)eventArgs.ExceptionObject); };
            RunHost(args);
        }

        private static void RunHost(string[] args)
        {
            Console.WriteLine("构建Orleans和WebAPI的服务...");
            using (IHost host = CreateHostBuilder(args).Build())
            {
                Console.WriteLine("启动Orleans和WebAPI的服务...");
                host.Start();

                Console.WriteLine("启动插件...");
                foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Plugin.dll"))
                    try
                    {
                        Console.WriteLine("加载插件程序集：{0}", fileName);
                        Assembly assembly = Assembly.LoadFrom(fileName);
                        PluginHost.Default.GetPlugin(assembly, (_, message) =>
                        {
                            Console.WriteLine(message);
                            return null;
                        }).Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("启动插件{0}失败：{1}", fileName, ex.Message);
                    }

                Console.WriteLine("启动插件完毕.");

                host.WaitForShutdown();
            }
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
                .UseOrleans((context, builder) =>
                {
                    builder.ConfigureLogging(logging => logging.AddConsole());
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