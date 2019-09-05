using System;
using System.Collections.Generic;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Reflection;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Data.Model.EntityBase 功能 ****");
            Console.WriteLine();
            Console.WriteLine("EntityBase 类，为系统开发封装了轻量级持久化引擎、动态刷新等基础设施功能。");
            Console.WriteLine("实体类可以不用继承 EntityBase 也能实现同样的功能，比如在数据库表中删除 user 对象的记录：");
            Console.WriteLine("Database.Default.MetaData.FindSheet<User>().DeleteRecord<User>(p => p.Id == user.Id)");
            Console.WriteLine("虽然看不到冗长的 ADO.NET 代码，可读性强，但还是比较繁琐，容易写错（拷贝/黏贴时最容易出错）。");
            Console.WriteLine("继承 EntityBase 类的话，只需：");
            Console.WriteLine("user.DeleteRecord(p => p.Id)");
            Console.WriteLine("虽对实体类代码有一定侵入性，但编码效率高，不易出错。");
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("不管是继承 EntityBase 还是直接操作 Database，持久化引擎都要求你在设计表结构和实体类时，遵循以下编写规范：");
            Console.WriteLine("1，实体类命名法为Pascal，不管是表还是视图，它们名称里的下划线，都在映射时会自动剔除掉，比如“TPT_PROJECT_INFO”表名映射的是“TptProjectInfo”类名（例外：“PH7_”前缀会被忽略）；");
            Console.WriteLine("   你可以设置Phenix.Core.Data.Schema.Table配置项ClassNameByTrimTableName，规定ClassName属性的取值是否取自被整理的表名(如果第4位是“_”则剔去其及之前的字符)，默认是{0}；", Phenix.Core.Data.Schema.Table.ClassNameByTrimTableName);
            Console.WriteLine("   你可以设置Phenix.Core.Data.Schema.View配置项ClassNameByTrimViewName，规定ClassName属性的取值是否取自被整理的视图名(如果第4位是“_”则剔去其及之前的字符, 如果倒数第2位是“_”则剔去其及之后的字符)，默认是{0}；", Phenix.Core.Data.Schema.View.ClassNameByTrimViewName);
            Console.WriteLine("2，实体类的属性名也是Pascal规则；字段命名法是加“_”前缀的camel规则名，比如“_projectName”；");
            Console.WriteLine("   表/视图的字段名/别名，如带前缀，字符数为3个且第3个字符是“_”，映射到类的属性/字段时会自动剔除掉，比如字段“PI_PROJECT_NAME”映射到类的属性名是“ProjectName”/字段名是“_projectName”；");
            Console.WriteLine("   表/视图的字段名/别名，命名时如不出于以下目的，请避免使用如下后缀，它们是持久层引擎的保留字：");
            Console.WriteLine("   “_ID”且是长整型15位以上精度：主键/外键，新增记录时自动填充Sequence.Value；每张表都应该有且仅一个长整型15位以上精度的主键字段，外键分物理外键（组合关系）和虚拟外键（聚合关系），命名尽可能与主键相呼应；");
            Console.WriteLine("   “_FG”且是整型2位/1位精度：枚举/布尔，映射到类的属性/字段时会被自动剔除掉后缀，比如字段“PI_PROJECT_TYPE_FG”映射到类的属性名是“ProjectType”/字段名是“_projectType”，类型应该是 ProjectType 枚举；");
            Console.WriteLine("   “_ORIGINATOR”且是字符串/长整型15位以上精度：新增记录时自动填充Identity.CurrentIdentity.User.Name/Id；");
            Console.WriteLine("   “_ORIGINATE_TIME”且是DateTime：新增记录时自动填充当前时间；");
            Console.WriteLine("   “_ORIGINATE_TEAMS”且是字符串/长整型15位以上精度：新增记录时自动填充Identity.CurrentIdentity.User.RootTeams.Name/Id，可用于SaaS模式下的系统开发，对不同团体的数据进行切片；");
            Console.WriteLine("   “_UPDATER”：且是字符串/长整型15位以上精度：更新记录时自动填充Identity.CurrentIdentity.User.Name/Id；");
            Console.WriteLine("   “_UPDATE_TIME”且是DateTime：更新记录时自动填充当前时间；");
            Console.WriteLine("   “_TIMESTAMP”且是长整型15位以上精度：时间戳，更新记录时自动填充Sequence.Value；时间戳可用于乐观锁模式下的数据更新，保证在分布式架构下新数据不会被脏数据覆盖（会抛出Phenix.Core.Data.Validity.OutdatedDataException）；");
            Console.WriteLine("以上是全部的编写规范。");
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("持久化引擎支持在实体属性上打System.ComponentModel.DataAnnotations.ValidationAttribute派生标签，提交对象/属性时会自动完成属性的有效性验证（验证失败抛出ValidationException）。");
            Console.WriteLine("以下默认规范会自动添加，不必手工打标签：");
            Console.WriteLine("    System.ComponentModel.DataAnnotations.RequiredAttribute，如果属性值不允许为空的规范是按照表字段NOT NULL的话；");
            Console.WriteLine("    System.ComponentModel.DataAnnotations.StringLengthAttribute，如果字符串属性值长度要求是按照表字段长度规范的话；");
            Console.WriteLine("一旦手工打上标签，默认规范会被覆盖");
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("持久化引擎支持在实体上实现System.ComponentModel.DataAnnotations.IValidatableObject接口，也可以实现Phenix.Core.Data.Validity.IValidation接口，提交对象时自动完成对象的有效性验证（验证失败抛出ValidationException）。");
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("以下演示借用了 Teams 类，源码见工程里同名文件，可作为你编写自己实体类的一个参考。");
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("设为调试状态");
            AppRun.Debugging = true;
            Console.WriteLine("测试过程中产生的日志保存在：" + AppRun.TempDirectory);
            Console.WriteLine();

            Console.WriteLine("注册缺省数据库连接");
            Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
            Console.WriteLine("数据库连接串 = {0}", Database.Default.ConnectionString);
            Console.WriteLine("请确认连接的是否是你的测试库？如不符，请退出程序修改 Database.RegisterDefault 部分代码段。");
            Console.WriteLine("否则按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("开始演示");
            Teams rootTeams = Teams.New("马鞍山中理外轮理货有限公司");
            Console.WriteLine("调用方法 New() 新增顶层团体（RootTeams）：{0}", Utilities.JsonSerialize(rootTeams));
            Teams subTeams1 = Teams.New("业务部", rootTeams);
            Console.WriteLine("调用方法 New() 传入父层团体（{0}），新增其子层团体：{1}", Utilities.JsonSerialize(rootTeams), Utilities.JsonSerialize(subTeams1));
            Teams subTeams11 = Teams.New("调度室", subTeams1);
            Console.WriteLine("调用方法 New() 传入父层团体（{0}），新增其子层团体：{1}", Utilities.JsonSerialize(subTeams1), Utilities.JsonSerialize(subTeams11));
            subTeams1.Name = "业务一部";
            Console.WriteLine("赋值 Name 属性直接更新到数据库：{0}", Utilities.JsonSerialize(subTeams1));
            subTeams1.Parent = Teams.New("大船事业部", rootTeams);
            Console.WriteLine("可以挂在其他分支上：{0}", Utilities.JsonSerialize(subTeams1.Parent));
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("最后，清理环境");
            IDictionary<string, long> nameIdDictionary = Teams.FetchRootNames("中理");
            Console.WriteLine("先获取到顶层团队的‘name/rootId’字典集合：{0}", Utilities.JsonSerialize(nameIdDictionary));
            if (nameIdDictionary.TryGetValue("马鞍山中理外轮理货有限公司", out long rootId))
            {
                DeleteTree(Teams.FetchRoot(rootId));
                Console.WriteLine("已完成整棵树的删除。");
            }
            else
                Console.WriteLine("未能完成整棵树的删除。");
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
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
