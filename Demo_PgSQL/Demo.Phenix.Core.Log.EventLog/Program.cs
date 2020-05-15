using System;
using System.Data;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Log;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Log.EventLog 功能 ****");
            Console.WriteLine();
            Console.WriteLine("EventLog 类，为系统提供在本地磁盘或在数据库里存储日志的方法。");
            Console.WriteLine("本地日志文件存储在 {0} 目录下按日期命名的子目录 {1} 里。", AppRun.TempDirectory, EventLog.LocalDirectory);
            Console.WriteLine("数据库日志存储在缺省数据库的 PH7_EventLog 表记录里。");
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

            string message = "我是一条日志";
            Console.WriteLine("先准备一条演示用的日志 = '{0}'", message);
            Console.WriteLine();

            Console.WriteLine("演示存储到本地磁盘的方法 SaveLocal()");
            EventLog.SaveLocal(message);
            System.Diagnostics.Process.Start("explorer.exe", EventLog.LocalDirectory);
            Console.WriteLine("仅传了 string message 参数，你可在此基础上，自行编码体验不同的写法。");
            Console.WriteLine("除了传 string message 参数外，还可以传 MethodBase 参数，以帮助自己在排查问题时定位发出消息的对象、所处的类方法。");
            Console.WriteLine("如果传入 Exception 参数，会被层层摘取出 InnerException 的 Message 内容，拼接后记录到日志里。");
            Console.WriteLine("如果传入 string extension 参数，可指定日志文件的后缀名，默认是 'log'。");
            Console.WriteLine("本地日志文件，默认存储在操作系统的 TEMP 目录里，所以有可能会被操作系统定时清理掉，请做好系统配置以保有足够的留存周期。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示存储到数据库的方法 Save()");
            EventLog.Save(message);
            Console.WriteLine("仅传了 string message 参数，还可以传 Exception、string extension 参数。");
            Console.WriteLine("日志保存在 PH7_EventLog 表里：");
            using (DataReader reader = Database.Default.CreateDataReader(@"
select *
from PH7_EventLog
order by EL_ID desc", CommandBehavior.SingleRow))
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                        Console.Write("{0} = {1}, ", reader.GetName(i), reader.GetValue(i) ?? "null");
                    Console.WriteLine();
                }
                else
                    Console.WriteLine("error：未找到日志！");
            }
            Console.WriteLine("默认下，每月月底会清理一次 PH7_EventLog 表里 {0} 月前的日志记录。", EventLog.ClearLogDeferMonths);
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
