using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Data.Expressions;
using Phenix.Core.SyncCollections;

namespace Phenix.Business
{
    /// <summary>
    /// 业务基类
    /// </summary>
    [Serializable]
    public abstract class BusinessBase<T> : UndoableBase<T>, IBusiness, IRefinedBusiness
        where T : BusinessBase<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected BusinessBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected BusinessBase(string dataSourceKey, bool? isNew, bool? isSelfDeleted, bool? isSelfDirty,
            IDictionary<string, object> oldPropertyValues, IDictionary<string, bool?> dirtyPropertyNames)
            : base(dataSourceKey, isNew, isSelfDeleted, isSelfDirty, oldPropertyValues, dirtyPropertyNames)
        {
        }

        #region 属性

        private readonly SynchronizedDictionary<Type, List<IBusiness>> _details = new SynchronizedDictionary<Type, List<IBusiness>>();

        /// <summary>
        /// 更新状态(含从业务对象)
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsDirty
        {
            get
            {
                if (IsSelfDirty)
                    return true;
                foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
                foreach (IBusiness item in kvp.Value)
                    if (item.IsDirty)
                        return true;
                return false;
            }
        }

        #endregion

        #region 方法

        #region Detail

        /// <summary>
        /// 设置从业务对象
        /// </summary>
        /// <param name="detail">从业务对象</param>
        public void SetDetail<TDetail>(params TDetail[] detail)
            where TDetail : BusinessBase<TDetail>
        {
            SetDetail(false, detail);
        }

        /// <summary>
        /// 设置从业务对象
        /// </summary>
        /// <param name="ignoreRepeat">忽略重复的</param>
        /// <param name="detail">从业务对象</param>
        public void SetDetail<TDetail>(bool ignoreRepeat = false, params TDetail[] detail)
            where TDetail : BusinessBase<TDetail>
        {
            if (detail == null || detail.Length == 0)
                return;
            List<IBusiness> value = _details.GetValue(typeof(TDetail), () => new List<IBusiness>());
            foreach (TDetail item in detail)
                if (ignoreRepeat || !value.Contains(item))
                {
                    item.Master = this;
                    value.Add(item);
                }
        }

        /// <summary>
        /// 存在从业务对象
        /// </summary>
        public bool HaveDetail<TDetail>()
            where TDetail : BusinessBase<TDetail>
        {
            return _details.ContainsKey(typeof(TDetail));
        }

        /// <summary>
        /// 检索从业务对象
        /// </summary>
        public TDetail[] FindDetail<TDetail>()
            where TDetail : BusinessBase<TDetail>
        {
            if (_details.TryGetValue(typeof(TDetail), out List<IBusiness> detail))
            {
                List<TDetail> result = new List<TDetail>(detail.Count);
                foreach (IBusiness item in detail)
                    result.Add((TDetail) item);
                return result.ToArray();
            }

            return null;
        }

        #region NewDetail

