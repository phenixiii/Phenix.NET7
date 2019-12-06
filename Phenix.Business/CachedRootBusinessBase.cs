using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Schema;
using Phenix.Core.SyncCollections;

namespace Phenix.Business
{
    /// <summary>
    /// 缓存根业务基类
    /// </summary>
    [Serializable]
    public abstract class CachedRootBusinessBase<T> : RootBusinessBase<T>, ICachedObject
        where T : CachedRootBusinessBase<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected CachedRootBusinessBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected CachedRootBusinessBase(bool? isNew, bool? isSelfDeleted, bool? isSelfDirty,
            IDictionary<string, object> oldPropertyValues, IDictionary<string, bool?> dirtyPropertyNames,
            long id)
            : base(isNew, isSelfDeleted, isSelfDirty, oldPropertyValues, dirtyPropertyNames, id)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected CachedRootBusinessBase(long id)
            : base(id)
        {
        }

        #region 工厂

        private static readonly object _lock = new object();

        private static SynchronizedDictionary<string, long> _idCache;

        /// <summary>
        /// 主键缓存
        /// </summary>
        protected static SynchronizedDictionary<string, long> IdCache
        {
            get
            {
                if (_idCache == null)
                {
                    lock (_lock)
                        if (_idCache == null)
                        {
                            _idCache = new SynchronizedDictionary<string, long>(StringComparer.Ordinal);
                        }

                    Thread.MemoryBarrier();
                }

                return _idCache;
            }
        }

        private static SynchronizedDictionary<long, T> _objectCache;

