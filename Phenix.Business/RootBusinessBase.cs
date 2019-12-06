using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Validity;

namespace Phenix.Business
{
    /// <summary>
    /// 根业务基类
    /// </summary>
    [Serializable]
    public abstract class RootBusinessBase<T> : BusinessBase<T>, IRootEntity
        where T : RootBusinessBase<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected RootBusinessBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected RootBusinessBase(bool? isNew, bool? isSelfDeleted, bool? isSelfDirty,
            IDictionary<string, object> oldPropertyValues, IDictionary<string, bool?> dirtyPropertyNames,
            long id)
            : base(isNew, isSelfDeleted, isSelfDirty, oldPropertyValues, dirtyPropertyNames)
        {
            _id = id;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected RootBusinessBase(long id)
        {
            _id = id;
        }

        #region 工厂

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="doCreate">如果没有该ID的业务对象则调用本函数新增并自动持久化</param>
        public static T Fetch(long id, Func<Task<T>> doCreate = null)
        {
            return Fetch(p => p.Id == id, doCreate);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="id">ID</param>
        /// <param name="doCreate">如果没有该ID的业务对象则调用本函数新增并自动持久化</param>
        public static T Fetch(DbConnection connection, long id, Func<Task<T>> doCreate = null)
        {
            return Fetch(connection, p => p.Id == id, doCreate);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="id">ID</param>
        /// <param name="doCreate">如果没有该ID的业务对象则调用本函数新增并自动持久化</param>
        public static T Fetch(DbTransaction transaction, long id, Func<Task<T>> doCreate = null)
        {
            return Fetch(transaction, p => p.Id == id, doCreate);
        }

        #endregion

        #region 属性

        /// <summary>
        /// ID
        /// </summary>
        protected long _id;

        /// <summary>
        /// ID(映射表XX_ID字段)
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        /// <summary>
        /// 主键表达式(null代表增删改持久化时遍历类遇到的第一个主键属性)
        /// </summary>
        protected override Expression<Func<T, object>> PrimaryKeyPropertyLambda
        {
            get { return p => p.Id; }
        }

        #endregion

        #region 方法

        #region DeleteSelf

        /// <summary>
        /// 为DeleteSelf()函数执行时追加检查是否存在关联关系的外键条件表达式
        /// </summary>
        protected virtual CriteriaExpression AppendAssociationLambda(CriteriaExpression criteriaExpressionForDeleteSelf)
        {
            return criteriaExpressionForDeleteSelf;
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public override int DeleteSelf(DbTransaction transaction, bool cascade = false)
        {
            CriteriaExpression criteriaExpressionForDeleteSelf = CriteriaExpression.Where<T>(p => p.Id == Id);
            CriteriaExpression criteriaExpression = AppendAssociationLambda(criteriaExpressionForDeleteSelf);
            if (object.ReferenceEquals(criteriaExpression, criteriaExpressionForDeleteSelf))
                return base.DeleteSelf(transaction, cascade);
            if (cascade)
                throw new ArgumentException("追加检查关联关系时不允许级联删除!", nameof(cascade));
            int result = Sheet.DeleteRecord(transaction, criteriaExpression);
            if (result == 0)
                throw new AssociationDataException();
            SaveRenovateLog(transaction, ExecuteAction.Delete);
            return result;
        }

        #endregion

        #region Renovator

        /// <summary>
        /// 添加动态刷新触发器
        /// </summary>
        /// <param name="asyncAction">异步动作(表名,表主键值,执行时间,执行动作)</param>
        public static void AddRenovatorTrigger(Action<string, long, DateTime, ExecuteAction> asyncAction)
        {
            AddRenovatorTrigger(p => p.Id, asyncAction);
        }

        /// <summary>
        /// 保存表记录更新日志
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="executeAction">执行动作</param>
        public void SaveRenovateLog(DbTransaction transaction, ExecuteAction executeAction)
        {
            SaveRenovateLog(transaction, executeAction, DateTime.Now);
        }

        /// <summary>
        /// 保存表记录更新日志
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="executeTime">执行时间</param>
        /// <param name="executeAction">执行动作</param>
        public virtual void SaveRenovateLog(DbTransaction transaction, ExecuteAction executeAction, DateTime executeTime)
        {
            SaveRenovateLog(transaction, p => p.Id, executeAction, executeTime);
        }

        #endregion

        #endregion
    }
}