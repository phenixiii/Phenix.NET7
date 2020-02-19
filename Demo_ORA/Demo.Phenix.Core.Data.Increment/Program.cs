using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.SyncCollections;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Data.Increment 功能 ****");
            Console.WriteLine();
            Console.WriteLine("Increment 类，64位增量生成器，仅允许通过 Database.Increment 获取到实例。");
            Console.WriteLine("每次调用 Database 的 Increment 属性的 GetNext() 函数，将获得该数据库下同一 key 从 initialValue 起连续递增的 Long 类型数值，可作为业务码（比如订单号）递增部分的填充来源。");
            Console.WriteLine("受并行竞争影响，单个进程或线程不一定能获得连续的增量值，但整个数据库下一定是连续的。");
            Console.WriteLine("需注意的是，取到后是用还是弃用，由你的代码自行决定。");
            Console.WriteLine();

            Console.WriteLine("本场景是在服务端运行，如果要在客户端获取64位增量，请使用 phAjax.getIncrement() / Phenix.Client.HttpClient.GetIncrementAsync()。");
            Console.WriteLine();

            Console.WriteLine("设为调试状态");
            AppRun.Debugging = true;
            Console.WriteLine("测试过程中产生的日志保存在：" + AppRun.TempDirectory);
            Console.WriteLine();

            Console.WriteLine("在接下来的演示之前，请检查缺省数据库连接配置信息，以保证其连接到的是你指定的测试库。");
            Console.WriteLine("数据库连接配置信息存放在 SQLite 库 Phenix.Core.db 文件的 PH7_Database 表中，配置方法见其示例记录的 Remark 字段内容。");
            Console.WriteLine("缺省数据库连接串：{0}", Database.Default.ConnectionString);
            Console.WriteLine("如不符，请退出程序，找到 PH7_Database 表中那条 DataSourceKey 字段值为'*'的记录，配置好后再重启本程序。");
            Console.Write("如果确认准确无误，请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("启动3个线程分别读取不同 key 下 Increment 的值（注意即使 key 相同但 initialValue 不同的话也是不一样的递增序列）：");
            Task[] tasks = new[]
            {
                Task.Run(() => FetchIncrement(1)),
                Task.Run(() => FetchIncrement(2)),
                Task.Run(() => FetchIncrement(3))
            };
            Task.WaitAll(tasks);
            foreach (KeyValuePair<string, int> kvp in _incrementValues)
            {
                Console.Write("business code = {0}, taskIndex = {1}", kvp.Key, kvp.Value);
                Console.WriteLine();
            }

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }

        static readonly SynchronizedSortedDictionary<string, int> _incrementValues = new SynchronizedSortedDictionary<string, int> ();

        static void FetchIncrement(int taskIndex)
        {
            Task[] tasks = new[]
            {
                Task.Run(() => FetchIncrement("SX2019AAA", 0, taskIndex)),
                Task.Run(() => FetchIncrement("SX2019AAA", 1000, taskIndex)),
                Task.Run(() => FetchIncrement("SX2019XXX", 1, taskIndex)),
            };
            Task.WaitAll(tasks);
        }

        static void FetchIncrement(string key, long initialValue, int index)
        {
            for (int i = 0; i < 10; i++)
                _incrementValues.Add(String.Format("{0}-{1:D6}", key, Database.Default.Increment.GetNext(key, initialValue)), index);
        }
    }
}
