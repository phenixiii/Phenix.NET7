using System;
using System.Collections.Generic;
using Phenix.Core.Data;
using Phenix.Core.Data.Schema;
using Phenix.Core.Reflection;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Data.Schema 功能 ****");
            Console.WriteLine();
            Console.WriteLine("Phenix.Core.Data.Schema 命名空间里提供的类，可获取到数据库表和视图的元数据，包括字段、索引、主键、外键、子健等信息。");
            Console.WriteLine("这些信息的对象，可被 JSON 序列化或反序列化， 以利于跨域使用。");
            Console.WriteLine();

            Console.WriteLine("不管是继承 EntityBase 还是直接操作 Database，持久化引擎都要求你在设计表结构和实体类时，遵循以下编写规范：");
            Console.WriteLine("1，实体类命名法为Pascal，不管是表还是视图，它们名称里的下划线，都在映射时会自动剔除掉，比如“TPT_PROJECT_INFO”表名映射的是“TptProjectInfo”类名（例外：“PH7_”前缀会被忽略）；");
            Console.WriteLine("   你可以设置Phenix.Core.Data.Schema.Table配置项ClassNameByTrimTableName，规定ClassName属性的取值是否取自被整理的表名(如果第4位是“_”则剔去其及之前的字符)，默认是{0}；", Phenix.Core.Data.Schema.Table.ClassNameByTrimTableName);
            Console.WriteLine("   你可以设置Phenix.Core.Data.Schema.View配置项ClassNameByTrimViewName，规定ClassName属性的取值是否取自被整理的视图名(如果第4位是“_”则剔去其及之前的字符, 如果倒数第2位是“_”则剔去其及之后的字符)，默认是{0}；", Phenix.Core.Data.Schema.View.ClassNameByTrimViewName);
            Console.WriteLine("2，实体类的属性名也是Pascal规则；字段命名法是加“_”前缀的camel规则名，比如“_projectName”；");
            Console.WriteLine("   表/视图的字段名/别名，如带前缀，字符数为3个且第3个字符是“_”，映射到类的属性/字段时会自动剔除掉，比如字段“PI_PROJECT_NAME”映射到类的属性名是“ProjectName”/字段名是“_projectName”；");
            Console.WriteLine("   表/视图的字段名/别名，命名时如不出于以下目的，请避免使用如下后缀，它们是持久层引擎的保留字：");
            Console.WriteLine("   “_ID”且是长整型15位以上精度：主键/外键；如为主键，新增记录时自动填充Sequence.Default.Value；每张表都应该有且仅一个长整型15位以上精度的主键字段，外键分物理外键（组合关系）和虚拟外键（聚合关系），命名尽可能与主键相呼应；");
            Console.WriteLine("   “_ORIGINATOR”且是字符串/长整型15位以上精度：新增记录时自动填充Identity.CurrentIdentity.User.Name/Id；");
            Console.WriteLine("   “_ORIGINATE_TIME”且是DateTime：新增记录时自动填充当前时间；");
            Console.WriteLine("   “_ORIGINATE_TEAMS”且是字符串/长整型15位以上精度：新增记录时自动填充Identity.CurrentIdentity.User.RootTeams.Name/Id；");
            Console.WriteLine("   “_UPDATER”且是字符串/长整型15位以上精度：更新记录时自动填充Identity.CurrentIdentity.User.Name/Id；");
            Console.WriteLine("   “_UPDATE_TIME”且是DateTime：更新记录时自动填充当前时间；");
            Console.WriteLine("   “_TIMESTAMP”且是长整型15位以上精度：时间戳，更新记录时自动填充Sequence.Default.Value；时间戳可用于乐观锁模式下的数据更新，保证在分布式架构下新数据不会被脏数据覆盖（会抛出Phenix.Core.Data.Rule.OutdatedDataException）；");
            Console.WriteLine("   “_RU”：分库路由字段，仅在新增记录时被提交，提交后不允许修改，分库增删改查时其值被HASH路由到指定的数据库；");
            Console.WriteLine("   “_WM”：水印字段，仅在新增记录时被提交，提交后不允许修改；");
            Console.WriteLine("   “_FG”且是整型2位/1位精度：枚举/布尔，映射到类的属性/字段时会被自动剔除掉后缀，比如字段“PI_PROJECT_TYPE_FG”映射到类的属性名是“ProjectType”/字段名是“_projectType”，类型应该是 ProjectType 枚举；");
            Console.WriteLine("   “_RU”、“_WM”可以组合“_FG”一起使用，组合时先后次序随意；");
            Console.WriteLine("以上是全部的编写规范。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("在接下来的演示之前，请检查缺省数据库连接配置信息，以保证其连接到的是你指定的测试库。");
            Console.WriteLine("数据库连接配置信息存放在 SQLite 库 Phenix.Core.db 文件的 PH7_Database 表中，配置方法见其示例记录的 Remark 字段内容。");
            Console.WriteLine("缺省数据库连接串：{0}", Database.Default.ConnectionString);
            Console.WriteLine("如不符，请退出程序，找到 PH7_Database 表中那条 DataSourceKey 字段值为'*'的记录，配置好后再重启本程序。");
            Console.Write("如果确认准确无误，请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("构建数据库构架对象...");
            MetaData metaData = Database.Default.MetaData;
            Console.WriteLine("数据库构架对象可序列化为 JSON 字符串...");
            string json = Utilities.JsonSerialize(metaData);
            Console.WriteLine(json);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("JSON 字符串可反序列化为数据库构架对象...");
            metaData = Utilities.JsonDeserialize<MetaData>(json);
            Console.WriteLine("数据库含{0}个表、{1}个视图", metaData.Tables.Count, metaData.Views.Count);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("罗列全部表的结构...");
            foreach (KeyValuePair<string, Table> kvp in metaData.Tables)
            {
                Console.WriteLine("表{0}({1})主键: {2}", kvp.Key, kvp.Value.Description, Utilities.JsonSerialize(kvp.Value.PrimaryKeys));
                Console.WriteLine("表{0}({1})外键: {2}", kvp.Key, kvp.Value.Description, Utilities.JsonSerialize(kvp.Value.ForeignKeys));
                Console.WriteLine("表{0}({1})子键: {2}", kvp.Key, kvp.Value.Description, Utilities.JsonSerialize(kvp.Value.DetailForeignKeys));
                Console.WriteLine("表{0}({1})索引: {2}", kvp.Key, kvp.Value.Description, Utilities.JsonSerialize(kvp.Value.Indexes));
                Console.WriteLine("表{0}({1})字段: {2}", kvp.Key, kvp.Value.Description, Utilities.JsonSerialize(kvp.Value.Columns));
                Console.WriteLine();
            }

            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("罗列全部视图的结构...");
            foreach (KeyValuePair<string, View> kvp in metaData.Views)
            {
                Console.WriteLine("视图{0}({1})内容: {2}", kvp.Key, kvp.Value.Description, kvp.Value.ViewText);
                Console.WriteLine("视图{0}({1})数据源: {2}", kvp.Key, kvp.Value.Description, Utilities.JsonSerialize(kvp.Value.Tables.Keys));
                Console.WriteLine("视图{0}({1})字段: {2}", kvp.Key, kvp.Value.Description, Utilities.JsonSerialize(kvp.Value.Columns));
                Console.WriteLine();
            }

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
