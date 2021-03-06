﻿using System;
using System.Collections.Generic;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Reflection;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Business.UndoableBase 功能 ****");
            Console.WriteLine();
            Console.WriteLine("UndoableBase 类，继承自 EntityBase 类，是所有 XXXBusinessBase 的基类，为业务对象的编辑操作提供单级回滚的功能。");
            Console.WriteLine("触发编辑操作的函数为BeginEdit()，与之成对使用（结束编辑状态）的CancelEdit()函数可取消编辑/ApplyEdit()函数可应用编辑，Save()函数可将编辑结果做持久化。");
            Console.WriteLine("调用BeginEdit()函数后，实体对象的 IsSelfDirty 状态变更为 true，IsFetched 状态变更为 false，IsNew 和 IsSelfDeleted 状态不变，当前数据被留存一份快照。");
            Console.WriteLine("调用CancelEdit()函数后，实体对象的 IsSelfDirty 状态变更为 false，如果 IsFetched 原状态为 true 则恢复为 true，IsNew 和 IsSelfDeleted 状态不变，当前数据被刷回快照。");
            Console.WriteLine("调用Save()函数或ApplyEdit()函数后，如果 IsSelfDeleted 状态为 false，则 IsSelfDirty 和 IsNew 状态都变更为 false，IsFetched 状态变更为 true，快照被丢弃。");
            Console.WriteLine("可见，将 IsSelfDirty 属性设置为 true 相当于调用BeginEdit()函数，将 IsSelfDirty 属性设置为 false 相当于调用CancelEdit()函数。");
            Console.WriteLine("实体对象的 IsNew 和 IsSelfDeleted 属性也是可以设置的，但是独立于编辑操作的一套逻辑，当它们为 true 时，如果最终目的是 Save 的话，期间做任何的编辑操作实质上都是没有任何意义的。");
            Console.WriteLine("设置 IsNew 属性为 true，实体对象的相关字段会被自动初始化：");
            Console.WriteLine("   字段映射的是“_ID”后缀且是长整型15位以上精度的主键：初始化为Sequence.Default.Value；");
            Console.WriteLine("   字段映射的是“_ORIGINATOR”且是字符串/长整型15位以上精度：初始化为Identity.CurrentIdentity.User.Name/Id；");
            Console.WriteLine("   字段映射的是“_ORIGINATE_TIME”且是DateTime：初始化为当前时间；");
            Console.WriteLine("   字段映射的是“_ORIGINATE_TEAMS”且是字符串/长整型15位以上精度：初始化为Identity.CurrentIdentity.User.RootTeams.Name/Id，可用于SaaS模式下的系统开发，对不同团体的数据进行切片；");
            Console.WriteLine("   字段映射的是“_UPDATER”：且是字符串/长整型15位以上精度：初始化为Identity.CurrentIdentity.User.Name/Id；");
            Console.WriteLine("   字段映射的是“_UPDATE_TIME”且是DateTime：初始化为当前时间；");
            Console.WriteLine("   字段映射的是“_TIMESTAMP”且是长整型15位以上精度：初始化为Sequence.Default.Value；");
            Console.WriteLine("以上后3个字段，在 IsSelfDirty 属性为 true 时也会被自动初始化一次。");
            Console.WriteLine("新增一个实体对象，应该调用工厂函数New()而不是直接 new 一个对象，因为工厂函数里会自动将 IsNew 属性设置为 true（当然，你也可以 new 一个对象出来后手动赋值 IsNew 为 true）。");
            Console.WriteLine("实体对象的 IsSelfDirty、IsNew 和 IsSelfDeleted 属性之间是没有逻辑联动的，可以有不同的组合状态，所以在 Save 时：");
            Console.WriteLine("   当 IsNew 为 true 且 IsSelfDeleted 为 false 时，才会按照新增实体的逻辑进行持久化；");
            Console.WriteLine("   当 IsSelfDirty 为 true 且 IsSelfDeleted 为 false 时，才会按照更新实体的逻辑进行持久化；");
            Console.WriteLine("   当 IsSelfDeleted 为 true 且 IsNew 为 false 时，才会按照删除实体的逻辑进行持久化；");
            Console.WriteLine("除以上组合状态之外，调用Save()函数不会发生持久化动作。");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
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

            Console.WriteLine("开始演示");
            Position position = Position.New(Database.Default);
            position.Name = "公司管理员";
            position.Roles = new string[]
            {
                "组织架构管理", "岗位管理", "组员管理"
            };
            Console.WriteLine("新增 Position 对象：{0}", Utilities.JsonSerialize(position));
            Console.WriteLine("状态：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                position.IsNew && !position.IsSelfDirty && !position.IsSelfDeleted && !position.IsFetched ? "ok" : "error");
            position.SaveSelf();
            Console.WriteLine("提交后：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                !position.IsNew && !position.IsSelfDirty && !position.IsSelfDeleted && position.IsFetched ? "ok" : "error");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            long id = position.Id;

            position = Position.FetchRoot(Database.Default, p => p.Id == id); 
            Console.WriteLine("获取 Position 对象：{0}", Utilities.JsonSerialize(position));
            Console.WriteLine("状态：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                !position.IsNew && !position.IsSelfDirty && !position.IsSelfDeleted && position.IsFetched ? "ok" : "error");
            position.BeginEdit();
            Console.WriteLine("编辑时：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                !position.IsNew && position.IsSelfDirty && !position.IsSelfDeleted && !position.IsFetched ? "ok" : "error");
            List<string> roles = new List<string>(position.Roles);
            roles.RemoveAt(2);
            position.Roles = roles;
            Console.WriteLine("赋值 Roles 属性为 '{0}'，旧值为 '{1}'", Utilities.JsonSerialize(position.Roles), Utilities.JsonSerialize(position.GetOldValue(p => p.Roles)));
            position.CancelEdit();
            Console.WriteLine("取消编辑后：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                !position.IsNew && !position.IsSelfDirty && !position.IsSelfDeleted && position.IsFetched ? "ok" : "error");
            Console.WriteLine("恢复 Roles 属性为 '{0}'", Utilities.JsonSerialize(position.Roles));
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            position.BeginEdit();
            Console.WriteLine("再次编辑时：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                !position.IsNew && position.IsSelfDirty && !position.IsSelfDeleted && !position.IsFetched ? "ok" : "error");
            roles = new List<string>(position.Roles);
            roles.RemoveAt(2);
            position.Roles = roles;
            Console.WriteLine("赋值 Roles 属性为 '{0}'，旧值为 '{1}'", Utilities.JsonSerialize(position.Roles), Utilities.JsonSerialize(position.GetOldValue(p => p.Roles)));
            position.SaveSelf();
            Console.WriteLine("提交后：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                !position.IsNew && !position.IsSelfDirty && !position.IsSelfDeleted && position.IsFetched ? "ok" : "error");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            position.IsSelfDeleted = true;
            Console.WriteLine("直接操作删除状态 IsSelfDeleted 属性为 true 后：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                !position.IsNew && !position.IsSelfDirty && position.IsSelfDeleted && !position.IsFetched ? "ok" : "error");
            position.SaveSelf();
            Console.WriteLine("提交后：IsNew = {0}，IsSelfDirty = {1}，IsSelfDeleted = {2}，IsFetched = {3}，{4}",
                position.IsNew, position.IsSelfDirty, position.IsSelfDeleted, position.IsFetched,
                !position.IsNew && !position.IsSelfDirty && position.IsSelfDeleted && !position.IsFetched ? "ok" : "error");
            Console.WriteLine("检索数据库，已被删除：{0}", Position.FetchRoot(Database.Default, p => p.Id == id) == null ? "ok" : "error");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
