using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Phenix.Core.Threading;

namespace Demo
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Client.HttpClient 的 UploadFile()、DownloadFile() 功能 ****");
            Console.WriteLine();
            Console.WriteLine("程序集 Phenix.Client 是 Phenix.Services.Host 的客户端类库。");
            Console.WriteLine("本 Demo 演示了通过 Phenix.Client.HttpClient 的 XXXFile 系列函数实现文件的上传和下载功能。");
            Console.WriteLine();

            Console.WriteLine("在接下来的演示之前，请启动 Phenix.Services.Host 程序，并保证其正确连接到你的测试库。");
            Console.WriteLine("数据库连接配置信息，存放在 Phenix.Services.Host 程序所在目录 SQLite 库 Phenix.Core.db 文件的 PH7_Database 表中，配置方法见其示例记录的 Remark 字段内容。");
            Console.WriteLine("当上传下载文件时，Host 会唤起 Phenix.Services（扩展服务）的 FileService 相关函数，你可以将程序集附加到执行中的 Phenix.Services.Host 程序， 设置断点观察调用过程。");
            Console.Write("准备好之后，请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Phenix.Client.HttpClient httpClient = Phenix.Client.HttpClient.New(new Uri("http://localhost.:5000"));
            Console.WriteLine("构造一个 Phenix.Client.HttpClient 对象用于访问‘{0}’服务端。", httpClient.BaseAddress);
            string userName = "测试用" + Guid.NewGuid().ToString();
            Console.WriteLine("登记/注册用户：{0}", userName);
            Console.WriteLine(AsyncHelper.RunSync(() => httpClient.CheckInAsync(userName)));
            while (true)
                try
                {
                    Console.Write("请依照以上提示，输入找到的动态口令/登录口令，完成后按回车确认：");
                    string password = Console.ReadLine() ?? String.Empty;
                    Phenix.Client.Security.Identity identity = AsyncHelper.RunSync(() => httpClient.LogonAsync(userName, password.Trim()));
                    Console.WriteLine("登录成功：{0}", identity.IsAuthenticated ? "ok" : "error");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("登录失败，需重试：{0}", Phenix.Core.AppRun.GetErrorMessage(ex));
                    Console.WriteLine();
                }

            Console.WriteLine("Phenix.Client.HttpClient.Default 缺省为第一个调用 LogonAsync 成功的 HttpClient 对象：{0}", Phenix.Client.HttpClient.Default == httpClient ? "ok" : "error");
            Console.WriteLine("Phenix.Client.HttpClient.Default.Identity 缺省为 Phenix.Client.Security.Identity.CurrentIdentity 对象：{0}", Phenix.Client.HttpClient.Default.Identity == Phenix.Client.Security.Identity.CurrentIdentity ? "ok" : "error");
            Console.WriteLine("当前用户资料：{0}", Phenix.Core.Reflection.Utilities.JsonSerialize(Phenix.Client.Security.Identity.CurrentIdentity.User));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = false;
                openFileDialog.Title = "请选择文件（上传后会被下载文件覆盖）";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine("开始上传: " + openFileDialog.FileName);
                    string message = AsyncHelper.RunSync(() => Phenix.Client.HttpClient.Default.UploadFileAsync("Hello uploadFile!", openFileDialog.FileName, fileChunkInfo =>
                    {
                        Console.WriteLine("上传{0}进度：{1}/{2}", fileChunkInfo.FileName, fileChunkInfo.ChunkNumber, fileChunkInfo.ChunkCount);
                        return true; //继续上传
                    }));
                    Console.WriteLine("完成上传: " + message);
                    Console.Write("请按任意键继续");
                    Console.ReadKey();
                    Console.WriteLine();
                    Console.WriteLine();

                    Console.WriteLine("开始下载刚上传文件...");
                    AsyncHelper.RunSync(() => Phenix.Client.HttpClient.Default.DownloadFileAsync("Hello downloadFile!", openFileDialog.FileName, fileChunkInfo =>
                    {
                        Console.WriteLine("下载{0}进度：{1}/{2}", fileChunkInfo.FileName, fileChunkInfo.ChunkNumber, fileChunkInfo.ChunkCount);
                        return true; //继续下载
                    }));
                    Console.WriteLine("完成下载: " + openFileDialog.FileName);
                }
            }

            Console.WriteLine();
            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
