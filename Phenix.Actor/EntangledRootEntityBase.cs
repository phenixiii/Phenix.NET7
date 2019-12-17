using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;
using Phenix.Core.Data.Validity;
using Phenix.Core.Reflection;
using Phenix.Core.SyncCollections;

namespace Phenix.Actor
{
    /// <summary>
    /// 纠缠根实体基类
    /// </summary>
    [Serializable]
    public abstract class EntangledRootEntityBase<T, TGrainInterface> 
        where T : EntangledRootEntityBase<T, TGrainInterface>
        where TGrainInterface : IRootEntityGrain
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected EntangledRootEntityBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected EntangledRootEntityBase(long id)
        {
            _id = id;
        }

        #region 工厂

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="doCreate">如果没有该ID的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static async Task<T> FetchAsync(long id, Func<Task<T>> doCreate = null)
        {
            return await FetchAsync(ClusterClient.Default, id, doCreate);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="client">IClusterClient</param>
        /// <param name="id">ID</param>
        /// <param name="doCreate">如果没有该ID的实体对象则调用本函数新增并自动持久化</param>
        /// <returns>根实体对象</returns>
        public static async Task<T> FetchAsync(IClusterClient client, long id, Func<Task<T>> doCreate = null)
        {
            return await FetchAsync(client.GetGrain<TGrainInterface>(id), id, doCreate);
        }

        private static async Task<T> FetchAsync(TGrainInterface grain, long id, Func<Task<T>> doCreate = null)
        {
            T result = Utilities.JsonDeserialize<T>(await grain.SelectRecord());
            if (result != null)
            {
                result.Grain = grain;
                result.Id = id;
            }
            else if (doCreate != null)
            {
                result = doCreate().Result;
                if (result != null)
                {
                    result.Grain = grain;
                    result.Id = id;
                    try
                    {
                        result.InsertSelf();
                    }
                    catch (UniqueConstraintException)
                    {
                        result = await FetchAsync(grain, id, null);
                    }
                }
            }

            return result;
        }

        private static readonly SynchronizedDictionary<string, CachedObject<long>> _idCache = 
            new SynchronizedDictionary<string, CachedObject<long>>(StringComparer.Ordinal);

        /// <summary>
        /// 获取实体对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        public static async Task<T> FetchAsync(Expression<Func<T, bool>> criteriaLambda, Func<Task<T>> doCreate = null)
        {
            return await FetchAsync(ClusterClient.Default, CriteriaExpression.Where(criteriaLambda), doCreate);
        }

        /// <summary>
        /// 获取实体对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        public static async Task<T> FetchAsync(CriteriaExpression criteriaExpression, Func<Task<T>> doCreate = null)
        {
            return await FetchAsync(ClusterClient.Default, criteriaExpression, doCreate);
        }

        /// <summary>
        /// 获取实体对象
        /// </summary>
        /// <param name="client">IClusterClient</param>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        public static async Task<T> FetchAsync(IClusterClient client, Expression<Func<T, bool>> criteriaLambda, Func<Task<T>> doCreate = null)
        {
            return await FetchAsync(client, CriteriaExpression.Where(criteriaLambda), doCreate);
        }

        /// <summary>
        /// 获取实体对象
        /// </summary>
        /// <param name="client">IClusterClient</param>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="doCreate">如果没有该条件的实体对象则调用本函数新增并自动持久化</param>
        public static async Task<T> FetchAsync(IClusterClient client, CriteriaExpression criteriaExpression, Func<Task<T>> doCreate = null)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (criteriaExpression == null)
                throw new ArgumentNullException(nameof(criteriaExpression));

            T result = null;
            string key = criteriaExpression.ToString();
            if (_idCache.TryGetValue(key, out CachedObject<long> id))
                result = await FetchAsync(client, id.Value, doCreate);
            if (result == null)
                result = Sheet.SelectEntity<T>(criteriaExpression).FirstOrDefault();
            if (result == null && doCreate != null)
            {
                result = doCreate().Result;
                if (result != null)
                    try
                    {
                        result.InsertSelf();
                    }
                    catch (UniqueConstraintException)
                    {
                        result = await FetchAsync(client, criteriaExpression, null);
                    }
            }

            if (result != null)
            {
                if (result.Grain == null)
                    result.Grain = client.GetGrain<TGrainInterface>(result.Id);
                _idCache[key] = new CachedObject<long>(result.Id, DateTime.Now.AddHours(8));
            }

            return result;
        }

