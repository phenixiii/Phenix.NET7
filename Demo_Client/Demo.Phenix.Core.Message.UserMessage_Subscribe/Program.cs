using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Phenix.Core;
using Phenix.Core.Message;
using Phenix.Core.Reflection;
using Phenix.Core.Security;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Message.UserMessage 的 Subscribe() 功能 ****");
            Console.WriteLine();
            Console.WriteLine("UserMessage 类，可以在服务端使用也可以在客户端使用，为系统提供针对用户之间收发消息的功能。");
            Console.WriteLine("在服务端使用，如果注册过数据库连接（见 Database 属性），消息就缓存在了 PH7_UserMessage 表里，否则会尝试向其 HttpClient 属性所指向的 WebAPI/SignalR 服务发起请求，一层层调用直到最后一层服务是直连数据库的可以缓存消息为止。");
            Console.WriteLine("HttpClient 属性，默认为 Phenix.Core.Net.Http.HttpClient.Default（即第一个被构造的 HttpClient），除非直接赋值指定，或者调用函数时传参指定。");
            Console.WriteLine("本 Demo 演示了显式调用带 httpClient 参数的函数的方法，直接向这个 httpClient 参数所指向的 WebAPI/SignalR 服务发起请求。");
            Console.WriteLine();

            Console.WriteLine("在接下来的演示之前，请启动 Phenix.Services.Host_MySQL/ORA 程序。");
            Console.WriteLine("如需观察 Phenix.Services（扩展服务）被唤起的代码执行效果，可在其 GateService 的函数里设置断点附加到执行中的 Phenix.Services.Host_MySQL/ORA 程序。");
            Console.Write("准备好之后，请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            string userName = "测试用";
            Console.WriteLine("以‘{0}’为名登录系统。", userName);
            Phenix.Core.Net.Http.HttpClient httpClient = Phenix.Core.Net.Http.HttpClient.New(new Uri("http://localhost.:5000"));
            Console.WriteLine("构造一个 Phenix.Core.Net.Http.HttpClient 对象用于访问‘{0}’服务端。", httpClient.BaseAddress);
            Console.WriteLine("HttpClient.Default 缺省为构造过的第一个HttpClient对象：{0}", Phenix.Core.Net.Http.HttpClient.Default == httpClient ? "ok" : "error");
            while (true)
                try
                {
                    Console.WriteLine("取到‘{0}’用户的动态口令：{1}。", userName, httpClient.CheckIn(userName));
                    Console.Write("请依照以上提示，找到动态口令并在此输入（如果是第一次CheckIn，给到的登录口令也是可以用的，输入完成后按回车确认）：");
                    string dynamicPassword = Console.ReadLine() ?? String.Empty;
                    httpClient.Logon(userName, dynamicPassword.Trim());
                    Console.WriteLine("登录成功：{0}", Identity.CurrentIdentity.IsAuthenticated ? "ok" : "error");
                    Console.WriteLine("当前用户身份：{0}", Utilities.JsonSerialize(Identity.CurrentIdentity));
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("登录失败，需重试：{0}", AppRun.GetErrorMessage(ex));
                    Console.WriteLine();
                }
            Console.WriteLine("如果客户端是自动运行的程序，需要CheckIn一个固定用户，将CheckIn时获得的登录口令写死在程序里（或其他手段保存和取用）。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("启动消息的订阅...");
            HubConnection connection = UserMessage.Subscribe(httpClient, delegate(IDictionary<long, string> messages) {
                foreach (KeyValuePair<long, string> kvp in messages)
                {
                    Console.WriteLine("收到消息：{0} — '{1}'", kvp.Key, kvp.Value);
                    UserMessage.AffirmReceived(httpClient, kvp.Key, true);
                    Console.WriteLine("确认收到消息：{0} — 阅后即焚", kvp.Key);
                }
                Console.Write("如果希望再来一遍，请输入需要发送的消息，否则请直接按回车键结束演示：");
                string message = Console.ReadLine();
                if (String.IsNullOrEmpty(message))
                    Environment.Exit(0);
                UserMessage.Send(httpClient, Identity.CurrentIdentity.User.Name, message);
                Console.WriteLine("向自己发送一条消息：{0}", message);
            });
            connection.Reconnecting += delegate (Exception error)
            {
                Console.WriteLine("重新订阅中：{0} — {1}", AppRun.GetErrorMessage(error), connection.State);
                return Task.CompletedTask;
            };
            connection.Reconnected += delegate (string connectionId)
            {
                Console.WriteLine("重新订阅好：{0} — {1}", connectionId, connection.State);
                return Task.CompletedTask;
            };
            connection.Closed += delegate (Exception error)
            {
                Console.WriteLine("订阅关闭：{0} — {1}", AppRun.GetErrorMessage(error), connection.State);
                return Task.CompletedTask;
            };
            connection.StartAsync().Wait();
            Console.WriteLine("订阅成功：{0}", connection.State);
            UserMessage.Send(httpClient, Identity.CurrentIdentity.User.Name, "第一条消息!");
            Console.WriteLine("向自己首发一条消息");

            while (true) Thread.Sleep( 100);
        }
    }
}
