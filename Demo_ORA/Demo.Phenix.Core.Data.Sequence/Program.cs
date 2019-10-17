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

            Console.WriteLine("Sequence 类，可以在服务端使用也可以在客户端使用。");
            Console.WriteLine("在服务端使用，如果注册过数据库连接（见 Database 属性），就以该数据库为基准保证数据的唯一性，否则会尝试向其 HttpClient 属性所指向的 WebAPI 服务发起请求，一层层调用直到最后一层服务是直连数据库的可以作为基准为止。");
            Console.WriteLine("HttpClient 属性，默认为 Phenix.Core.Net.Http.HttpClient.Default（即第一个被构造的 HttpClient），除非直接赋值指定，或者调用函数时传参指定。");
            Console.WriteLine("本 Demo 演示了直连数据库的服务端使用场景。");
            Console.WriteLine();

            Console.WriteLine("注册缺省数据库连接");
            Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
            Console.WriteLine("数据库连接串 = {0}", Database.Default.ConnectionString);
            Console.WriteLine("请确认连接的是否是你的测试库？如不符，请退出程序修改 Database.RegisterDefault 部分代码段。");
            Console.Write("否则按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
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
