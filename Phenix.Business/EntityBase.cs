using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Phenix.Core.Data;
using Phenix.Mapper;
using Phenix.Mapper.Expressions;
using Phenix.Mapper.Rule;
using Phenix.Mapper.Schema;
using Phenix.Core.Reflection;
using Phenix.Core.SyncCollections;

namespace Phenix.Business
{
    /// <summary>
    /// 实体基类
    /// </summary>
    [Serializable]
    public abstract class EntityBase<T> : IEntity<T>
        where T : EntityBase<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected EntityBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected EntityBase(string dataSourceKey)
        {
            _dataSourceKey = dataSourceKey;
        }

        #region 工厂

        #region New

        /// <summary>
        /// 新增实体对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        /// <returns>实体对象</returns>
        public static T New(Database database, params NameValue<T>[] propertyValues)
        {
            return New(database, NameValue<T>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 新增实体对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        /// <returns>实体对象</returns>
        public static T New(Database database, IDictionary<string, object> propertyValues)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            T result = DynamicInstanceFactory.Create<T>();
            result.Database = database;
            result.Apply(result.SelfSheet.FillReservedProperties(result.GetType(), propertyValues, ExecuteAction.Insert));
            result.InitializeSelf();
            return result;
        }

        /// <summary>
        /// 新增实体对象(自动填充保留字段)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="primaryKeyLong">主键值</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        /// <returns>实体对象</returns>
        public static T New(Database database, long primaryKeyLong, params NameValue<T>[] propertyValues)
        {
            return New(database, primaryKeyLong, NameValue<T>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 新增实体对象(自动填充保留字段)
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="primaryKeyLong">主键值</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        /// <returns>实体对象</returns>
        public static T New(Database database, long primaryKeyLong, IDictionary<string, object> propertyValues)
        {
            T result = New(database, propertyValues);
            result.PrimaryKeyLong = primaryKeyLong;
            return result;
        }

        #endregion

        #region FetchRoot

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(Database database, Expression<Func<T, bool>> criteriaLambda, Func<T> doCreate = null)
        {
            return FetchRoot(database, CriteriaExpression.Where(criteriaLambda), null, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(Database database, Expression<Func<T, bool>> criteriaLambda, object criteria, Func<T> doCreate = null)
        {
            return FetchRoot(database, CriteriaExpression.Where(criteriaLambda), criteria, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(Database database, CriteriaExpression criteriaExpression, Func<T> doCreate = null)
        {
            return FetchRoot(database, criteriaExpression, null, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(Database database, CriteriaExpression criteriaExpression, object criteria, Func<T> doCreate = null)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            T result = null;
            List<Task> tasks = new List<Task>(database.Handles.Count);
            foreach (KeyValuePair<int, Database> kvp in database.Handles)
            {
                Database handle = kvp.Value;
                tasks.Add(Task.Run(() =>
                {
                    Sheet sheet = MetaData.Fetch(handle).FindSheet<T>(InitializeSheet, true);
                    Label:
                    T entity = sheet.SelectFirstEntity<T>(criteriaExpression, criteria).SingleOrDefault();
                    if (entity != null)
                    {
                        entity.SelfSheet = sheet;
                        result = entity;
                    }
                    else if (doCreate != null)
                    {
                        entity = doCreate();
                        if (entity != null && object.ReferenceEquals(entity.SelfSheet, sheet))
                        {
                            try
                            {
                                entity.InsertSelf();
                                doCreate = null;
                                goto Label;
                            }
                            catch (UniqueConstraintException)
                            {
                                doCreate = null;
                                goto Label;
                            }
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            return result;
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, Func<T> doCreate = null)
        {
            return FetchRoot(connection, CriteriaExpression.Where(criteriaLambda), null, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, Func<T> doCreate = null)
        {
            return FetchRoot(connection, CriteriaExpression.Where(criteriaLambda), criteria, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbConnection connection, CriteriaExpression criteriaExpression, Func<T> doCreate = null)
        {
            return FetchRoot(connection, criteriaExpression, null, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, Func<T> doCreate = null)
        {
            Sheet sheet = MetaData.Fetch().FindSheet<T>(InitializeSheet, true);
            Label:
            T result = sheet.SelectFirstEntity<T>(connection, criteriaExpression, criteria).SingleOrDefault();
            if (result == null && doCreate != null)
            {
                result = doCreate();
                if (result != null)
                {
                    try
                    {
                        result.InsertSelf(connection);
                        doCreate = null;
                        goto Label;
                    }
                    catch (UniqueConstraintException)
                    {
                        doCreate = null;
                        goto Label;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, Func<T> doCreate = null)
        {
            return FetchRoot(transaction, CriteriaExpression.Where(criteriaLambda), null, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, Func<T> doCreate = null)
        {
            return FetchRoot(transaction, CriteriaExpression.Where(criteriaLambda), criteria, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbTransaction transaction, CriteriaExpression criteriaExpression, Func<T> doCreate = null)
        {
            return FetchRoot(transaction, criteriaExpression, null, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, Func<T> doCreate = null)
        {
            Sheet sheet = MetaData.Fetch().FindSheet<T>(InitializeSheet, true);
            Label:
            T result = sheet.SelectFirstEntity<T>(transaction, criteriaExpression, criteria).SingleOrDefault();
            if (result == null && doCreate != null)
            {
                result = doCreate();
                if (result != null)
                {
                    try
                    {
                        result.InsertSelf(transaction);
                        doCreate = null;
                        goto Label;
                    }
                    catch (UniqueConstraintException)
                    {
                        doCreate = null;
                        goto Label;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(Database database, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(database, CriteriaExpression.Where(criteriaLambda), null, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(Database database, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(database, CriteriaExpression.Where(criteriaLambda), criteria, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(Database database, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(database, criteriaExpression, null, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(Database database, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            T result = null;
            List<Task> tasks = new List<Task>(database.Handles.Count);
            foreach (KeyValuePair<int, Database> kvp in database.Handles)
            {
                Database handle = kvp.Value;
                tasks.Add(Task.Run(() =>
                {
                    Sheet sheet = MetaData.Fetch(handle).FindSheet<T>(InitializeSheet, true);
                    T entity = sheet.SelectFirstEntity(criteriaExpression, criteria, orderBys).SingleOrDefault();
                    if (entity != null)
                    {
                        entity.SelfSheet = sheet;
                        result = entity;
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            return result;
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(connection, CriteriaExpression.Where(criteriaLambda), null, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(connection, CriteriaExpression.Where(criteriaLambda), criteria, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbConnection connection, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(connection, criteriaExpression, null, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            Sheet sheet = MetaData.Fetch().FindSheet<T>(InitializeSheet, true);
            return sheet.SelectFirstEntity(connection, criteriaExpression, criteria, orderBys).SingleOrDefault();
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(transaction, CriteriaExpression.Where(criteriaLambda), null, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(transaction, CriteriaExpression.Where(criteriaLambda), criteria, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbTransaction transaction, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FetchRoot(transaction, criteriaExpression, null, orderBys);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        /// <returns>根实体对象</returns>
        public static T FetchRoot(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            Sheet sheet = MetaData.Fetch().FindSheet<T>(InitializeSheet, true);
            return sheet.SelectFirstEntity(transaction, criteriaExpression, criteria, orderBys).SingleOrDefault();
        }

        #endregion

        #region FetchKeyValues

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="keyLambda">键 lambda 表达式</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        public static IDictionary<TKey, T> FetchKeyValues<TKey>(Database database, Expression<Func<T, TKey>> keyLambda, Expression<Func<T, bool>> criteriaLambda = null, object criteria = null)
        {
            return FetchKeyValues(database, keyLambda, CriteriaExpression.Where(criteriaLambda), criteria);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="keyLambda">键 lambda 表达式</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        public static IDictionary<TKey, T> FetchKeyValues<TKey>(Database database, Expression<Func<T, TKey>> keyLambda, CriteriaExpression criteriaExpression, object criteria = null)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            SynchronizedDictionary<TKey, T> result = new SynchronizedDictionary<TKey, T>();
            List<Task> tasks = new List<Task>(database.Handles.Count);
            foreach (KeyValuePair<int, Database> handle in database.Handles)
            {
                Database value = handle.Value;
                tasks.Add(Task.Run(() =>
                {
                    Sheet sheet = MetaData.Fetch(value).FindSheet<T>(InitializeSheet, true);
                    foreach (KeyValuePair<TKey, T> keyValue in sheet.SelectEntity(keyLambda, criteriaExpression, criteria))
                    {
                        keyValue.Value.SelfSheet = sheet;
                        result.Add(keyValue.Key, keyValue.Value);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            return result;
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="keyLambda">键 lambda 表达式</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        public static IDictionary<TKey, T> FetchKeyValues<TKey>(DbConnection connection, Expression<Func<T, TKey>> keyLambda, Expression<Func<T, bool>> criteriaLambda = null, object criteria = null)
        {
            return FetchKeyValues(connection, keyLambda, CriteriaExpression.Where(criteriaLambda), criteria);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="keyLambda">键 lambda 表达式</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        public static IDictionary<TKey, T> FetchKeyValues<TKey>(DbConnection connection, Expression<Func<T, TKey>> keyLambda, CriteriaExpression criteriaExpression, object criteria = null)
        {
            Sheet sheet = MetaData.Fetch().FindSheet<T>(InitializeSheet, true);
            return sheet.SelectEntity(connection, keyLambda, criteriaExpression, criteria);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="keyLambda">键 lambda 表达式</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        public static IDictionary<TKey, T> FetchKeyValues<TKey>(DbTransaction transaction, Expression<Func<T, TKey>> keyLambda, Expression<Func<T, bool>> criteriaLambda = null, object criteria = null)
        {
            return FetchKeyValues(transaction, keyLambda, CriteriaExpression.Where(criteriaLambda), criteria);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="keyLambda">键 lambda 表达式</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        public static IDictionary<TKey, T> FetchKeyValues<TKey>(DbTransaction transaction, Expression<Func<T, TKey>> keyLambda, CriteriaExpression criteriaExpression, object criteria = null)
        {
            Sheet sheet = MetaData.Fetch().FindSheet<T>(InitializeSheet, true);
            return sheet.SelectEntity(transaction, keyLambda, criteriaExpression, criteria);
        }

        #endregion

        #region FetchList

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(Database database, params OrderBy<T>[] orderBys)
        {
            return FetchList(database, (CriteriaExpression) null, null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(Database database, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FetchList(database, CriteriaExpression.Where(criteriaLambda), null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(Database database, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FetchList(database, CriteriaExpression.Where(criteriaLambda), criteria, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(Database database, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FetchList(database, criteriaExpression, null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(Database database, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            SynchronizedList<T> result = new SynchronizedList<T>();
            List<Task> tasks = new List<Task>(database.Handles.Count);
            foreach (KeyValuePair<int, Database> handle in database.Handles)
            {
                Database value = handle.Value;
                tasks.Add(Task.Run(() =>
                {
                    Sheet sheet = MetaData.Fetch(value).FindSheet<T>(InitializeSheet, true);
                    foreach (T entity in sheet.SelectEntity(criteriaExpression, criteria, orderBys))
                    {
                        entity.SelfSheet = sheet;
                        result.Add(entity);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            return result;
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbConnection connection, params OrderBy<T>[] orderBys)
        {
            return FetchList(connection, (CriteriaExpression) null, null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FetchList(connection, CriteriaExpression.Where(criteriaLambda), null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FetchList(connection, CriteriaExpression.Where(criteriaLambda), criteria, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbConnection connection, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FetchList(connection, criteriaExpression, null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            Sheet sheet = MetaData.Fetch().FindSheet<T>(InitializeSheet, true);
            return sheet.SelectEntity(connection, criteriaExpression, criteria, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbTransaction transaction, params OrderBy<T>[] orderBys)
        {
            return FetchList(transaction, (CriteriaExpression) null, null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, params OrderBy<T>[] orderBys)
        {
            return FetchList(transaction, CriteriaExpression.Where(criteriaLambda), null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, params OrderBy<T>[] orderBys)
        {
            return FetchList(transaction, CriteriaExpression.Where(criteriaLambda), criteria, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbTransaction transaction, CriteriaExpression criteriaExpression, params OrderBy<T>[] orderBys)
        {
            return FetchList(transaction, criteriaExpression, null, orderBys);
        }

        /// <summary>
        /// 获取全部实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public static IList<T> FetchList(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, params OrderBy<T>[] orderBys)
        {
            Sheet sheet = MetaData.Fetch().FindSheet<T>(InitializeSheet, true);
            return sheet.SelectEntity(transaction, criteriaExpression, criteria, orderBys);
        }

        #endregion

        #endregion

        #region 属性

        /// <summary>
        /// 数据源键
        /// </summary>
        protected string _dataSourceKey;

        /// <summary>
        /// 数据源键
        /// </summary>
        public string DataSourceKey
        {
            get { return _dataSourceKey ??= Database.DataSourceKey; }
        }

        [NonSerialized]
        private Database _database;

        /// <summary>
        /// 数据库入口
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Database Database
        {
            get { return Master != this ? Master.Database : SelfSheet.Owner.Database; }
            set
            {
                _database = value;
                _dataSourceKey = null;
                _selfSheet = null;
                _primaryKeyProperty = null;
            }
        }

        [NonSerialized]
        private Sheet _selfSheet;

        /// <summary>
        /// 操作单子
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Sheet SelfSheet
        {
            get
            {
                if (_selfSheet != null)
                    return _selfSheet;
                MetaData metaData = Master != this
                    ? Master.SelfSheet.Owner
                    : MetaData.Fetch(_database ?? (!String.IsNullOrEmpty(_dataSourceKey) ? Database.Fetch(_dataSourceKey) : Database.Default));
                return metaData.FindSheet<T>(InitializeSheet, true).GetHandle(this);
            }
            set
            {
                _selfSheet = value;
                _dataSourceKey = null;
                _database = null;
                _primaryKeyProperty = null;
            }
        }

        [NonSerialized]
        private Property _primaryKeyProperty;

        /// <summary>
        /// 主键表字段映射类属性
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Property PrimaryKeyProperty
        {
            get { return _primaryKeyProperty ??= SelfSheet.GetPrimaryKeyProperty(this.GetType()); }
        }

        /// <summary>
        /// 持久化表
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Sheet TargetTable
        {
            get { return PrimaryKeyProperty.Column.TableColumn.Owner; }
        }

        [NonSerialized]
        private IEntity _master;

        /// <summary>
        /// 主实体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public virtual IEntity Master
        {
            get { return _master ?? this; }
            protected set { _master = value; }
        }

        /// <summary>
        /// 根实体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IEntity Root
        {
            get { return Master != this ? Master.Root : this; }
        }

        /// <summary>
        /// 是根实体
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsRoot
        {
            get { return Master == this; }
        }

        /// <summary>
        /// 主键值
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public object PrimaryKey
        {
            get { return PrimaryKeyProperty.Field != null ? PrimaryKeyProperty.Field.GetValue(this) : PrimaryKeyProperty.GetValue(this); }
        }

        /// <summary>
        /// 主键值
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public virtual long PrimaryKeyLong
        {
            get { return Utilities.ChangeType<long>(PrimaryKey); }
            protected set
            {
                if (PrimaryKeyProperty.Field != null)
                    PrimaryKeyProperty.Field.Set(this, value);
                else
                    PrimaryKeyProperty.Set(this, value);
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 填充保留字段
        /// </summary>
        /// <param name="executeAction">执行动作</param>
        public T FillReservedFields(ExecuteAction executeAction)
        {
            return SelfSheet.FillReservedFields((T) this, executeAction);
        }

        /// <summary>
        /// 初始化自己
        /// </summary>
        protected virtual void InitializeSelf()
        {
        }

        private static void InitializeSheet(Database database)
        {
            MethodInfo methodInfo = typeof(T).GetMethod("Initialize",
                BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] {typeof(Database)}, null);
            if (methodInfo != null)
                methodInfo.Invoke(null, new object[] {database});
        }

        #region Detail

        private Sheet FindSheet<TDetail>(ref CriteriaExpression criteriaExpression)
            where TDetail : EntityBase<TDetail>
        {
            Sheet result = SelfSheet.Owner.FindSheet<TDetail>(true);
            foreach (Column primaryKeyColumn in SelfSheet.PrimaryKeyColumns)
            foreach (Column foreignKeyColumn in result.ForeignKeyColumns)
                if (foreignKeyColumn.ForeignKey.PrimaryKeyColumn == primaryKeyColumn)
                {
                    Property foreignKeyProperty = foreignKeyColumn.GetProperty(typeof(TDetail));
                    Property primaryKeyProperty = primaryKeyColumn.GetProperty(this.GetType());
                    criteriaExpression = criteriaExpression & new CriteriaExpression(new OperationExpression(foreignKeyProperty.PropertyInfo), CriteriaOperator.Equal, primaryKeyProperty.Field != null ? primaryKeyProperty.Field.GetValue(this) : primaryKeyProperty.GetValue(this));
                    break;
                }

            return result;
        }

        #region NewDetail

        /// <summary>
        /// 新增从实体对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>实体对象</returns>
        public TDetail NewDetail<TDetail>(params NameValue<TDetail>[] propertyValues)
            where TDetail : EntityBase<TDetail>
        {
            return NewDetail<TDetail>(NameValue<TDetail>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 新增从实体对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>实体对象</returns>
        public TDetail NewDetail<TDetail>(IDictionary<string, object> propertyValues)
            where TDetail : EntityBase<TDetail>
        {
            TDetail result = EntityBase<TDetail>.New(Database, propertyValues);
            foreach (Column primaryKeyColumn in SelfSheet.PrimaryKeyColumns)
            foreach (Column foreignKeyColumn in result.SelfSheet.ForeignKeyColumns)
                if (foreignKeyColumn.ForeignKey.PrimaryKeyColumn == primaryKeyColumn)
                {
                    Property foreignKeyProperty = foreignKeyColumn.GetProperty(result.GetType());
                    Property primaryKeyProperty = primaryKeyColumn.GetProperty(this.GetType());
                    if (foreignKeyProperty.Field != null)
                        foreignKeyProperty.Field.Set(result, primaryKeyProperty.Field != null ? primaryKeyProperty.Field.GetValue(this) : primaryKeyProperty.GetValue(this));
                    else
                        foreignKeyProperty.Set(result, primaryKeyProperty.Field != null ? primaryKeyProperty.Field.GetValue(this) : primaryKeyProperty.GetValue(this));
                    break;
                }

            result.Master = this;
            return result;
        }

        #endregion

        #region FetchDetails

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails((CriteriaExpression) null, null, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails((CriteriaExpression) null, null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), null, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, object criteria, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), criteria, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, object criteria, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), criteria, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails(criteriaExpression, null, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, object criteria, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails(criteriaExpression, criteria, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            return FetchDetails(criteriaExpression, null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取从实体对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, object criteria, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : EntityBase<TDetail>
        {
            IList<TDetail> result = FindSheet<TDetail>(ref criteriaExpression).SelectEntity(criteriaExpression, criteria, pageNo, pageSize, orderBys);
            foreach (TDetail item in result)
                item.Master = this;
            return result;
        }

        #endregion
        
        #region DeleteDetails

        /// <summary>
        /// 删除从实体对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <returns>删除记录数</returns>
        public int DeleteDetails<TDetail>(Expression<Func<T, bool>> criteriaLambda = null, object criteria = null)
            where TDetail : EntityBase<TDetail>
        {
            return DeleteDetails<TDetail>(CriteriaExpression.Where(criteriaLambda), criteria);
        }

        /// <summary>
        /// 删除从实体对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <returns>删除记录数</returns>
        public int DeleteDetails<TDetail>(CriteriaExpression criteriaExpression, object criteria = null)
            where TDetail : EntityBase<TDetail>
        {
            return FindSheet<TDetail>(ref criteriaExpression).DeleteRecord(criteriaExpression, criteria);
        }

        /// <summary>
        /// 删除从实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <returns>删除记录数</returns>
        public int DeleteDetails<TDetail>(DbConnection connection, Expression<Func<T, bool>> criteriaLambda = null, object criteria = null)
            where TDetail : EntityBase<TDetail>
        {
            return DeleteDetails<TDetail>(connection, CriteriaExpression.Where(criteriaLambda), criteria);
        }
        
        /// <summary>
        /// 删除从实体对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <returns>删除记录数</returns>
        public int DeleteDetails<TDetail>(DbConnection connection, CriteriaExpression criteriaExpression, object criteria = null)
            where TDetail : EntityBase<TDetail>
        {
            return FindSheet<TDetail>(ref criteriaExpression).DeleteRecord(connection, criteriaExpression, criteria);
        }

        /// <summary>
        /// 删除从实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <returns>删除记录数</returns>
        public int DeleteDetails<TDetail>(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda = null, object criteria = null)
            where TDetail : EntityBase<TDetail>
        {
            return DeleteDetails<TDetail>(transaction, CriteriaExpression.Where(criteriaLambda), criteria);
        }

        /// <summary>
        /// 删除从实体对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <returns>删除记录数</returns>
        public int DeleteDetails<TDetail>(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria = null)
            where TDetail : EntityBase<TDetail>
        {
            return FindSheet<TDetail>(ref criteriaExpression).DeleteRecord(transaction, criteriaExpression, criteria);
        }

        #endregion

        #endregion

        #region InsertOrUpdateSelf

        /// <summary>
        /// 新增自己如遇唯一键冲突则更新记录
        /// </summary>
        /// <returns>更新记录数</returns>
        public int InsertOrUpdateSelf()
        {
            SelfSheet = SelfSheet.GetHandle(this);
            return SelfSheet.InsertOrUpdateEntity((T) this);
        }

        /// <summary>
        /// 新增自己如遇唯一键冲突则更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int InsertOrUpdateSelf(DbConnection connection)
        {
            SelfSheet = SelfSheet.GetHandle(this);
            return SelfSheet.InsertOrUpdateEntity(connection, (T) this);
        }

        /// <summary>
        /// 新增自己如遇唯一键冲突则更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int InsertOrUpdateSelf(DbTransaction transaction)
        {
            SelfSheet = SelfSheet.GetHandle(this);
            return SelfSheet.InsertOrUpdateEntity(transaction, (T) this);
        }

        #endregion

        #region InsertSelf

        /// <summary>
        /// 新增自己
        /// </summary>
        /// <returns>更新记录数</returns>
        public int InsertSelf()
        {
            SelfSheet = SelfSheet.GetHandle(this);
            return SelfSheet.InsertEntity((T) this);
        }

        /// <summary>
        /// 新增自己
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int InsertSelf(DbConnection connection)
        {
            SelfSheet = SelfSheet.GetHandle(this);
            return SelfSheet.InsertEntity(connection, (T) this);
        }

        /// <summary>
        /// 新增自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int InsertSelf(DbTransaction transaction)
        {
            SelfSheet = SelfSheet.GetHandle(this);
            return SelfSheet.InsertEntity(transaction, (T) this);
        }

        /// <summary>
        /// 新增自己
        /// </summary>
        /// <param name="primaryKeyLong">主键值</param>
        /// <returns>更新记录数</returns>
        public int InsertSelf(long primaryKeyLong)
        {
            PrimaryKeyLong = primaryKeyLong;
            return InsertSelf();
        }

        /// <summary>
        /// 新增自己
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="primaryKeyLong">主键值</param>
        /// <returns>更新记录数</returns>
        public int InsertSelf(DbConnection connection, long primaryKeyLong)
        {
            PrimaryKeyLong = primaryKeyLong;
            return InsertSelf(connection);
        }

        /// <summary>
        /// 新增自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="primaryKeyLong">主键值</param>
        /// <returns>更新记录数</returns>
        public int InsertSelf(DbTransaction transaction, long primaryKeyLong)
        {
            PrimaryKeyLong = primaryKeyLong;
            return InsertSelf(transaction);
        }

        #endregion

        #region Property

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="value">值</param>
        public static NameValue<T> Set(Expression<Func<T, object>> propertyLambda, object value)
        {
            return NameValue.Set(propertyLambda, value);
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="valueLambda">值 lambda 表达式</param>
        public static NameValue<T> Set(Expression<Func<T, object>> propertyLambda, Expression<Func<T, object>> valueLambda)
        {
            return NameValue.Set(propertyLambda, valueLambda);
        }

        /// <summary>
        /// 应用属性值
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        public void Apply(params NameValue<T>[] propertyValues)
        {
            SelfSheet.Apply((T) this, propertyValues);
        }

        /// <summary>
        /// 应用属性值
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段)</param>
        public void Apply(IDictionary<string, object> propertyValues)
        {
            SelfSheet.Apply(this, propertyValues);
        }

        /// <summary>
        /// 提取属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        public object GetValue(Expression<Func<T, object>> propertyLambda, bool throwIfNotFound = true)
        {
            if (propertyLambda == null)
                throw new ArgumentNullException(nameof(propertyLambda));

            PropertyInfo propertyInfo = Utilities.GetPropertyInfo(propertyLambda, throwIfNotFound);
            if(propertyInfo != null)
                return propertyInfo.GetValue(this);

            if (throwIfNotFound)
                throw new InvalidOperationException(String.Format("{0} 应该是类 {1} 某个属性的表达式", propertyLambda.Name, typeof(T).FullName));
            return null;
        }

        /// <summary>
        /// 提取属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 InvalidOperationException; 如果为 false, 则在找不到信息时返回 null</param>
        public object GetValue(string propertyName, bool throwIfNotFound = true)
        {
            PropertyInfo propertyInfo = Utilities.FindPropertyInfo(this.GetType(), propertyName);
            if (propertyInfo != null)
                return propertyInfo.GetValue(this);

            if (throwIfNotFound)
                throw new InvalidOperationException(String.Format("{0} 应该是类 {1} 的某个属性名", propertyName, typeof(T).FullName));
            return null;
        }

        /// <summary>
        /// 提取脏属性值
        /// </summary>
        /// <param name="source">数据源</param>
        public IDictionary<string, object> GetDirtValues(T source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source = source.FillReservedFields(ExecuteAction.Update);
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (KeyValuePair<string, Property> kvp in SelfSheet.GetProperties(this.GetType(), TargetTable, false))
            {
                object value = kvp.Value.Field != null ? kvp.Value.Field.GetValue(source) : kvp.Value.GetValue(source);
                if (!object.Equals(kvp.Value.Field != null ? kvp.Value.Field.GetValue(this) : kvp.Value.GetValue(this), value))
                    result.Add(kvp.Key, value);
            }

            return result;
        }

        #endregion

        #region UpdateSelf

        /// <summary>
        /// 为更新自己追加条件表达式
        /// </summary>
        protected virtual CriteriaExpression AppendCriteriaForUpdateSelf(CriteriaExpression criteriaExpression)
        {
            return criteriaExpression;
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(params NameValue<T>[] propertyValues)
        {
            return UpdateSelf((CriteriaExpression) null, null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf((CriteriaExpression) null, null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(Expression<Func<T, bool>> criteriaLambda, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(CriteriaExpression.Where(criteriaLambda), null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(Expression<Func<T, bool>> criteriaLambda, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(CriteriaExpression.Where(criteriaLambda), null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(Expression<Func<T, bool>> criteriaLambda, object criteria, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(CriteriaExpression.Where(criteriaLambda), criteria, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(Expression<Func<T, bool>> criteriaLambda, object criteria, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(CriteriaExpression.Where(criteriaLambda), criteria, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(CriteriaExpression criteriaExpression, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(criteriaExpression, null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(CriteriaExpression criteriaExpression, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(criteriaExpression, null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(CriteriaExpression criteriaExpression, object criteria, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(criteriaExpression, criteria, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(CriteriaExpression criteriaExpression, object criteria, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return SelfSheet.UpdateEntity((T) this, AppendCriteriaForUpdateSelf(criteriaExpression), criteria, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, (CriteriaExpression) null, null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, (CriteriaExpression) null, null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, CriteriaExpression.Where(criteriaLambda), null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, CriteriaExpression.Where(criteriaLambda), null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, CriteriaExpression.Where(criteriaLambda), criteria, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, CriteriaExpression.Where(criteriaLambda), criteria, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, CriteriaExpression criteriaExpression, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, criteriaExpression, null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, CriteriaExpression criteriaExpression, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, criteriaExpression, null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(connection, criteriaExpression, criteria, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return SelfSheet.UpdateEntity(connection, (T) this, AppendCriteriaForUpdateSelf(criteriaExpression), criteria, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, (CriteriaExpression) null, null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, (CriteriaExpression) null, null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, CriteriaExpression.Where(criteriaLambda), null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, CriteriaExpression.Where(criteriaLambda), null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, CriteriaExpression.Where(criteriaLambda), criteria, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, CriteriaExpression.Where(criteriaLambda), criteria, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, CriteriaExpression criteriaExpression, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, criteriaExpression, null, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, CriteriaExpression criteriaExpression, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, criteriaExpression, null, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, params NameValue<T>[] propertyValues)
        {
            return UpdateSelf(transaction, criteriaExpression, criteria, true, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, bool checkTimestamp = true, params NameValue<T>[] propertyValues)
        {
            return SelfSheet.UpdateEntity(transaction, (T) this, AppendCriteriaForUpdateSelf(criteriaExpression), criteria, checkTimestamp, propertyValues);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(T source, bool checkTimestamp = true)
        {
            return UpdateSelf((CriteriaExpression) null, null, GetDirtValues(source), checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf((CriteriaExpression) null, null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(Expression<Func<T, bool>> criteriaLambda, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(CriteriaExpression.Where(criteriaLambda), null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(Expression<Func<T, bool>> criteriaLambda, object criteria, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(CriteriaExpression.Where(criteriaLambda), criteria, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(CriteriaExpression criteriaExpression, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(criteriaExpression, null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(CriteriaExpression criteriaExpression, object criteria, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return SelfSheet.UpdateEntity(this, AppendCriteriaForUpdateSelf(criteriaExpression), criteria, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="source">数据源</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, T source, bool checkTimestamp = true)
        {
            return UpdateSelf(connection, (CriteriaExpression) null, null, GetDirtValues(source), checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(connection, (CriteriaExpression) null, null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(connection, CriteriaExpression.Where(criteriaLambda), null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(connection, CriteriaExpression.Where(criteriaLambda), criteria, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, CriteriaExpression criteriaExpression, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(connection, criteriaExpression, null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return SelfSheet.UpdateEntity(connection, this, AppendCriteriaForUpdateSelf(criteriaExpression), criteria, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="source">数据源</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, T source, bool checkTimestamp = true)
        {
            return UpdateSelf(transaction, (CriteriaExpression) null, null, GetDirtValues(source), checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(transaction, (CriteriaExpression) null, null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(transaction, CriteriaExpression.Where(criteriaLambda), null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(transaction, CriteriaExpression.Where(criteriaLambda), criteria, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, CriteriaExpression criteriaExpression, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return UpdateSelf(transaction, criteriaExpression, null, propertyValues, checkTimestamp);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="propertyValues">待更新属性值队列(如果没有set语句的话就直接更新字段，null代表提交的是实体本身)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        /// <returns>更新记录数</returns>
        public int UpdateSelf(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, IDictionary<string, object> propertyValues, bool checkTimestamp = true)
        {
            return SelfSheet.UpdateEntity(transaction, this, AppendCriteriaForUpdateSelf(criteriaExpression), criteria, propertyValues, checkTimestamp);
        }

        #endregion

        #region DeleteSelf

        /// <summary>
        /// 为删除自己追加条件表达式
        /// </summary>
        protected virtual CriteriaExpression AppendCriteriaForDeleteSelf(CriteriaExpression criteriaExpression)
        {
            return criteriaExpression;
        }

        private CriteriaExpression AssembleCriteriaForDeleteSelf(CriteriaExpression criteriaExpression)
        {
            CriteriaExpression result = criteriaExpression;
            foreach (Column item in SelfSheet.PrimaryKeyColumns)
            {
                Property property = item.GetProperty(this.GetType());
                result = result & new CriteriaExpression(new OperationExpression(property.PropertyInfo), CriteriaOperator.Equal, property.Field != null ? property.Field.GetValue(this) : property.GetValue(this));
            }

            return AppendCriteriaForDeleteSelf(result);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(bool cascade = false)
        {
            return DeleteSelf((CriteriaExpression) null, null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(Expression<Func<T, bool>> criteriaLambda, bool cascade = false)
        {
            return DeleteSelf(CriteriaExpression.Where(criteriaLambda), null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(Expression<Func<T, bool>> criteriaLambda, object criteria, bool cascade = false)
        {
            return DeleteSelf(CriteriaExpression.Where(criteriaLambda), criteria, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(CriteriaExpression criteriaExpression, bool cascade = false)
        {
            return DeleteSelf(criteriaExpression, null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(CriteriaExpression criteriaExpression, object criteria, bool cascade = false)
        {
            int result = SelfSheet.DeleteEntity((T) this, AssembleCriteriaForDeleteSelf(criteriaExpression), criteria, cascade);
            if (result == 0 && cascade)
                throw new AssociationDataException();
            return result;
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbConnection connection, bool cascade = false)
        {
            return DeleteSelf(connection, (CriteriaExpression) null, null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, bool cascade = false)
        {
            return DeleteSelf(connection, CriteriaExpression.Where(criteriaLambda), null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, object criteria, bool cascade = false)
        {
            return DeleteSelf(connection, CriteriaExpression.Where(criteriaLambda), criteria, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbConnection connection, CriteriaExpression criteriaExpression, bool cascade = false)
        {
            return DeleteSelf(connection, criteriaExpression, null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbConnection connection, CriteriaExpression criteriaExpression, object criteria, bool cascade = false)
        {
            int result = SelfSheet.DeleteEntity(connection, (T) this, AssembleCriteriaForDeleteSelf(criteriaExpression), criteria, cascade);
            if (result == 0 && cascade)
                throw new AssociationDataException();
            return result;
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbTransaction transaction, bool cascade = false)
        {
            return DeleteSelf(transaction, (CriteriaExpression) null, null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, bool cascade = false)
        {
            return DeleteSelf(transaction, CriteriaExpression.Where(criteriaLambda), null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, object criteria, bool cascade = false)
        {
            return DeleteSelf(transaction, CriteriaExpression.Where(criteriaLambda), criteria, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbTransaction transaction, CriteriaExpression criteriaExpression, bool cascade = false)
        {
            return DeleteSelf(transaction, criteriaExpression, null, cascade);
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public int DeleteSelf(DbTransaction transaction, CriteriaExpression criteriaExpression, object criteria, bool cascade = false)
        {
            int result = SelfSheet.DeleteEntity(transaction, (T) this, AssembleCriteriaForDeleteSelf(criteriaExpression), criteria, cascade);
            if (result == 0 && cascade)
                throw new AssociationDataException();
            return result;
        }

        #endregion

        #region OrderBy

        /// <summary>
        /// 升序
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        public static OrderBy<T> Ascending(Expression<Func<T, object>> propertyLambda)
        {
            return OrderBy.Ascending(propertyLambda);
        }

        /// <summary>
        /// 降序
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        public static OrderBy<T> Descending(Expression<Func<T, object>> propertyLambda)
        {
            return OrderBy.Descending(propertyLambda);
        }

        #endregion

        #endregion
    }
}