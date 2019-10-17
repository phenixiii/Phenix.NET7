using System;
using System.Collections.Generic;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;
using Phenix.Core.Reflection;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Core.Data.Model.DynamicEntity 功能 ****");
            Console.WriteLine();
            Console.WriteLine("DynamicEntity 类，为系统开发封装了 System.Dynamic.DynamicObject，以便于用动态语法操作“属性名-属性值”键值队列的记录数据。");
            Console.WriteLine("你可以直接用 IDictionary<string, object>、IList<IDictionary<string, object>> 类型的 propertyValues 参数 Fetch 出 DynamicEntity、IList<DynamicEntity> 对象");
            Console.WriteLine("(如需传入JSON格式请先调用 Phenix.Core.Reflection.Utilities.JsonDeserialize<IDictionary<string, object>>(propertyValues)、Phenix.Core.Reflection.Utilities.JsonDeserialize<IList<IDictionary<string, object>>>(propertyValues) 进行转换)，");
            Console.WriteLine("也可以附带或仅仅传入 Sheet 对象，实现对指定表（或是视图时仅针对第一个属性映射的表）的增删改操作，而且动态属性的可操作范围能够扩展到 Sheet.Columns 所映射的所有属性名。");
            Console.WriteLine("使用动态对象，减少了不必要的类型定义，增加了程序代码的灵活性，但本质上和操作 IDictionary<string, object> 对象是没什么区别的，大量使用会带来一些副作用，比如代码可维护性问题，应避免滥用。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("设为调试状态");
            AppRun.Debugging = true;
            Console.WriteLine("测试过程中产生的日志保存在：" + AppRun.TempDirectory);
            Console.WriteLine();

            Console.WriteLine("注册缺省数据库连接");
            Database.RegisterDefault("192.168.248.52", "TEST", "SHBPMO", "SHBPMO");
            Console.WriteLine("数据库连接串 = {0}", Database.Default.ConnectionString);
            Console.WriteLine("请确认连接的是否是你的测试库？如不符，请退出程序修改 Database.RegisterDefault 部分代码段。");
            Console.Write("否则按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Sheet positionsSheet = Database.Default.MetaData.FindTable("ph7_position");
            long positionId = Sequence.Value;

            Position position = new Position(positionId, "企业组织架构管理员", "组织架构管理,岗位管理,组员管理");
            position.InsertSelf();
            Console.WriteLine("先准备一条演示用的岗位记录：{0}", Utilities.JsonSerialize(position));
            string positionsJson = positionsSheet.SelectRecord<Position>(p => p.Id == positionId);
            Console.WriteLine("获取刚保存的岗位记录，不构建实体对象，直接返回的是JSON格式字符串：{0}", positionsJson);
            IList<IDictionary<string, object>> positionsDictionary = Utilities.JsonDeserialize<IList<IDictionary<string, object>>>(positionsJson);
            Console.WriteLine("JSON格式字符串反序列化为“属性名-属性值”键值队列的数组对象：{0}", Utilities.JsonSerialize(positionsDictionary));
            string positionJson = positionsSheet.SelectRecord<Position>(p => p.Id == positionId, true);
            Console.WriteLine("也可以获取单条记录，不构建实体对象，直接返回的是JSON格式字符串：{0}", positionJson);
            IDictionary<string, object> positionDictionary = Utilities.JsonDeserialize<IDictionary<string, object>>(positionJson);
            Console.WriteLine("JSON格式字符串反序列化为“属性名-属性值”键值队列对象：{0}", Utilities.JsonSerialize(positionDictionary));
            Console.WriteLine("数据准备完毕");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示构建动态岗位对象数组");
            IList<DynamicEntity> positionsDynamic = DynamicEntity.FetchList(positionsDictionary);
            foreach (dynamic item in positionsDynamic)
            {
                Console.WriteLine();
                Console.WriteLine("item.Id = {0}", item.Id);
                Console.WriteLine("item.Name = {0}", item.Name);
                Console.WriteLine("item.Roles = {0}", item.Roles);
                Console.WriteLine();
            }
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示构建动态岗位对象");
            dynamic positionDynamic = DynamicEntity.Fetch(positionDictionary);
            Console.WriteLine();
            Console.WriteLine("positionDynamic.Id = {0}", positionDynamic.Id);
            Console.WriteLine("positionDynamic.Name = {0}", positionDynamic.Name);
            Console.WriteLine("positionDynamic.Roles = {0}", positionDynamic.Roles);
            Console.WriteLine();
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示动态岗位对象的增删改操作，注意传入了指向 ph7_position 表的 Sheet 对象以执行增删改操作");
            positionDynamic = DynamicEntity.Fetch(positionDictionary, positionsSheet);
            positionDynamic.Roles = "岗位管理,组员管理";
            Console.WriteLine("赋值 Roles 属性：{0}", positionDynamic.Roles);
            ((DynamicEntity) positionDynamic).UpdateRecord();
            Console.WriteLine("可更新到数据库：{0}", positionsSheet.SelectRecord<Position>(p => p.Id == positionId));
            ((DynamicEntity) positionDynamic).DeleteRecord();
            Console.WriteLine("可彻底删除记录：{0}", positionsSheet.SelectRecord<Position>(p => p.Id == positionId));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("演示直接用指向 ph7_position 表的 Sheet 对象构建动态岗位对象");
            positionDynamic = DynamicEntity.Fetch(positionsSheet);
            positionDynamic.Id = positionId;
            positionDynamic.Name = "企业组织架构管理员";
            positionDynamic.Roles = "组织架构管理,岗位管理,组员管理";
            Console.WriteLine();
            Console.WriteLine("positionDynamic.Id = {0}", positionDynamic.Id);
            Console.WriteLine("positionDynamic.Name = {0}", positionDynamic.Name);
            Console.WriteLine("positionDynamic.Roles = {0}", positionDynamic.Roles);
            Console.WriteLine();
            ((DynamicEntity)positionDynamic).InsertRecord();
            Console.WriteLine("可添加记录到数据库：{0}", positionsSheet.SelectRecord<Position>(p => p.Id == positionId));
            ((DynamicEntity)positionDynamic).DeleteRecord();
            Console.WriteLine("再次彻底删除记录：{0}", positionsSheet.SelectRecord<Position>(p => p.Id == positionId));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