        #endregion

        #region 属性

        #region 数据源

        private static Database _database;

        /// <summary>
        /// 数据库入口
        /// </summary>
        public static Database Database
        {
            get { return _database ?? (_database = _sheet != null ? _sheet.Owner.Database : Database.Default); }
            set
            {
                _database = value;
                _sheet = null;
            }
        }

        private static readonly object _initializeLock = new object();
        private static bool _initialized;
        private static Sheet _sheet;

        /// <summary>
        /// 单子
        /// </summary>
        public static Sheet Sheet
        {
            get
            {
                Sheet result = _sheet ?? (_sheet = _database != null ? _database.MetaData.FindSheet<T>(true) : Database.Default.MetaData.FindSheet<T>(true));

                if (!_initialized)
                {
                    lock (_initializeLock)
                        if (!_initialized)
                        {
                            MethodInfo methodInfo = typeof(T).GetMethod("Initialize",
                                BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] {typeof(Sheet)}, null);
                            if (methodInfo != null)
                                methodInfo.Invoke(null, new object[] {result});
                            _initialized = true;
                        }

                    Thread.MemoryBarrier();
                }

                return result;
            }
            set
            {
                _sheet = value;
                _database = null;
            }
        }
        
        #endregion

        [NonSerialized]
        private TGrainInterface _grain;

        /// <summary>
        /// IRootEntityGrain
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public TGrainInterface Grain
        {
            get { return _grain ; }
            private set { _grain = value; }
        }

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
            private set { _id = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 刷新自己(映射表的字段不能带 readonly 标记)
        /// </summary>
        public virtual void RefreshSelf()
        {
            Utilities.FillFieldValues(FetchAsync(Grain, Id).Result, this);
        }

        #region InsertSelf

        /// <summary>
        /// 新增自己(如未指定主键属性则仅提交第一个属性映射的表记录)
        /// </summary>
        /// <returns>更新记录数</returns>
        public int InsertSelf()
        {
            return Database.ExecuteGet((Func<DbTransaction, int>)InsertSelf);
        }

        /// <summary>
        /// 新增自己(如未指定主键属性则仅提交第一个属性映射的表记录)
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int InsertSelf(DbConnection connection)
        {
            return DbConnectionHelper.ExecuteGet(connection, InsertSelf);
        }

        /// <summary>
        /// 新增自己(如未指定主键属性则仅提交第一个属性映射的表记录)
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public virtual int InsertSelf(DbTransaction transaction)
        {
            return Sheet.InsertEntity(transaction, (T) this, p => p.Id);
        }

        #endregion

        #region SetProperty

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="newValue">新值</param>
        public NameValue SetProperty(Expression<Func<T, object>> propertyLambda, object newValue)
        {
            return NameValue.Set(propertyLambda, newValue);
        }

        #endregion

        #region UpdateSelf

        /// <summary>
        /// 更新自己(提交第一个属性映射的表记录)
        /// </summary>
        /// <param name="nameValues">待更新属性值队列(映射表的字段不能带 readonly 标记)</param>
        /// <returns>更新记录数</returns>
        public Task<int> UpdateSelf(params NameValue[] nameValues)
        {
            if (nameValues == null || nameValues.Length == 0)
                throw new ArgumentNullException(nameof(nameValues));

            Dictionary<string, object> propertyValues = new Dictionary<string, object>(nameValues.Length + 1);
            foreach (NameValue item in nameValues)
                propertyValues.Add(item.Name, item.Value);
            return UpdateSelf(propertyValues);
        }

        /// <summary>
        /// 更新自己(提交第一个属性映射的表记录)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列(映射表的字段不能带 readonly 标记)</param>
        /// <returns>更新记录数</returns>
        public Task<int> UpdateSelf(IDictionary<string, object> propertyValues)
        {
            if (propertyValues == null || propertyValues.Count == 0)
                throw new ArgumentNullException(nameof(propertyValues));

            propertyValues["Id"] =  Id;
            return Grain.UpdateRecord(Utilities.JsonSerialize(propertyValues));
        }

        #endregion

        #endregion
    }
}
