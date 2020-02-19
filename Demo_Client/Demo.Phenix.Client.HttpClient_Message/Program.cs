using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Client.HttpClient 的 SubscribeMessage()、SendMessageAsync()、AffirmReceivedMessageAsync()功能 ****");
            Console.WriteLine();
            Console.WriteLine("程序集 Phenix.Client 是 Phenix.Services.Host 的客户端类库。");
            Console.WriteLine("本 Demo 演示了通过 Phenix.Client.HttpClient 的 XXXMessage 系列函数实现登录用户之间简单的消息收发机制。");
            Console.WriteLine();

            Console.WriteLine("在接下来的演示之前，请启动 Phenix.Services.Host 程序，并保证其正确连接到你的测试库。");
            Console.WriteLine("Phenix.Services.Host 程序的数据库连接配置信息，存放在其所在目录 SQLite 库 Phenix.Core.db 文件的 PH7_Database 表中，配置方法见其示例记录的 Remark 字段内容。");
            Console.WriteLine("如需观察 Phenix.Services（扩展服务）被唤起的代码执行效果，可在其 GateService 的函数里设置断点，将程序集附加到执行中的 Phenix.Services.Host 程序。");
            Console.Write("准备好之后，请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Phenix.Client.HttpClient httpClient = Phenix.Client.HttpClient.New(new Uri("http://localhost.:5000"));
            Console.WriteLine("构造一个 Phenix.Client.HttpClient 对象用于访问‘{0}’服务端。", httpClient.BaseAddress);
            string userName = "测试用" + Guid.NewGuid().ToString();
            Console.WriteLine("登记/注册用户：{0}", userName);
            Console.WriteLine(httpClient.CheckInAsync(userName).Result);
            while (true)
                try
                {
                    Console.Write("请依照以上提示，输入找到的动态口令/登录口令，完成后按回车确认：");
                    string password = Console.ReadLine() ?? String.Empty;
                    Phenix.Client.Security.Identity identity = httpClient.LogonAsync(userName, password.Trim()).Result;
                    Console.WriteLine("登录成功：{0}", identity.IsAuthenticated ? "ok" : "error");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("登录失败，需重试：{0}", Phenix.Core.AppRun.GetErrorMessage(ex));
                    Console.WriteLine();
                }

            Console.WriteLine("Phenix.Client.HttpClient.Default 缺省为第一个调用LogonAsync成功的HttpClient对象：{0}", Phenix.Client.HttpClient.Default == httpClient ? "ok" : "error");
            Console.WriteLine("当前用户身份：{0} {1}", Phenix.Core.Reflection.Utilities.JsonSerialize(Phenix.Client.Security.Identity.CurrentIdentity), Phenix.Client.Security.Identity.CurrentIdentity == httpClient.Identity ? "ok" : "error");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("启动消息的订阅...");
            int i = 0;
            string prevMessage = String.Empty;
            long messageId = httpClient.GetSequenceAsync().Result;
            HubConnection connection = httpClient.SubscribeMessage(messages =>
            {
                foreach (KeyValuePair<long, string> kvp in messages)
                {
                    Console.WriteLine("收到消息：{0} — '{1}'", kvp.Key, kvp.Value);
                    httpClient.AffirmReceivedMessageAsync(kvp.Key, i == 0).Wait();
                    Console.WriteLine("确认收到消息：{0}({1})", kvp.Key, i == 0 ? "阅后即焚" : "不阅后即焚");
                }

                Console.Write("如果希望再来一遍，请输入需要发送的消息，否则请直接按回车键结束演示：");
                string message = Console.ReadLine();
                if (String.IsNullOrEmpty(message))
                    Environment.Exit(0);
                if (String.CompareOrdinal(prevMessage, message) == 0)
                {
                    i = i + 1;
                    message = String.Format("{0}[同一ID({1})第{2}次]", message, messageId, i);
                    httpClient.SendMessageAsync(messageId, Phenix.Client.Security.Identity.CurrentIdentity.User.Name, message).Wait();
                    Console.WriteLine("向自己发送刷新消息：{0}", message);
                }
                else
                {
                    i = 0;
                    prevMessage = message;
                    httpClient.SendMessageAsync(Phenix.Client.Security.Identity.CurrentIdentity.User.Name, message).Wait();
                    Console.WriteLine("向自己发送一条消息：{0}", message);
                }
            });
            connection.Reconnecting += delegate(Exception error)
            {
                Console.WriteLine("重新订阅中：{0} — {1}", Phenix.Core.AppRun.GetErrorMessage(error), connection.State);
                return Task.CompletedTask;
            };
            connection.Reconnected += delegate(string connectionId)
            {
                Console.WriteLine("重新订阅好：{0} — {1}", connectionId, connection.State);
                return Task.CompletedTask;
            };
            connection.Closed += delegate(Exception error)
            {
                Console.WriteLine("订阅关闭：{0} — {1}", Phenix.Core.AppRun.GetErrorMessage(error), connection.State);
                return Task.CompletedTask;
            };
            connection.StartAsync().Wait();
            Console.WriteLine("订阅成功：{0}", connection.State);
            httpClient.SendMessageAsync(Phenix.Client.Security.Identity.CurrentIdentity.User.Name, "第一条消息!").Wait();
            Console.WriteLine("向自己首发一条消息");

            while (true) Thread.Sleep(100);
        }
    }
}