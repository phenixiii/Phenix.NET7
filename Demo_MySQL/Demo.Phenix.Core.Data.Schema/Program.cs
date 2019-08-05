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
            Console.WriteLine("这些信息的对象，可被 JSON 序列化或反序列化，以利于跨域使用。");
            Console.WriteLine();

            Console.WriteLine("注册缺省数据库连接");
            Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
            Console.WriteLine("数据库连接串 = {0}", Database.Default.ConnectionString);
            Console.WriteLine("请确认连接的是否是你的测试库？如不符，请退出程序修改 Database.RegisterDefault 部分代码段。");
            Console.WriteLine("否则按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("构建数据库构架对象...");
            MetaData metaData = Database.Default.MetaData;
            Console.WriteLine("数据库构架对象可序列化为 JSON 字符串...");
            string json = Utilities.JsonSerialize(metaData);
            Console.WriteLine(json);
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("JSON 字符串可反序列化为数据库构架对象...");
            metaData = Utilities.JsonDeserialize<MetaData>(json);
            Console.WriteLine("数据库含{0}个表、{1}个视图。", metaData.Tables.Count, metaData.Views.Count);
            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
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

            Console.WriteLine("请按任意键继续");
            Console.ReadKey();
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
