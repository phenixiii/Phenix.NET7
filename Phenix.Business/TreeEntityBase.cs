using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using Phenix.Core.Data;
using Phenix.Mapper;
using Phenix.Mapper.Expressions;
using Phenix.Core.SyncCollections;

namespace Phenix.Business
{
    /// <summary>
    /// 树实体基类
    /// </summary>
    [Serializable]
    public abstract class TreeEntityBase<T> : EntityBase<T>
        where T : TreeEntityBase<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected TreeEntityBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected TreeEntityBase(string dataSourceKey, long id, long rootId, long parentId, T[] children)
            : base(dataSourceKey)
        {
            _id = id;
            _rootId = rootId;
            _parentId = parentId;
            _children = children != null ? new List<T>(children) : new List<T>();
        }

        #region 工厂

        #region NewRoot

        /// <summary>
        /// 新增根实体对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        /// <returns>根实体对象</returns>
        public static T NewRoot(Database database, params NameValue<T>[] propertyValues)
        {
            return NewRoot(database, NameValue<T>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 新增根实体对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        /// <returns>根实体对象</returns>
        public static T NewRoot(Database database, IDictionary<string, object> propertyValues)
        {
            T result = New(database, propertyValues);
            result._rootId = result._id;
            result._parentId = result._id;
            return result;
        }

        /// <summary>
        /// 新增根实体对象(自动填充保留字段)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="id">主键值</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        /// <returns>根实体对象</returns>
        public static T NewRoot(Database database, long id, params NameValue<T>[] propertyValues)
        {
            return NewRoot(database, id, NameValue<T>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 新增根实体对象(自动填充保留字段)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="id">主键值</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        /// <returns>根实体对象</returns>
        public static T NewRoot(Database database, long id, IDictionary<string, object> propertyValues)
        {
            T result = New(database, id, propertyValues);
            result._rootId = result._id;
            result._parentId = result._id;
            return result;
        }

        #endregion

        #region FetchTree

        private static T FillAllChildren(T root)
        {
            if (root == null)
                return null;

            SynchronizedDictionary<long, List<T>> allChildren = new SynchronizedDictionary<long, List<T>>();
            foreach (T item in root.SelfSheet.SelectEntity<T>(p => p.RootId == root.Id && p.Id != root.Id))
                allChildren.GetValue(item.ParentId, () => new List<T>()).Add(item);
            FillChildren(root, allChildren);
            return root;
        }

        private static void FillChildren(T parent, IDictionary<long, List<T>> allChildren)
        {
            if (allChildren.TryGetValue(parent.Id, out List<T> children))
            {
                parent._children.AddRange(children);
                foreach (T child in children)
                    FillChildren(child, allChildren);
            }
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(Database database, Expression<Func<T, bool>> criteriaLambda, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(database, criteriaLambda, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(Database database, Expression<Func<T, bool>> criteriaLambda, object criteria, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(database, criteriaLambda, criteria, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(Database database, CriteriaExpression criteriaExpression, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(database, criteriaExpression, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(Database database, CriteriaExpression criteriaExpression, object criteria, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(database, criteriaExpression, criteria, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(connection, criteriaLambda, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(connection, criteriaLambda, criteria, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbConnection connection, CriteriaExpression criteriaExpression, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(connection, criteriaExpression, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(connection, criteriaExpression, criteria, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(transaction, criteriaLambda, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(transaction, criteriaLambda, criteria, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbTransaction transaction, CriteriaExpression criteriaExpression, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(transaction, criteriaExpression, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, Func<T> doCreate = null)
        {
            return FillAllChildren(FetchRoot(transaction, criteriaExpression, criteria, doCreate));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(Database database, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(database, criteriaLambda, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(Database database, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(database, criteriaLambda, criteria, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(Database database, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(database, criteriaExpression, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(Database database, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(database, criteriaExpression, criteria, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(connection, criteriaLambda, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(connection, criteriaLambda, criteria, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbConnection connection, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(connection, criteriaExpression, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(connection, criteriaExpression, criteria, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(transaction, criteriaLambda, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(transaction, criteriaLambda, criteria, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbTransaction transaction, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(transaction, criteriaExpression, orderBys));
        }

        /// <summary>
        /// 获取整颗树
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchTree(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            return FillAllChildren(FetchRoot(transaction, criteriaExpression, criteria, orderBys));
        }

        #endregion

        #endregion

        #region 属性

        /// <summary>
        /// 主键属性(映射表ID字段)
        /// </summary>
        protected long _id;

        /// <summary>
        /// 主键属性(映射表ID字段)
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        /// <summary>
        /// 根节点ID(映射表Root_ID字段)
        /// </summary>
        protected long _rootId;

        /// <summary>
        /// 根节点ID(映射表Root_ID字段)
        /// </summary>
        public long RootId
        {
            get { return _rootId; }
        }

        /// <summary>
        /// 父节点ID(映射表Parent_ID字段)
        /// </summary>
        protected long _parentId;

        /// <summary>
        /// 父节点ID(映射表Parent_ID字段)
        /// </summary>
        public long ParentId
        {
            get { return _parentId; }
        }

        /// <summary>
        /// 主实体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public override IEntity Master
        {
            get { return (T) base.Master ?? this; }
            protected set
            {
                if (value != null && FindInBranch(p => p.Id == ((T) value).Id) != null)
                    throw new ArgumentException("不允许倒挂在自己或儿孙节点下", nameof(value));

                T parent = (T) base.Master;
                if (parent != null && parent != this)
                    parent._children.Remove((T) this);

                T newParent = (T) (value ?? this);
                base.Master = newParent;
                SelfSheet = newParent.SelfSheet;
                newParent._children.Add((T) this);
                _rootId = value != null ? newParent._rootId : _id;
                _parentId = newParent._id;
                foreach (T item in _children)
                    item.Master = this;
            }
        }

        /// <summary>
        /// 父实体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public T Parent
        {
            get { return (T) Master; }
            protected set { Master = value; }
        }

        /// <summary>
        /// 儿孙
        /// </summary>
        protected List<T> _children = new List<T>();

        /// <summary>
        /// 儿孙
        /// </summary>
        public T[] Children
        {
            get { return _children.ToArray(); }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 在本枝杈中寻找
        /// </summary>
        /// <param name="criteria">匹配条件(节点,是否匹配)</param>
        /// <returns>节点</returns>
        public virtual T FindInBranch(Func<T, bool> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return DoFindInBranch(criteria);
        }

        private T DoFindInBranch(Func<T, bool> criteria)
        {
            if (criteria((T) this))
                return (T) this;
            foreach (T item in _children)
            {
                T result = item.DoFindInBranch(criteria);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// 添加子对象
        /// </summary>
        /// <param name="doCreate">调用本函数新增(子节点)</param>
        /// <returns>子节点</returns>
        public virtual T AddChild(Func<T> doCreate)
        {
            if (doCreate == null)
                throw new ArgumentNullException(nameof(doCreate));

            T result = doCreate();
            if (result != null)
            {
                result.Parent = (T) this;
                result.InsertSelf();
            }

            return result;
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="newParent">父节点</param>
        public virtual void ChangeParent(T newParent)
        {
            if (newParent == null)
                throw new ArgumentNullException(nameof(newParent));

            T parent = Parent;
            try
            {
                Parent = newParent;
                UpdateSelf(Set(p => p.ParentId, newParent.Id));
            }
            catch
            {
                Parent = parent;
                throw;
            }
        }

        /// <summary>
        /// 删除本枝杈
        /// </summary>
        /// <returns>更新记录数</returns>
        public virtual int DeleteBranch()
        {
            if (IsRoot)
                if (_children.Count == 0)
                    return DeleteSelf();
                else
                    throw new InvalidOperationException("不允许删除整棵树");

            T parent = Parent;
            try
            {
                Parent = null;
                return Database.ExecuteGet(DoDeleteBranch);
            }
            catch
            {
                Parent = parent;
                throw;
            }
        }

        private int DoDeleteBranch(DbTransaction transaction)
        {
            int result = 0;
            foreach (T item in _children)
                result = result + item.DoDeleteBranch(transaction);
            return result + DeleteSelf(transaction);
        }

        #endregion
    }
}