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
            Console.WriteLine("Sequence 类，封装了运行在 Orleans 服务群的 SequenceGrain 的64位序号生成器。");
            Console.WriteLine("每次调用 Sequence 对象的 Value 属性，将获得同一 index 下不会重复（但不保证连续）的15位 Long 类型数值，可作为业务对象（业务表）ID 值的填充来源。");
            Console.WriteLine();

            Console.WriteLine("本场景是运行在服务端，如果要在客户端获取64位增量，请使用 phAjax.getSequence() / Phenix.Client.HttpClient.GetSequenceAsync()。");
            Console.WriteLine();

            Console.WriteLine("在接下来的演示之前，请启动 Phenix.Services.Host 程序，并保证其与本程序的 Phenix.Core.db 的缺省数据库配置信息是一致的。");
            Console.WriteLine("数据库配置信息存放在 Phenix.Core.db 的 PH7_Database 表中，配置方法见其示例记录的 Remark 字段内容。");
            Console.Write("准备好之后，请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("设为调试状态");
            AppRun.Debugging = true;
            Console.WriteLine("测试过程中产生的日志保存在：" + AppRun.TempDirectory);
            Console.WriteLine();

            Console.WriteLine("注册缺省数据库连接（也可以在 Phenix.Core.db 的 PH7_Database 表中配置）");
            Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
            Console.WriteLine("数据库连接串 = {0}", Database.Default.ConnectionString);
            Console.WriteLine("请确认是否是你的测试库（并保证与 Phenix.Services.Host 程序连接的缺省数据库是同一个）？如不符，请退出程序修改 Database.RegisterDefault 部分代码段。");
            Console.Write("否则按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("启动3个线程分别读取不同 index 下 Sequence 的值，参数 index 随机产生，范围在0~999之间：");
            Task[] tasks = new[]
            {
                Task.Run(() => FetchSequence(new Random().Next(0, 1000))),
                Task.Run(() => FetchSequence(new Random().Next(0, 1000))),
                Task.Run(() => FetchSequence(new Random().Next(0, 1000)))
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
            Phenix.Core.Data.Sequence sequence = Phenix.Core.Data.Sequence.Fetch(index);
            for (int i = 0; i < 10; i++)
                _sequenceValues.Add(sequence.Value.Result, index);
        }
    }
}
