using System;
using System.Collections.Generic;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Data.Schema;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Data.Common.DataReader 功能 ****");
            Console.WriteLine();
            Console.WriteLine("DataReader 类，提供了一系列便于开发的初始化函数，除了直接传 SQL 语句外，还可以传 Command、Connection、Transaction、Database、CommandBehavior、bool? needSaveLog 参数，可自行组合达到不一样的效果。");
            Console.WriteLine("本演示仅是传 SQL 语句，你可在此基础上，自行编码体验不同的写法。");
            Console.WriteLine("如传入 Command、Connection、Transaction 对象，请自行编码控制它们的生命周期，及时释放。");
            Console.WriteLine("DataReader 类除了封装 .NET DataReader 原功能外，还提供了 GetNullableXXX 系列函数，以利获取允许为空的字段值时精简代码。");
            Console.WriteLine();

            Console.WriteLine("注册缺省数据库连接");
            Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
            Console.WriteLine("数据库连接串 = {0}", Database.Default.ConnectionString);
            Console.WriteLine("请确认连接的是否是你的测试库？如不符，请退出程序修改 Database.RegisterDefault 部分代码段。");
            Console.WriteLine("否则按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.Write("将随机挑选数据库的某个视图进行演示：");
            View view;
            while (true)
            {
                MetaData metaData = Database.Default.FetchMetaData(-1); //负数为即刻重置, 一般情况下用Database.Default.MetaData属性就能满足需要
                if (metaData.Views.Count > 0)
                {
                    List<View> views = new List<View>(metaData.Views.Values);
                    view = views[new Random().Next(0, metaData.Views.Count)];
                    Console.WriteLine(view.ViewText);
                    break;
                }
                Console.WriteLine("数据库中还未有视图，请新建一个用于演示!");
                Console.WriteLine("如已准备好，请按任意键继续");
                Console.ReadKey();
            }
            Console.WriteLine();

            Console.WriteLine("演示 DataReader 自动连接缺省数据库获取数据...");
            using (DataReader reader = Database.Default.CreateDataReader(view.ViewText))
            {
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                        Console.Write("{0} = {1}, ", reader.GetName(i), reader.GetValue(i) ?? "null");
                    Console.WriteLine();
                }

                if (!reader.HasRows)
                    Console.WriteLine("么有一条记录");
            }
            Console.WriteLine();
            
            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
