using System;
using System.Windows.Forms;
using Phenix.Core;
using Phenix.Core.Reflection;
using Phenix.Core.Security;

namespace Demo
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Net.Http.HttpClient 的 UploadFile()、DownloadFile() 功能 ****");
            Console.WriteLine();
            Console.WriteLine("HttpClient 类，封装 System.Net.Http.HttpClient，提供了客户端程序向 WebAPI/SignalR 服务发起请求的通用入口。");
            Console.WriteLine("本 Demo 演示的是其上传和下载文件的功能。");
            Console.WriteLine();

            Console.WriteLine("在接下来的演示之前，请启动 Phenix.Services.Host_MySQL/ORA 程序。");
            Console.WriteLine("如需观察 Phenix.Services（扩展服务）被唤起的代码执行效果，可在其 FileService、GateService 的函数里设置断点附加到执行中的 Phenix.Services.Host_MySQL/ORA 程序。");
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

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = false;
                openFileDialog.Title = "请选择文件（上传后会被下载覆盖）";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine("开始上传: " + openFileDialog.FileName);
                    string message = httpClient.UploadFile("Hello uploadFile!", openFileDialog.FileName, fileChunkInfo =>
                    {
                        Console.WriteLine("上传{0}进度：{1}/{2}", fileChunkInfo.FileName, fileChunkInfo.ChunkNumber, fileChunkInfo.ChunkCount);
                        return true; //继续上传
                    });
                    Console.WriteLine("完成上传: " + message);
                    Console.Write("请按任意键继续");
                    Console.ReadKey();
                    Console.WriteLine();
                    Console.WriteLine();

                    Console.WriteLine("开始下载刚上传文件...");
                    httpClient.DownloadFile("Hello downloadFile!", openFileDialog.FileName, fileChunkInfo =>
                    {
                        Console.WriteLine("下载{0}进度：{1}/{2}", fileChunkInfo.FileName, fileChunkInfo.ChunkNumber, fileChunkInfo.ChunkCount);
                        return true; //继续下载
                    });
                    Console.WriteLine("完成下载: " + openFileDialog.FileName);
                }
            }
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
