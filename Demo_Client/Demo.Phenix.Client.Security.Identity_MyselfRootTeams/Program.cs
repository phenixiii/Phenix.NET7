using System;
using System.Threading.Tasks;
using Phenix.Client.Security.Myself;

namespace Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("**** 演示 Demo.Phenix.Client.Security.Identity 的 Myself RootTeams 功能 ****");
            Console.WriteLine();
            Console.WriteLine("程序集 Phenix.Client 是 Phenix.Services.Host 的客户端类库。");
            Console.WriteLine("本 Demo 演示了用户通过 Phenix.Client.HttpClient 注册并登录成功后如何通过其 Identity 属性管理自己团队的组织架构。");
            Console.WriteLine();

            Console.WriteLine("在接下来的演示之前，请启动 Phenix.Services.Host 程序，并保证其正确连接到你的测试库。");
            Console.WriteLine("数据库连接配置信息，存放在 Phenix.Services.Host 程序所在目录 SQLite 库 Phenix.Core.db 文件的 PH7_Database 表中，配置方法见其示例记录的 Remark 字段内容。");
            Console.WriteLine("当注册用户、登录和操作组织架构时，Host 会唤起 Phenix.Services.Plugin（扩展插件）的 XXXProxy、XXXGrain 相关函数，你可以将程序集附加到执行中的 Phenix.Services.Host 程序， 设置断点观察调用过程。");
            Console.Write("准备好之后，请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();
            
            Phenix.Client.HttpClient httpClient = Phenix.Client.HttpClient.New(new Uri("http://localhost.:5000"));
            Console.WriteLine("构造一个 Phenix.Client.HttpClient 对象用于访问‘{0}’服务端。", httpClient.BaseAddress);
            string userName = "测试用" + Guid.NewGuid().ToString();
            Console.WriteLine("登记/注册用户：{0}", userName);
            Console.WriteLine(await httpClient.CheckInAsync(userName));
            while (true)
                try
                {
                    Console.Write("请依照以上提示，输入找到的动态口令/登录口令，完成后按回车确认：");
                    string password = Console.ReadLine() ?? String.Empty;
                    Phenix.Client.Security.Identity identity = await httpClient.LogonAsync(userName, password.Trim());
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

            Console.WriteLine("更新用户资料...");
            Phenix.Client.HttpClient.Default.Identity.User.Phone = "我的手机号";
            User user = await Phenix.Client.HttpClient.Default.Identity.ReFetchUserAsync();
            Console.WriteLine("服务端已更新用户资料：{0}", Phenix.Core.Reflection.Utilities.JsonSerialize(user));
            Console.WriteLine();

            Console.WriteLine("搭建组织架构...");
            await Phenix.Client.HttpClient.Default.Identity.User.PatchRootTeamsAsync("我的公司");
            Console.WriteLine("所属顶层团体：{0}", Phenix.Core.Reflection.Utilities.JsonSerialize(Phenix.Client.HttpClient.Default.Identity.User.RootTeams));
            Console.WriteLine("所属团体：{0}", Phenix.Core.Reflection.Utilities.JsonSerialize(Phenix.Client.HttpClient.Default.Identity.User.Teams));
            Console.WriteLine();
            Teams teamsA = Phenix.Client.HttpClient.Default.Identity.User.RootTeams.AddChild("A部门");
            Teams teamsB = Phenix.Client.HttpClient.Default.Identity.User.RootTeams.AddChild("B部门");
            Console.WriteLine("添加两部门后的组织架构：{0}", Phenix.Core.Reflection.Utilities.JsonSerialize(Phenix.Client.HttpClient.Default.Identity.User.RootTeams));
            Console.WriteLine();
            teamsB.ChangeParent(teamsA);
            Console.WriteLine("改变两部门层级关系后的组织架构：{0}", Phenix.Core.Reflection.Utilities.JsonSerialize(Phenix.Client.HttpClient.Default.Identity.User.RootTeams));
            Console.WriteLine();
            teamsB.Name = "AB部门";
            teamsB.UpdateSelf();
            Console.WriteLine("改变部门名称后的组织架构：{0}", Phenix.Core.Reflection.Utilities.JsonSerialize(Phenix.Client.HttpClient.Default.Identity.User.RootTeams));
            Console.WriteLine();
            teamsA.DeleteBranch();
            Console.WriteLine("删除部门后的组织架构：{0}", Phenix.Core.Reflection.Utilities.JsonSerialize(Phenix.Client.HttpClient.Default.Identity.User.RootTeams));
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
