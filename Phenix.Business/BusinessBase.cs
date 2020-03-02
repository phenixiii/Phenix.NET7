using System;
using System.Collections.Generic;
using System.Data.Common;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
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
        protected BusinessBase(bool? isNew, bool? isSelfDeleted, bool? isSelfDirty,
            IDictionary<string, object> oldPropertyValues, IDictionary<string, bool?> dirtyPropertyNames)
            : base(isNew, isSelfDeleted, isSelfDirty, oldPropertyValues, dirtyPropertyNames)
        {
        }

        #region 属性

        [NonSerialized]
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
        protected void SetDetail<TDetail>(params TDetail[] detail)
            where TDetail : BusinessBase<TDetail>
        {
            SetDetail(true, detail);
        }

        /// <summary>
        /// 设置从业务对象
        /// </summary>
        /// <param name="ignoreRepeat">忽略重复的</param>
        /// <param name="detail">从业务对象</param>
        protected void SetDetail<TDetail>(bool ignoreRepeat = true, params TDetail[] detail)
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
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Validity.OutdatedDataException，仅当属性含映射时间戳字段时有效）</param>
        public void SaveDepth(bool checkTimestamp = true)
        {
            Database.Execute((Action<DbTransaction, bool>) SaveDepth, checkTimestamp);
        }

        /// <summary>
        /// 级联保存
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Validity.OutdatedDataException，仅当属性含映射时间戳字段时有效）</param>
        public void SaveDepth(DbConnection connection, bool checkTimestamp = true)
        {
            DbConnectionHelper.Execute(connection, SaveDepth, checkTimestamp);
        }

        /// <summary>
        /// 级联保存
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Validity.OutdatedDataException，仅当属性含映射时间戳字段时有效）</param>
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
                        UpdateSelf(transaction, checkTimestamp, GetDirtValues());
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