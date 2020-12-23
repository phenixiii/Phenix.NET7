using System;
using System.Collections.Generic;
using System.Data.Common;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.SyncCollections;

namespace Phenix.Business
{
    /// <summary>
    /// ҵ�����
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
            //��ֹ��Ӵ���
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected BusinessBase(string dataSourceKey, bool? isNew, bool? isSelfDeleted, bool? isSelfDirty,
            IDictionary<string, object> oldPropertyValues, IDictionary<string, bool?> dirtyPropertyNames)
            : base(dataSourceKey, isNew, isSelfDeleted, isSelfDirty, oldPropertyValues, dirtyPropertyNames)
        {
        }

        #region ����

        [NonSerialized]
        private readonly SynchronizedDictionary<Type, List<IBusiness>> _details = new SynchronizedDictionary<Type, List<IBusiness>>();

        /// <summary>
        /// ����״̬(����ҵ�����)
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

        #region ����

        #region Detail

        /// <summary>
        /// ���ô�ҵ�����
        /// </summary>
        /// <param name="detail">��ҵ�����</param>
        protected void SetDetail<TDetail>(params TDetail[] detail)
            where TDetail : BusinessBase<TDetail>
        {
            SetDetail(true, detail);
        }

        /// <summary>
        /// ���ô�ҵ�����
        /// </summary>
        /// <param name="ignoreRepeat">�����ظ���</param>
        /// <param name="detail">��ҵ�����</param>
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
        /// ���ڴ�ҵ�����
        /// </summary>
        public bool HaveDetail<TDetail>()
            where TDetail : BusinessBase<TDetail>
        {
            return _details.ContainsKey(typeof(TDetail));
        }

        /// <summary>
        /// ������ҵ�����
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

        #region �༭״̬

        /// <summary>
        /// �����༭(������༭��IsSelfDirty״̬�Ķ���(����ҵ�����))
        /// ���յ�ǰ����
        /// </summary>
        public override void BeginEdit()
        {
            base.BeginEdit();

            foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
            foreach (IBusiness item in kvp.Value)
                item.BeginEdit();
        }

        /// <summary>
        /// �����༭(��������IsSelfDirty״̬�Ķ���(����ҵ�����))
        /// �ָ���������
        /// </summary>
        public override void CancelEdit()
        {
            base.CancelEdit();

            foreach (KeyValuePair<Type, List<IBusiness>> kvp in _details)
            foreach (IBusiness item in kvp.Value)
                item.CancelEdit();
        }

        /// <summary>
        /// Ӧ�ñ༭(����ҵ�����)
        /// ������������
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
        /// ��������
        /// </summary>
        /// <param name="checkTimestamp">�Ƿ���ʱ�������һ��ʱ�׳�Phenix.Core.Data.Rule.OutdatedDataException��</param>
        public void SaveDepth(bool checkTimestamp = true)
        {
            Database.Execute((Action<DbTransaction, bool>) SaveDepth, checkTimestamp);
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="connection">DbConnection(ע�������δ��У��)</param>
        /// <param name="checkTimestamp">�Ƿ���ʱ�������һ��ʱ�׳�Phenix.Core.Data.Rule.OutdatedDataException��</param>
        public void SaveDepth(DbConnection connection, bool checkTimestamp = true)
        {
            DbConnectionHelper.Execute(connection, SaveDepth, checkTimestamp);
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="transaction">DbTransaction(ע�������δ��У��)</param>
        /// <param name="checkTimestamp">�Ƿ���ʱ�������һ��ʱ�׳�Phenix.Core.Data.Rule.OutdatedDataException��</param>
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