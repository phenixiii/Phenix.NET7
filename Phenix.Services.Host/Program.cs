using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace Phenix.Services.Host
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            /*
             * 如果未在Phenix.Core.db库文件PH7_Database表里配置数据库连接串的话请在此注册缺省数据库
             */
            //Phenix.Core.Data.Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => { Phenix.Core.Log.EventLog.SaveLocal("An unhandled exception occurred in the current domain", (Exception) eventArgs.ExceptionObject); };
            
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseContentRoot(Phenix.Core.AppRun.BaseDirectory)
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
                        .UseUrls("http://*:5000") //不部署到IIS环境时请改写为自己系统的端口
                        .UseIISIntegration() //当部署到IIS环境时可以自动搭接ANCM和IIS
                        .UseStartup<Startup>();
                });
        }
    }
}