        /// <summary>
        /// 新增从业务对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>业务对象</returns>
        public new TDetail NewDetail<TDetail>(params NameValue[] propertyValues)
            where TDetail : BusinessBase<TDetail>
        {
            return NewDetail<TDetail>(NameValue.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 新增从业务对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>业务对象</returns>
        public new TDetail NewDetail<TDetail>(IDictionary<string, object> propertyValues)
            where TDetail : BusinessBase<TDetail>
        {
            TDetail result = base.NewDetail<TDetail>(propertyValues);
            SetDetail(result);
            return result;
        }

        #endregion

        #region FetchDetails

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails((CriteriaExpression)null, null, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails((CriteriaExpression)null, null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), null, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, object criteria, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), criteria, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="criteriaLambda">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, object criteria, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), criteria, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(criteriaExpression, null, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, object criteria, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(criteriaExpression, criteria, 0, 10, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(criteriaExpression, null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// 获取从业务对象
        /// </summary>
        /// <param name="criteriaExpression">条件表达式</param>
        /// <param name="criteria">条件对象/JSON格式字符串/属性值队列</param>
        /// <param name="pageNo">页码(1..N, 0为不分页)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="orderBys">排序队列</param>
        public new IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, object criteria, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            IList<TDetail> result = base.FetchDetails(criteriaExpression, criteria, pageNo, pageSize, orderBys);
            SetDetail(result.ToArray());
            return result;
        }

        #endregion

        #endregion

        #region 编辑状态

        /// <summary>
        /// 启动编辑(仅允许编辑非IsSelfDirty状态的对象(含从业务对象))
        /// 快照当前数据
        /// </summary>
        public override void BeginEdit()
        {
            base.BeginEdit();

            foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
            foreach (IBusiness item in kvp.Value)
                item.BeginEdit();
        }

        /// <summary>
        /// 撤销编辑(仅允许撤销IsSelfDirty状态的对象(含从业务对象))
        /// 恢复快照数据
        /// </summary>
        public override void CancelEdit()
        {
            base.CancelEdit();

            foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
            foreach (IBusiness item in kvp.Value)
                item.CancelEdit();
        }

        /// <summary>
        /// 应用编辑(含从业务对象)
        /// 丢弃快照数据
        /// </summary>
        public override void ApplyEdit()
        {
            base.ApplyEdit();

            foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
            foreach (IBusiness item in kvp.Value)
                item.ApplyEdit();
        }

        #endregion

        #region SaveDepth

        /// <summary>
        /// 级联保存
        /// </summary>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        public void SaveDepth(bool checkTimestamp = true)
        {
            Database.Execute((Action<DbTransaction, bool>) SaveDepth, checkTimestamp);
        }

        /// <summary>
        /// 级联保存
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        public void SaveDepth(DbConnection connection, bool checkTimestamp = true)
        {
            DbConnectionHelper.Execute(connection, SaveDepth, checkTimestamp);
        }

        /// <summary>
        /// 级联保存
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        public virtual void SaveDepth(DbTransaction transaction, bool checkTimestamp = true)
        {
            if (IsNew)
                SaveDepth(transaction, ExecuteAction.Insert, checkTimestamp);
            else if (IsSelfDeleted)
                SaveDepth(transaction, ExecuteAction.Delete, checkTimestamp);
            else
                SaveDepth(transaction, ExecuteAction.Update, checkTimestamp);
            ApplyEdit();
        }

        void IRefinedBusiness.SaveDepth(DbTransaction transaction, bool checkTimestamp)
        {
            SaveDepth(transaction, checkTimestamp);
        }

        private void SaveDepth(DbTransaction transaction, ExecuteAction executeAction, bool checkTimestamp)
        {
            switch (executeAction)
            {
                case ExecuteAction.Insert:
                    if (IsSelfDeleted)
                        break;
                    InsertSelf(transaction);
                    foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
                    foreach (IBusiness item in kvp.Value)
                        ((IRefinedBusiness) item).SaveDepth(transaction, ExecuteAction.Insert, checkTimestamp);
                    break;
                case ExecuteAction.Delete:
                    if (IsNew)
                        break;
                    foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
                    foreach (IBusiness item in kvp.Value)
                        ((IRefinedBusiness) item).SaveDepth(transaction, ExecuteAction.Delete, checkTimestamp);
                    DeleteSelf(transaction, true);
                    break;
                case ExecuteAction.Update:
                    foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
                    foreach (IBusiness item in kvp.Value)
                        if (item.IsNew)
                            ((IRefinedBusiness) item).SaveDepth(transaction, ExecuteAction.Insert, checkTimestamp);
                        else if (IsSelfDeleted)
                            ((IRefinedBusiness) item).SaveDepth(transaction, ExecuteAction.Delete, checkTimestamp);
                        else
                            ((IRefinedBusiness) item).SaveDepth(transaction, ExecuteAction.Update, checkTimestamp);
                    if (IsSelfDirty)
                        UpdateSelf(transaction, GetDirtValues(), checkTimestamp);
                    break;
            }
        }

        void IRefinedBusiness.SaveDepth(DbTransaction transaction, ExecuteAction executeAction, bool checkTimestamp)
        {
            SaveDepth(transaction, executeAction, checkTimestamp);
        }

        #endregion

        #endregion
    }
}