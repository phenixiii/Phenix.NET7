using System;
using System.Collections.Generic;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Reflection;
using Phenix.Core.Security;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Security.Teams 功能 ****");
            Console.WriteLine();
            Console.WriteLine("Teams 类，为系统提供管理团体（Teams）信息的功能。");
            Console.WriteLine("Teams 对象，可被 JSON 序列化或反序列化，以利于跨域使用。");
            Console.WriteLine("注册用户必须加入一个团体，见 User 的 TeamsId、Teams 属性。");
            Console.WriteLine("团体的层次结构是棵倒置的树，树根/顶层团体（RootTeams）一般对应的是一家企业（命名为公司名称），逐层对应到公司的组织架构上。");
            Console.WriteLine("允许在一个SaaS模式的系统内并存N棵团体树，你在设计数据模型时应考虑到在业务数据上有记录到数据归属于哪个 RootTeams 的字段，以便切分业务数据。");
            Console.WriteLine("一棵团体树的结构，由属于顶层团体的企业组织架构管理员（见 User.IsOrganizationalArchitectureManager）进行管理。");
            Console.WriteLine("新注册用户，如果不加入现有的某个团体，可以自建一个新团体的组织架构，默认为这个团体的企业组织架构管理员，归属于顶层团体。");
            Console.WriteLine("企业组织架构管理员有维护团体的组织架构、审批普通用户加入团体（挂到某个层级节点上成为团体组员）、设置组员岗位、锁定和注销组员的权利。");
            Console.WriteLine("你可以基于此（通过调用 Teams 类公共函数）开发自己系统的企业组织架构配置管理模块，然后开放权限给到这些团体的企业组织架构管理员使用。");
            Console.WriteLine("普通用户，需等待企业组织架构管理员审批通过后才有相应的操作权限，且仅限于这个团体的数据（见SaaS模式下对业务数据的切分要求）。");
            Console.WriteLine();

            Console.WriteLine("设为调试状态");
            AppRun.Debugging = true;
            Console.WriteLine("测试过程中产生的日志保存在：" + AppRun.TempDirectory);
            Console.WriteLine();

            Console.WriteLine("注册缺省数据库连接");
            Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
            Console.WriteLine("数据库连接串 = {0}", Database.Default.ConnectionString);
            Console.WriteLine("请确认连接的是否是你的测试库？如不符，请退出程序修改 Database.RegisterDefault 部分代码段。");
            Console.Write("否则按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("开始演示");
            Teams rootTeams = Teams.New("马鞍山中理外轮理货有限公司");
            Console.WriteLine("调用方法 New() 新增顶层团体（RootTeams）：{0}", Utilities.JsonSerialize(rootTeams));
            rootTeams = Teams.FetchRoot(rootTeams.Id, -1);
            Console.WriteLine("已保存到数据库中，可调用方法 FetchRoot() 传入ID或名称，以获取：{0}", Utilities.JsonSerialize(rootTeams));
            Console.WriteLine("如果传入 resetHoursLater 参数值为正数，是要求它缓存到本进程缓存内，后续调用只要在这段时间内且 resetHoursLater 参数值为正数，都取自缓存。");
            Console.WriteLine("如果传入 resetHoursLater 参数值为 0，则优先取缓存，没有才从数据库取，之前调用时传入的缓存时间被忽略。");
            Console.WriteLine("默认 resetHoursLater 参数值为 8 小时。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Teams subTeams1 = Teams.New("业务部", rootTeams);
            Console.WriteLine("调用方法 New() 传入父层团体（{0}），新增其子层团体：{1}", Utilities.JsonSerialize(rootTeams), Utilities.JsonSerialize(subTeams1));
            Teams subTeams11 = Teams.New("调度室", subTeams1);
            Console.WriteLine("调用方法 New() 传入父层团体（{0}），新增其子层团体：{1}", Utilities.JsonSerialize(subTeams1), Utilities.JsonSerialize(subTeams11));
            Console.WriteLine("可在父层团体的 SubTeams 属性上看到子层团体：{0}", Utilities.JsonSerialize(subTeams1.SubTeams));
            Console.WriteLine("可在顶层团体的 AllSubTeams 属性上看到各层子团体：{0}", Utilities.JsonSerialize(rootTeams.AllSubTeams));
            subTeams11 = rootTeams.FindSubTeams(subTeams11.Name);
            Console.WriteLine("可通过顶层团体/父层团体的方法 FindSubTeams() 传入ID或名称，以遍历各层子团体获取：{0}", Utilities.JsonSerialize(subTeams11));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示当新增的团体与旧团体重名时（同一棵团体树内不允许重名）：");
            try
            {
                Teams.New("业务部", rootTeams);
            }
            catch (Exception ex)
            {
                Console.WriteLine(AppRun.GetErrorMessage(ex));
            }
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            subTeams1.Name = "业务一部";
            Console.WriteLine("赋值 Name 属性直接更新到数据库：{0}", Utilities.JsonSerialize(subTeams1));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示组织架构调整时，不允许挂在自己下层的团体的情况（另一个规则是仅允许在同一顶层团体下（同一棵团体树内）切挂）：");
            try
            {
                subTeams1.Parent = subTeams11;
            }
            catch (Exception ex)
            {
                Console.WriteLine(AppRun.GetErrorMessage(ex));
            }
            subTeams1.Parent = Teams.New("大船事业部", rootTeams);
            Console.WriteLine("可以挂在其他分支上：{0}", Utilities.JsonSerialize(subTeams1.Parent));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示删除团体时的规则，不允许删除中层团体：");
            try
            {
                subTeams1.Delete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(AppRun.GetErrorMessage(ex));
            }
            Console.WriteLine("必须从底层向上逐层删除团体，可以直到顶层团体。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("以下示例采用递归方式删除整棵树：");
            IDictionary<string, long> nameIdDictionary = Teams.FetchRootNames("中理");
            Console.WriteLine("先获取到顶层团队的‘name/rootId’字典集合：{0}", Utilities.JsonSerialize(nameIdDictionary));
            if (nameIdDictionary.TryGetValue("马鞍山中理外轮理货有限公司", out long rootId))
            {
                DeleteTree(Teams.FetchRoot(rootId));
                Console.WriteLine("已完成整棵树的删除。");
            }
            else
                Console.WriteLine("未能完成整棵树的删除。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }

        private static void DeleteTree(Teams teams)
        {
            foreach (Teams item in new List<Teams>(teams.SubTeams))
                DeleteTree(item);
            teams.Delete();
        }
    }
}
