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
            Console.WriteLine("**** 演示 Phenix.Core.Security.Position 功能 ****");
            Console.WriteLine();
            Console.WriteLine("Position 类，为系统提供管理岗位（Position）信息的功能。");
            Console.WriteLine("Position 对象，可被 JSON 序列化或反序列化，以利于跨域使用。");
            Console.WriteLine("注册用户必须拥有一个岗位，见 User 的 PositionId、Position 属性。");
            Console.WriteLine("岗位的 Roles 属性用于限制注册用户使用系统的功能范围，可配合 System.Web.Http.AuthorizeAttribute 完成 WebAPI 服务的授权控制。");
            Console.WriteLine("可见，你在系统设计时应该已经将 Role 绑定到了具体的 WebAPI 服务上了，那么在系统运行期，可灵活配置的是将 Role 动态绑定到岗位上。");
            Console.WriteLine("你可以基于此（通过调用 Position 类公共函数）开发自己系统的岗位配置管理模块，然后开放权限给到系统Admin管理员使用。");
            Console.WriteLine("系统Admin管理员凌驾于使用系统的企业（RootTeams），对岗位（Position）的配置结果会影响到所有企业的系统用户（User）。");
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
            Position position = Position.New("企业组织架构管理员", new string[]
            {
                "组织架构管理", "岗位管理", "组员管理"
            });
            Console.WriteLine("调用方法 New() 新增：{0}", Utilities.JsonSerialize(position));
            position = Position.Fetch(position.Id, -1);
            Console.WriteLine("已保存到数据库中，可调用方法 Fetch() 获取：{0}", Utilities.JsonSerialize(position));
            Console.WriteLine("如果传入 resetHoursLater 参数值为正数，是要求它缓存到本进程缓存内，后续调用只要在这段时间内且 resetHoursLater 参数值为正数，都取自缓存。");
            Console.WriteLine("如果传入 resetHoursLater 参数值为 0，则优先取缓存，没有才从数据库取，之前调用时传入的缓存时间被忽略。");
            Console.WriteLine("默认 resetHoursLater 参数值为 8 小时。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            List<string> roles = new List<string>(position.Roles);
            roles.RemoveAt(2);
            position.Roles = roles;
            Console.WriteLine("赋值 Roles 属性可更新到数据库：{0}", Utilities.JsonSerialize(position));
            Console.WriteLine("赋值 Name 属性也会更新到数据库，可自行编码体验，注意岗位不允许重名。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            IList<Position> positions = Position.FetchAll();
            Console.WriteLine("可调用方法 FetchAll() 从数据库中获取全部的岗位资料：{0}", Utilities.JsonSerialize(positions));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示当新增的岗位与旧岗位重名时：");
            try
            {
                Position.New("企业组织架构管理员", new string[] { "人事管理" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(AppRun.GetErrorMessage(ex));
            }
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            position.Delete();
            Console.WriteLine("可调用方法 Delete() 从数据库中删除岗位资料：{0}", Utilities.JsonSerialize(position));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
