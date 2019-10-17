using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Log;
using Phenix.Core.Net.Http;
using Phenix.Core.Reflection;
using Phenix.Core.Security;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Net.Http.OfflineCache 功能 ****");
            Console.WriteLine();
            Console.WriteLine("OfflineCache 类，为系统的客户端提供脱机缓存实时上传报文的功能。");
            Console.WriteLine("系统的服务端如果需要向其他指定的服务端发送报文，也是可以使用本功能的，此时统称它为脱机缓存实时上传的客户端。");
            Console.WriteLine("脱机缓存，是指 OfflineCache 将待上传的报文暂时存放在客户端的当前目录下 Phenix.Core.db 文件里（SQLite库）。");
            Console.WriteLine("实时上传，是指 OfflineCache 会自动将暂存的报文发送到指定URL的服务端，上传失败的话还会尝试上传，直到成功为止。");
            Console.WriteLine();

            Console.WriteLine("本演示借用了 Phenix.Core.Log.EventLog 类。");
            Console.WriteLine("EventLog 在保存日志时，如果所在进程里未曾注册过任何数据库连接，就会用 OfflineCache 尝试上传给到其属性 BaseAddress 所指定的服务端。");
            Console.WriteLine("服务端应该提供‘{0}’路径的控制器，比如 Phenix.Services.Host 工程里就加载了 Phenix.Core.dll 里的 EventLogController 控制器。", Phenix.Core.Net.NetConfig.ApiLogEventLogPath);
            Console.WriteLine("在接下来的演示之前，请启动 Phenix.Services.Host_MySQL/ORA 程序。");
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

            Console.WriteLine("EventLog 在保存日志时，会首先查看其 Database 属性（默认是 Phenix.Core.Data.Database.Default 值）是否有数据库连接。");
            Console.WriteLine("因为本程序模拟的是客户端，不会直连数据库的，EventLog.Database 属性肯定为空，EventLog 只会将日志给到 OfflineCache 暂存并尝试上传到服务端。");
            Console.WriteLine("OfflineCache 将日志暂存在 {0} 文件里（SQLite库）。", OfflineCache.FilePath);
            while (true)
            {
                if (File.Exists(OfflineCache.FilePath))
                    break;
                Console.Write("{0} 目录下未发现 {1} 文件，请从 Bin_ORA 或 Bin_MySQL 目录里拷贝进同名文件，完成后按任意键继续", AppRun.BaseDirectory, OfflineCache.FilePath);
                System.Diagnostics.Process.Start("explorer.exe", AppRun.BaseDirectory);
                Console.ReadKey();
                Console.WriteLine();
            }
            Console.WriteLine();

            string message = "我是一条跨域传递的日志";
            Console.WriteLine("先准备一条演示用的日志 = '{0}'", message);
            OfflineCache.UploadSuspending = true;
            Console.WriteLine("为演示需要，暂停 OfflineCache 的上传线程：{0}", OfflineCache.UploadSuspending ? "ok" : "error");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            EventLog.Save(message);
            Console.WriteLine("调用 EventLog.Save() 函数后，日志被保存在了 {0} 的 PH7_OfflineCache 表里：", OfflineCache.FilePath);
            ShowFirstCache();
            Console.WriteLine("请注意 OC_BaseAddress 字段是空的，因为 EventLog.UploadBaseAddress 属性未曾赋值过：{0}", EventLog.UploadBaseAddress ?? "null");
            Console.WriteLine("对于 OC_BaseAddress 字段为空值的记录，OfflineCache 上传报文时会尝试向 HttpClient.Default 指向的服务发起请求，地址为：{0}", Phenix.Core.Net.Http.HttpClient.Default.BaseAddress);
            Console.WriteLine("暂存的报文是有有效期的，见 OC_ValidityTime 字段，你可通过设置 EventLog.UploadValidityMinutes 属性（默认值为 {0} 分钟以内）进行控制。", EventLog.UploadValidityMinutes);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            OfflineCache.UploadSuspending = false;
            Console.WriteLine("恢复 OfflineCache 的上传线程：{0}", !OfflineCache.UploadSuspending ? "ok" : "error");
            do
            {
                Thread.Sleep(3000);
                Console.WriteLine("等待 OfflineCache 上传报文...");
                Console.WriteLine("OfflineCache 正在上传的报文数量：{0}", OfflineCache.UploadingCount);
            } while (OfflineCache.UploadingCount > 0);
            Console.WriteLine("OfflineCache 上传报文成功。");
            ShowFirstCache();
            Console.WriteLine("缓存的报文，一旦上传成功，就会被自动删除。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("启动 3 个线程分别保存 10 条日志...");
            Task[] tasks = new[]
            {
                Task.Run(() => SaveEventLog(1)),
                Task.Run(() => SaveEventLog(2)),
                Task.Run(() => SaveEventLog(3)),
            };
            Task.WaitAll(tasks);
            Console.WriteLine("线程运行结束。");
            Console.WriteLine("请到 Phenix.Services.Host_MySQL/ORA 所连接的数据库里，查看 PH7_EventLog 表里已上传保存的报文。");
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }

        static void SaveEventLog(int index)
        {
            for (int i = 0; i < 10; i++)
            {
                EventLog.Save(String.Format("{0}-{1}:{2}", index, i, Sequence.Value));
                Console.WriteLine("保存日志：{0}-{1}:{2}", index, i, Sequence.Value);
            }
        }

        private static void ShowFirstCache()
        {
            using (SQLiteConnection connection = new SQLiteConnection(OfflineCache.ConnectionString))
            using (SQLiteCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
select *
from PH7_OfflineCache
order by OC_ID desc";
                using (SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            Console.Write("{0} = {1}, ", reader.GetName(i), reader.GetValue(i) ?? "null");
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
