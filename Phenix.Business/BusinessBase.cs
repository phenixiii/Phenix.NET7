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
        public void SetDetail<TDetail>(params TDetail[] detail)
            where TDetail : BusinessBase<TDetail>
        {
            SetDetail(false, detail);
        }

        /// <summary>
        /// ���ô�ҵ�����
        /// </summary>
        /// <param name="ignoreRepeat">�����ظ���</param>
        /// <param name="detail">��ҵ�����</param>
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

        #region NewDetail

        /// <summary>
        /// ������ҵ�����(�Զ���������ͱ����ֶ�)
        /// </summary>
        /// <param name="propertyValues">����������ֵ����</param>
        /// <returns>ҵ�����</returns>
        public new TDetail NewDetail<TDetail>(params NameValue[] propertyValues)
            where TDetail : BusinessBase<TDetail>
        {
            return NewDetail<TDetail>(NameValue.ToDictionary(propertyValues));
        }

        /// <summary>
        /// ������ҵ�����(�Զ���������ͱ����ֶ�)
        /// </summary>
        /// <param name="propertyValues">����������ֵ����</param>
        /// <returns>ҵ�����</returns>
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
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails((CriteriaExpression)null, null, 0, 10, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="pageNo">ҳ��(1..N, 0Ϊ����ҳ)</param>
        /// <param name="pageSize">��ҳ��С</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails((CriteriaExpression)null, null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="criteriaLambda">�������ʽ</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), null, 0, 10, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="criteriaLambda">�������ʽ</param>
        /// <param name="criteria">��������/JSON��ʽ�ַ���/����ֵ����</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, object criteria, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), criteria, 0, 10, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="criteriaLambda">�������ʽ</param>
        /// <param name="pageNo">ҳ��(1..N, 0Ϊ����ҳ)</param>
        /// <param name="pageSize">��ҳ��С</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="criteriaLambda">�������ʽ</param>
        /// <param name="criteria">��������/JSON��ʽ�ַ���/����ֵ����</param>
        /// <param name="pageNo">ҳ��(1..N, 0Ϊ����ҳ)</param>
        /// <param name="pageSize">��ҳ��С</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(Expression<Func<TDetail, bool>> criteriaLambda, object criteria, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(CriteriaExpression.Where(criteriaLambda), criteria, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="criteriaExpression">�������ʽ</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(criteriaExpression, null, 0, 10, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="criteriaExpression">�������ʽ</param>
        /// <param name="criteria">��������/JSON��ʽ�ַ���/����ֵ����</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, object criteria, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(criteriaExpression, criteria, 0, 10, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="criteriaExpression">�������ʽ</param>
        /// <param name="pageNo">ҳ��(1..N, 0Ϊ����ҳ)</param>
        /// <param name="pageSize">��ҳ��С</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            return FetchDetails(criteriaExpression, null, pageNo, pageSize, orderBys);
        }

        /// <summary>
        /// ��ȡ��ҵ�����
        /// </summary>
        /// <param name="criteriaExpression">�������ʽ</param>
        /// <param name="criteria">��������/JSON��ʽ�ַ���/����ֵ����</param>
        /// <param name="pageNo">ҳ��(1..N, 0Ϊ����ҳ)</param>
        /// <param name="pageSize">��ҳ��С</param>
        /// <param name="orderBys">�������</param>
        public new IList<TDetail> FetchDetails<TDetail>(CriteriaExpression criteriaExpression, object criteria, int pageNo, int pageSize = 10, params OrderBy<TDetail>[] orderBys)
            where TDetail : BusinessBase<TDetail>
        {
            IList<TDetail> result = base.FetchDetails(criteriaExpression, criteria, pageNo, pageSize, orderBys);
            SetDetail(result.ToArray());
            return result;
        }

        #endregion

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