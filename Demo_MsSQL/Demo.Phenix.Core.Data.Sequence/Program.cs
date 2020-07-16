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
            Console.WriteLine("**** 演示 Phenix.Core.Data.Sequence 功能 ****");
            Console.WriteLine();
            Console.WriteLine("Sequence 类，64位序号生成器，仅允许通过 Database.Sequence 获取到实例。");
            Console.WriteLine("每次获取 Database 的 Sequence 属性的 Value 值，将获得该数据库下不会重复（但不保证连续）的15位 Long 类型数值，可作为业务对象（业务表）ID 值的填充来源。");
            Console.WriteLine();

            Console.WriteLine("本场景是在服务端运行，如果要在客户端获取64位序号，请使用 phAjax.getSequence() / Phenix.Client.HttpClient.GetSequenceAsync()。");
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

            Console.WriteLine("启动3个线程分别读取 Sequence 的值：");
            Task[] tasks = new[]
            {
                Task.Run(() => FetchSequence(1)),
                Task.Run(() => FetchSequence(2)),
                Task.Run(() => FetchSequence(3))
            };
            Task.WaitAll(tasks);
            foreach (KeyValuePair<long, int> kvp in _sequenceValues)
            {
                Console.Write("sequence = {0}, taskIndex = {1}", kvp.Key, kvp.Value);
                Console.WriteLine();
            }

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }

        static readonly SynchronizedSortedDictionary<long, int> _sequenceValues = new SynchronizedSortedDictionary<long, int>();

        static void FetchSequence(int taskIndex)
        {
            for (int i = 0; i < 10; i++)
                _sequenceValues.Add(Database.Default.Sequence.Value, taskIndex);
        }
    }
}
