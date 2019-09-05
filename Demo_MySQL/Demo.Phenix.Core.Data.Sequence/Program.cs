using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            Console.WriteLine("Sequence 类，为系统提供唯一的序列号，支持多线程、多进程、分布式应用，每次调用 Value 属性可获取不会重复的 15 位 Long 值，但不保证是连续的。");
            Console.WriteLine("序列号可作为业务对象（业务表）ID 值的填充来源。");
            Console.WriteLine();

            Console.WriteLine("注册缺省数据库连接");
            Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
            Console.WriteLine("数据库连接串 = {0}", Database.Default.ConnectionString);
            Console.WriteLine("请确认连接的是否是你的测试库？如不符，请退出程序修改 Database.RegisterDefault 部分代码段。");
            Console.WriteLine("否则按任意键继续");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("启动 3 个线程分别读取 Sequence：");
            Task[] tasks = new[]
            {
                Task.Run(() => FetchSequence(1)),
                Task.Run(() => FetchSequence(2)),
                Task.Run(() => FetchSequence(3))
            };
            Task.WaitAll(tasks);
            foreach (KeyValuePair<long, int> kvp in _sequenceValues)
            {
                Console.Write("sequence = {0}, index = {1}", kvp.Key, kvp.Value);
                Console.WriteLine();
            }

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }

        static readonly SynchronizedSortedDictionary<long, int> _sequenceValues = new SynchronizedSortedDictionary<long, int>();

        static void FetchSequence(int index)
        {
            for (int i = 0; i < 10; i++)
            {
                _sequenceValues.Add(Phenix.Core.Data.Sequence.Value, index);
            }
        }
    }
}
