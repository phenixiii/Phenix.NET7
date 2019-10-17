using System;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Reflection;
using Phenix.Core.Security;
using Phenix.Core.Security.Cryptography;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Security.User 功能 ****");
            Console.WriteLine();
            Console.WriteLine("User 类，为系统提供管理注册用户（User）信息的功能。");
            Console.WriteLine("User 对象，可被 JSON 序列化或反序列化，以利于跨域使用。");
            Console.WriteLine("注册用户必须拥有一个岗位（见 PositionId、Position 属性）加入一个团体（见 TeamsId、Teams 属性）才能全面使用系统（范围受岗位的 Roles 属性限制，配合 System.Web.Http.AuthorizeAttribute 完成 WebAPI 服务的授权控制）。");
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

            Console.WriteLine("演示注册用户的方法 New()");
            string initialPassword;
            string dynamicPassword;
            User user = User.New("林冲", "13966666666", "13966666666@139.com", "豹子头", "127.0.0.1", out initialPassword, out dynamicPassword);
            Console.WriteLine("当有用户申请注册时，你的服务应调用方法 New() 新增用户，然后将调用返回的初始口令 {0} 或动态口令 {1} 利用第三方渠道（邮箱或短信）推送给到用户。", initialPassword, dynamicPassword);
            Console.WriteLine("被持久化的用户信息里，登录口令和动态口令都已被MD5散列了的。");
            Console.WriteLine("用户的手机 {0}、邮箱 {1}、注册昵称 {2}，这些属性在注册后仍然是可以赋值被自动提交持久化的，前提是你必须保证系统已经成功验证了用户身份才能允许修改。", user.Phone, user.EMail, user.RegAlias);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示获取用户资料的方法 Fetch()");
            user = User.Fetch("林冲");
            Console.WriteLine("你的服务在响应用户登录时，首先根据传来的用户名获取用户资料，以便完成下一步的身份验证：{0}", Utilities.JsonSerialize(user));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示核对登录口令有效性的方法 IsValidPassword()");
            Console.WriteLine("你的服务在做用户身份验证时，客户端除了传来用户名，也要提供登录口令（你必须在客户端完成MD5散列化）。");
            Console.WriteLine("身份验证结果：{0}", user.IsValidPassword(MD5CryptoTextProvider.ComputeHash(initialPassword), "127.0.0.1", false));
            Console.WriteLine("这个函数的第二个参数，要求你的服务能识别出请求方的IP地址，以便后续在响应客户端请求时能做比对 {0}，实现禁止多处终端发起请求的功能，开关是 User.AllowMultiAddressRequest 属性（默认值 {1}）。", user.RequestAddress, User.AllowMultiAddressRequest);
            Console.WriteLine("用户登录后，服务在每次响应客户端的请求时都要做身份验证，具体实现可以交给 WebAPI 服务框架来完成，在此略过。");
            Console.WriteLine("如果同一用户名在尝试多次身份验证都失败的话，是有怀疑服务在被高频试错攻击，应该禁止这个用户名在一段时间内的登录请求。");
            Console.WriteLine("防护措施可配置如下：");
            Console.WriteLine("服务请求失败次数极限，User.RequestFailureCountMaximum 属性，缺省为 {0}(次>=3)", User.RequestFailureCountMaximum);
            Console.WriteLine("服务请求失败锁定周期，User.RequestFailureLockedMinutes 属性，缺省为 {0}(分钟>=10)", User.RequestFailureLockedMinutes);
            Console.WriteLine("服务请求超时时限(与服务端时钟差值)，User.RequestOvertimeLimitMinutes 属性，缺省为 {0}(分钟>=10)", User.RequestOvertimeLimitMinutes);
            Console.WriteLine("你也可以直接锁定用户，将属性 Locked 置为true，缺省为 {0}，可恢复", user.Locked);
            Console.WriteLine("甚至注销用户，将属性 Disabled 置为true，缺省为 {0}，可恢复", user.Disabled);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示修改登录口令的方法 ChangePassword()");
            Console.WriteLine("你的服务可以响应用户修改登录口令的请求，调用方法 ChangePassword() 传入新口令（明码）。");
            string newPassword = "123456";
            Console.WriteLine("类似 {0} 这样的简单口令是被禁止的：{1}", newPassword, user.ChangePassword(newPassword, false) == false ? "ok" : "error");
            Console.WriteLine("登录口令的编码规则如下：");
            Console.WriteLine("口令长度最小值，User.PasswordLengthMinimum 属性，缺省为 {0}(个>=6)", User.PasswordLengthMinimum);
            Console.WriteLine("口令复杂度最小值(含数字、大写字母、小写字母、特殊字符的种类)， User.PasswordComplexityMinimum 属性，缺省为 {0}(个>=1)", User.PasswordComplexityMinimum);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示核对动态口令有效性的方法 IsValidDynamicPassword()");
            Console.WriteLine("你的服务除了可以通过核对登录口令的有效性来验证用户身份外，还可以通过核对动态口令的方法。");
            Console.WriteLine("身份验证结果：{0}", user.IsValidDynamicPassword(MD5CryptoTextProvider.ComputeHash(dynamicPassword), "127.0.0.1", false));
            Console.WriteLine("动态口令是有有效期的，可通过设置 User.DynamicPasswordValidityMinutes 属性（默认值为 {0} 分钟以内）进行控制。", User.DynamicPasswordValidityMinutes);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示申请动态口令的方法 ApplyDynamicPassword()");
            user = User.Fetch("林冲");
            Console.WriteLine("动态口令登录，客户端仅需提供用户名，你的服务 Fetch 到用户资料：{0}", Utilities.JsonSerialize(user));
            dynamicPassword = user.ApplyDynamicPassword("127.0.0.1", false);
            Console.WriteLine("然后生成一个6位随机的动态口令：{0}", dynamicPassword);
            Console.WriteLine("通过短信等第三方渠道推送给到用户，用户用它来登录系统，客户端向服务发起动态口令登录请求（你必须在客户端完成动态口令的MD5散列化）。");
            Console.WriteLine("身份验证结果：{0}", user.IsValidDynamicPassword(MD5CryptoTextProvider.ComputeHash(dynamicPassword), "127.0.0.1", false));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示用户绑定所属团体的属性 Teams 和担任岗位的属性 Position");
            user.Position = Position.New("企业组织架构管理员", new string[]
            {
                "组织架构管理", "岗位管理", "组员管理"
            });
            Console.WriteLine("新增的一个岗位资料，假设是由你系统的岗位配置管理模块提供的，供注册用户所属团体树的企业组织架构管理员（见 User.IsOrganizationalArchitectureManager）完成绑定：{0}", Utilities.JsonSerialize(user.Position));
            user.Teams = Teams.New("马鞍山中理外轮理货有限公司");
            Console.WriteLine("所属团体，可以由注册用户自己新增（那他就是这棵团体树的企业组织架构管理员），也可以向某棵团体树的企业组织架构管理员提出加入组织的申请，由管理员审核通过后帮你绑定到他的团队树（某个枝节）上：{0}", Utilities.JsonSerialize(user.Teams));
            Console.WriteLine("现在 {0} 成为了 {1} 的一名企业组织架构管理员：{2}", user.Name, user.RootTeams.Name, user.IsOrganizationalArchitectureManager ? "ok" : "error");
            Console.WriteLine("以上提到的管理功能，需要你在系统中自行实现（如果你的系统需要这样的SaaS模式的话）。");
            Console.WriteLine("总之，你系统的岗位配置管理模块、组织架构管理模块、注册用户的权限管理（申请、审核、担任岗位和所属团体）模块，都是需要你自行开发实现的。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("最后，清理环境");
            Console.WriteLine("鉴于 User 类不提供删除资料的功能，我们直接操作数据库来实现。");
            Console.WriteLine("完成用户资料的删除：{0}", Database.Default.MetaData.FindSheet<User>().DeleteRecord<User>(p => p.Id == user.Id) == 1 ? "ok" : "error");
            user.Position.Delete();
            Console.WriteLine("完成岗位资料的删除。");
            user.Teams.Delete();
            Console.WriteLine("完成团体资料的删除。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