        /// <summary>
        /// 对象缓存
        /// </summary>
        protected static SynchronizedDictionary<long, T> ObjectCache
        {
            get
            {
                if (_objectCache == null)
                {
                    lock (_lock)
                        if (_objectCache == null)
                        {
                            _objectCache = Phenix.Core.Threading.Timer.CreateTimedTask<SynchronizedDictionary<long, T>>(0, 0);
                            AddRenovatorTrigger((tableName, primaryKeyValue, executeTime, executeAction) =>
                            {
                                if (_idCache != null)
                                    _idCache.Clear();
                                _objectCache.Remove(primaryKeyValue);
                            });
                        }

                    Thread.MemoryBarrier();
                }

                return _objectCache;
            }
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(long id, int resetHoursLater = 8)
        {
            return Fetch(id, null, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="doCreate">如果没有该ID的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(long id, Func<Task<T>> doCreate, int resetHoursLater = 8)
        {
            return ObjectCache.GetValue(id,
                () =>
                {
                    T result = RootBusinessBase<T>.Fetch(id, doCreate);
                    if (result != null)
                        result.InvalidTime = resetHoursLater == 0 ? DateTime.MaxValue : DateTime.Now.AddHours(resetHoursLater);
                    return result;
                },
                value => value == null || resetHoursLater < 0 || resetHoursLater > 0 && value.InvalidTime < DateTime.Now);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="id">ID</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbConnection connection, long id, int resetHoursLater = 8)
        {
            return Fetch(connection, id, null, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="id">ID</param>
        /// <param name="doCreate">如果没有该ID的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbConnection connection, long id, Func<Task<T>> doCreate, int resetHoursLater = 8)
        {
            return ObjectCache.GetValue(id,
                () =>
                {
                    T result = RootBusinessBase<T>.Fetch(connection, id, doCreate);
                    if (result != null)
                        result.InvalidTime = resetHoursLater == 0 ? DateTime.MaxValue : DateTime.Now.AddHours(resetHoursLater);
                    return result;
                },
                value => value == null || resetHoursLater < 0 || resetHoursLater > 0 && value.InvalidTime < DateTime.Now);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="id">ID</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbTransaction transaction, long id, int resetHoursLater = 8)
        {
            return Fetch(transaction, id, null, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="id">ID</param>
        /// <param name="doCreate">如果没有该ID的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbTransaction transaction, long id, Func<Task<T>> doCreate, int resetHoursLater = 8)
        {
            return ObjectCache.GetValue(id,
                () =>
                {
                    T result = RootBusinessBase<T>.Fetch(transaction, id, doCreate);
                    if (result != null)
                        result.InvalidTime = resetHoursLater == 0 ? DateTime.MaxValue : DateTime.Now.AddHours(resetHoursLater);
                    return result;
                },
                value => value == null || resetHoursLater < 0 || resetHoursLater > 0 && value.InvalidTime < DateTime.Now);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        /// <returns>IMemCachedEntity</returns>
        public static T Fetch(Expression<Func<T, bool>> criteriaLambda, int resetHoursLater = 8)
        {
            return Fetch(criteriaLambda, null, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(Expression<Func<T, bool>> criteriaLambda, Func<Task<T>> doCreate = null, int resetHoursLater = 8)
        {
            return Fetch(CriteriaExpression.Where(criteriaLambda), doCreate, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        /// <returns>IMemCachedEntity</returns>
        public static T Fetch(CriteriaExpression criteriaExpression, int resetHoursLater = 8)
        {
            return Fetch(criteriaExpression, null, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(CriteriaExpression criteriaExpression, Func<Task<T>> doCreate = null, int resetHoursLater = 8)
        {
            if (criteriaExpression == null)
                throw new ArgumentNullException(nameof(criteriaExpression));

            T result = null;
            string key = criteriaExpression.ToString();
            if (IdCache.TryGetValue(key, out long id))
                result = Fetch(id, doCreate, resetHoursLater);
            if (result == null)
            {
                result = BusinessBase<T>.Fetch(criteriaExpression, doCreate);
                if (result != null)
                {
                    result.InvalidTime = resetHoursLater == 0 ? DateTime.MaxValue : DateTime.Now.AddHours(resetHoursLater);
                    ObjectCache[result.Id] = result;
                    IdCache[key] = result.Id;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, int resetHoursLater = 8)
        {
            return Fetch(connection, criteriaLambda, null, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbConnection connection, Expression<Func<T, bool>> criteriaLambda, Func<Task<T>> doCreate = null, int resetHoursLater = 8)
        {
            return Fetch(connection, CriteriaExpression.Where(criteriaLambda), doCreate, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbConnection connection, CriteriaExpression criteriaExpression, int resetHoursLater = 8)
        {
            return Fetch(connection, criteriaExpression, null, resetHoursLater);
        }
        
        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbConnection connection, CriteriaExpression criteriaExpression, Func<Task<T>> doCreate = null, int resetHoursLater = 8)
        {
            if (criteriaExpression == null)
                throw new ArgumentNullException(nameof(criteriaExpression));

            T result = null;
            string key = criteriaExpression.ToString();
            if (IdCache.TryGetValue(key, out long id))
                result = Fetch(connection, id, doCreate, resetHoursLater);
            if (result == null)
            {
                result = BusinessBase<T>.Fetch(connection, criteriaExpression, doCreate);
                if (result != null)
                {
                    result.InvalidTime = resetHoursLater == 0 ? DateTime.MaxValue : DateTime.Now.AddHours(resetHoursLater);
                    ObjectCache[result.Id] = result;
                    IdCache[key] = result.Id;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, int resetHoursLater = 8)
        {
            return Fetch(transaction, criteriaLambda, null, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbTransaction transaction, Expression<Func<T, bool>> criteriaLambda, Func<Task<T>> doCreate = null, int resetHoursLater = 8)
        {
            return Fetch(transaction, CriteriaExpression.Where(criteriaLambda), doCreate, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbTransaction transaction, CriteriaExpression criteriaExpression, int resetHoursLater = 8)
        {
            return Fetch(transaction, criteriaExpression, null, resetHoursLater);
        }

        /// <summary>
        /// 获取根业务对象
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的业务对象则调用本函数新增并自动持久化</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        public static T Fetch(DbTransaction transaction, CriteriaExpression criteriaExpression, Func<Task<T>> doCreate = null, int resetHoursLater = 8)
        {
            T result = null;
            string key = criteriaExpression.ToString();
            if (IdCache.TryGetValue(key, out long id))
                result = Fetch(transaction, id, doCreate, resetHoursLater);
            if (result == null)
            {
                result = BusinessBase<T>.Fetch(transaction, criteriaExpression, doCreate);
                if (result != null)
                {
                    result.InvalidTime = resetHoursLater == 0 ? DateTime.MaxValue : DateTime.Now.AddHours(resetHoursLater);
                    ObjectCache[result.Id] = result;
                    IdCache[key] = result.Id;
                }
            }

            return result;
        }

        #endregion

        #region 属性

        [NonSerialized]
        private DateTime _invalidTime;

        /// <summary>
        /// 失效时间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime InvalidTime
        {
            get { return _invalidTime; }
            set { _invalidTime = value; }
        }

        #endregion

        #region 方法

        #region InsertSelf

        /// <summary>
        /// 新增自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public override int InsertSelf(DbTransaction transaction)
        {
            int result = base.InsertSelf(transaction);
            SaveRenovateLog(transaction, ExecuteAction.Insert);
            return result;
        }

        #endregion

        #region UpdateSelf

        /// <summary>
        /// 更新自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Validity.OutdatedDataException，仅当propertyValues含映射时间戳字段时有效）</param>
        /// <param name="propertyValues">待更新属性值队列(null代表提交所有属性)</param>
        /// <returns>更新记录数</returns>
        public override int UpdateSelf(DbTransaction transaction, bool checkTimestamp = true, params PropertyValue[] propertyValues)
        {
            int result = base.UpdateSelf(transaction, checkTimestamp, propertyValues);
            SaveRenovateLog(transaction, ExecuteAction.Update);
            return result;
        }

        #endregion

        #region DeleteSelf

        /// <summary>
        /// 删除自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="cascade">是否级联</param>
        /// <returns>删除记录数</returns>
        public override int DeleteSelf(DbTransaction transaction, bool cascade = false)
        {
            int result = base.DeleteSelf(transaction, cascade);
            SaveRenovateLog(transaction, ExecuteAction.Delete);
            return result;
        }

        #endregion

        #region Renovator

        /// <summary>
        /// 保存表记录更新日志
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="executeTime">执行时间</param>
        /// <param name="executeAction">执行动作</param>
        public override void SaveRenovateLog(DbTransaction transaction, ExecuteAction executeAction, DateTime executeTime)
        {
            ObjectCache.Remove(Id);
            base.SaveRenovateLog(transaction, executeAction, executeTime);
        }

        #endregion

        #endregion
    }
}