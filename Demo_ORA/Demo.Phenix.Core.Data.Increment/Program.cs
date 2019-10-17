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
            Console.WriteLine("**** 演示 Phenix.Core.Data.Increment 功能 ****");
            Console.WriteLine();
            Console.WriteLine("Increment 类，为系统提供连续递增的序列号，支持多线程、多进程、分布式应用，每次调用 GetNext 函数可获取参数 key 下从 initialValue 起连续的 Long 值。");
            Console.WriteLine("同一参数 key 值下，参数 initialValue 仅在第一时间调用时起作用，而且是被第一时间调用者抢得，之后调用 GetNext 函数时传什么 initialValue 值，就都已无意义了。");
            Console.WriteLine("受线程竞争影响，单个线程不一定能获得连续的序列号，但在整个系统里一定是连续的，需注意的是，获取后是用还是弃用，由系统编程设计决定。");
            Console.WriteLine("序列号可作为业务码（比如类似‘SX+日期+序列号’格式的订单号）中递增段内容的填充来源，可用业务码某一段内容（比如‘SX20190510’）作为参数 key 的值，以得到一组序列号填入业务码。");
            Console.WriteLine();

            Console.WriteLine("Increment 类，可以在服务端使用也可以在客户端使用。");
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

            Console.WriteLine("启动 3 个线程分别读取 2 个 key 的 Increment：");
            Task[] tasks = new[]
            {
                Task.Run(() => FetchIncrement(1)),
                Task.Run(() => FetchIncrement(2)),
                Task.Run(() => FetchIncrement(3))
            };
            Task.WaitAll(tasks);
            foreach (KeyValuePair<string, int> kvp in _sequenceValues)
            {
                Console.Write("business code = {0}, index = {1}", kvp.Key, kvp.Value);
                Console.WriteLine();
            }

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }

        static readonly SynchronizedSortedDictionary<string, int> _sequenceValues = new SynchronizedSortedDictionary<string, int> ();

        static void FetchIncrement(int index)
        {
            Task[] tasks = new[]
            {
                Task.Run(() => FetchIncrement("SX20190510A", 0, index)),
                Task.Run(() => FetchIncrement("SX20190510B", 1, index)),
            };
            Task.WaitAll(tasks);
        }

        static void FetchIncrement(string key, long initialValue, int index)
        {
            for (int i = 0; i < 10; i++)
            {
                _sequenceValues.Add(String.Format("{0}-{1:D6}", key, Phenix.Core.Data.Increment.GetNext(key, initialValue)), index);
            }
        }
    }
}
