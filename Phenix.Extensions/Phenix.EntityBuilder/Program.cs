using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Data.Schema;

namespace Phenix.EntityBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 实体类代码生成工具 ****");
            Console.WriteLine();

            string dataSource;
            string databaseName;
            string userId;
            string password;
            if (args.Length == 4)
            {
                dataSource = args[0];
                databaseName = args[1];
                userId = args[2];
                password = args[3];
            }
            else
                while (true)
                {
                    Console.WriteLine("请按照提示，输入需映射到实体对象的数据库的连接串...");
                    Console.Write("dataSource（数据源，示例'192.168.248.52'）：");
                    dataSource = Console.ReadLine();
                    Console.Write("databaseName（数据库名称，示例'TEST'）：");
                    databaseName = Console.ReadLine();
                    Console.Write("userId（用户ID，示例'SHBPMO'）：");
                    userId = Console.ReadLine();
                    Console.Write("password（用户口令，示例'SHBPMO'）：");
                    password = Console.ReadLine();
                    Console.Write("以上是否正确(Y/N)：");
                    if (String.Compare(Console.ReadKey().KeyChar.ToString(), "Y", StringComparison.OrdinalIgnoreCase) == 0)
                        break;
                    Console.WriteLine();
                }

            Console.WriteLine();
            Console.WriteLine("如需Class名称取自被整理的表名(如果第4位是“_”则剔去其及之前的字符)，请设置Phenix.Core.Data.Schema.Table.ClassNameByTrimTableName属性，默认是{0}；", Phenix.Core.Data.Schema.Table.ClassNameByTrimTableName);
            Console.WriteLine("如需Class名称取自被整理的视图名(如果第4位是“_”则剔去其及之前的字符, 如果倒数第2位是“_”则剔去其及之后的字符)，请设置Phenix.Core.Data.Schema.View.ClassNameByTrimViewName属性，默认是{0}；", Phenix.Core.Data.Schema.View.ClassNameByTrimViewName);
            Console.WriteLine();
            string baseDirectory = Path.Combine(AppRun.BaseDirectory, DateTime.Now.ToString("yyyyMMddHHmm"));
            Console.WriteLine("生成的实体类文件将存放在目录：{0}", baseDirectory);
            Console.WriteLine("你可以根据开发需要，摘取文件到自己的项目工程目录中。");
            Console.WriteLine();

            try
            {
                Database database = Database.RegisterDefault(dataSource, databaseName, userId, password);
                Console.Write("是否遍历{0}数据库的表，生成实体类代码(Y/N)：", database.DatabaseName);
                if (String.Compare(Console.ReadKey().KeyChar.ToString(), "Y", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Building...");
                    foreach (KeyValuePair<string, Table> kvp in database.MetaData.Tables)
                        Console.WriteLine(BuildClass(kvp.Value, baseDirectory));
                    Console.WriteLine();
                }

                Console.Write("是否遍历{0}数据库的视图，生成实体类代码(Y/N)：", database.DatabaseName);
                if (String.Compare(Console.ReadKey().KeyChar.ToString(), "Y", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Building...");
                    foreach (KeyValuePair<string, View> kvp in database.MetaData.Views)
                        Console.WriteLine(BuildClass(kvp.Value, baseDirectory));
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生异常，中断代码生成过程: " + AppRun.GetErrorMessage(ex));
                Console.Write("请按回车键结束程序");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("完成代码生成。");
            System.Diagnostics.Process.Start("Explorer.exe", baseDirectory);
            Console.Write("请按回车键结束程序");
            Console.ReadLine();
            Console.WriteLine();
        }

        private static string BuildClass(Sheet sheet, string baseDirectory)
        {
            string directory = !String.IsNullOrEmpty(sheet.Prefix) ? Path.Combine(baseDirectory, sheet.Prefix.ToUpper()) : baseDirectory;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, sheet.ClassName + ".cs");

            StringBuilder code = new StringBuilder("using System;");
            code.Append(String.Format(@"
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Validity;
using Phenix.Core.Log;
using Phenix.Core.SyncCollections;

/* 
   builder:    {0}
   build time: {1}
   mapping to: {2}
*/

namespace {3}
{{
    /// <summary>
    /// {4}
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = ""{4}"")]
    public class {5} : EntityBase<{5}>
    {{
        private {5}()
        {{
            // used to fetch object, do not add code
        }}

        [Newtonsoft.Json.JsonConstructor]
        public {5}(",
                Environment.UserName, DateTime.Now, sheet.Name, sheet.Owner.Database.DatabaseName, sheet.Description, sheet.ClassName));


            foreach (KeyValuePair<string, Column> kvp in sheet.Columns)
                code.Append(String.Format("{0} {1}, ", kvp.Value.FieldTypeName, kvp.Value.ParameterName));
            code[code.Length - 2] = ')';

            code.Append(@"
        {");
            foreach (KeyValuePair<string, Column> kvp in sheet.Columns)
                code.Append(String.Format(@"
            {0} = {1};",
                    kvp.Value.FieldName, kvp.Value.ParameterName));
            code.Append(@"
        }
");

            foreach (KeyValuePair<string, Column> kvp in sheet.Columns)
            {
                code.Append(String.Format(@"
        private {0} {1};
        /// <summary>
        /// {2}
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = ""{2}"")]
        public {0} {3}
        {{
            get {{ return {1}; }}
            set {{ {1} = value; }}
        }}
",
                    kvp.Value.FieldTypeName, kvp.Value.FieldName, kvp.Value.Description, kvp.Value.PropertyName));
            }

            code.Append(@"
    }
}
");

            using (StreamWriter writer = File.CreateText(filePath))
            {
                writer.Write(code.ToString());
                writer.Flush();
            }

            return filePath;
        }
    }
}