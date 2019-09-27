using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Phenix.Services.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => { Phenix.Core.Log.EventLog.SaveLocal("An unhandled exception occurred in the current domain", (Exception) eventArgs.ExceptionObject); };
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Phenix.Core.AppRun.BaseDirectory)
                .UseStartup<Startup>()
                /*
                 * 使用轻量级跨平台服务 KestrelServer
                 * 请根据自己系统的运行要求配置 KestrelServer 选项
                 */
                .UseKestrel(options =>
                {
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2); //保持活动状态超时
                    options.Limits.MaxConcurrentConnections = 1000; //客户端最大连接数
                    options.Limits.MaxConcurrentUpgradedConnections = 1000; //客户端最大连接数（其他协议如Websocket）
                    options.Limits.MaxRequestBodySize = Int64.MaxValue; //请求正文最大大小
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 240, gracePeriod: TimeSpan.FromSeconds(5)); //请求正文最小数据速率
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 240, gracePeriod: TimeSpan.FromSeconds(5)); //响应正文最小数据速率
                    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30); //请求标头超时
                    options.AllowSynchronousIO = true; //是否允许对请求和响应使用同步 IO
                })
                /*
                 * 请改写为自己系统的端口
                 */
                .UseUrls("http://*:5000");
    }
}