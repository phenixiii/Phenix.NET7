using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Phenix.WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
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
                .UseUrls("http://*:5000");
    }
}
