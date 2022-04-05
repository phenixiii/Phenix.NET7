using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Phenix.Common.Reflection;

namespace Phenix.Common.SyncCollections
{
    /// <summary>
    /// ��ʾ��ͨ���������ʵĶ����ǿ�����б�
    /// �ṩ���ڶ��б���ж�ά����������Ͳ����ķ���
    /// </summary>
    /// <typeparam name="T">�б���Ԫ�ص�����</typeparam>
    [Serializable]
    public class SynchronizedMultiSortedList<T> : SynchronizedList<T>
        where T : class
    {
        #region ����

        [NonSerialized]
        private readonly SynchronizedDictionary<MemberInfo, SynchronizedDictionary<object, T>> _cache =
            new SynchronizedDictionary<MemberInfo, SynchronizedDictionary<object, T>>();

        #endregion

        #region ����

        private IDictionary<object, T> FetchCache(MemberInfo memberInfo)
        {
            return _cache.GetValue(memberInfo, () =>
            {
                SynchronizedDictionary<object, T> result = new SynchronizedDictionary<object, T>();
                foreach (T item in _infos)
                {
                    object memberValue = Utilities.GetMemberValue(item, memberInfo);
                    if (result.ContainsKey(memberValue))
                        throw new InvalidOperationException(String.Format("������������������ {0}.{1} �����ϳ����ظ���ֵ: {2}", typeof(T).FullName, memberInfo, memberValue));
                    result.Add(memberValue, item);
                }
                return result;
            });
        }

        private void AddCache(T item)
        {
            foreach (KeyValuePair<MemberInfo, SynchronizedDictionary<object, T>> kvp in _cache)
            {
                object memberValue = Utilities.GetMemberValue(item, kvp.Key);
                if (kvp.Value.ContainsKey(memberValue))
                    throw new InvalidOperationException(String.Format("������������������ {0}.{1} ����������ظ���ֵ: {2}", typeof(T).FullName, kvp.Key.Name, memberValue));
                kvp.Value.Add(memberValue, item);
            }
        }

        private void RemoveCache(T item)
        {
            foreach (KeyValuePair<MemberInfo, SynchronizedDictionary<object, T>> kvp in _cache)
            {
                object memberValue = Utilities.GetMemberValue(item, kvp.Key);
                kvp.Value.Remove(memberValue);
            }
        }

        #region Add

        internal override void DoAdd(T item)
        {
            base.DoAdd(item);
            AddCache(item);
        }

        internal override void DoAddRange(IEnumerable<T> collection)
        {
            T[] enumerable = collection as T[] ?? collection.ToArray();
            base.DoAddRange(enumerable);
            foreach (T item in enumerable)
                AddCache(item);
        }

        #endregion

        #region Insert

        internal override void DoInsert(int index, T item)
        {
            base.DoInsert(index, item);
            AddCache(item);
        }

        internal override void DoInsertRange(int index, IEnumerable<T> collection)
        {
            T[] enumerable = collection as T[] ?? collection.ToArray();
            base.DoInsertRange(index, enumerable);
            foreach (T item in enumerable)
                AddCache(item);
        }

        #endregion

        #region Remove

        internal override bool DoRemove(T item)
        {
            RemoveCache(item);
            return base.DoRemove(item);
        }

        internal override int DoRemoveAll(Predicate<T> match)
        {
            foreach (T item in _infos)
                if (match(item))
                    RemoveCache(item);
            return base.DoRemoveAll(match);
        }

        internal override void DoRemoveAt(int index)
        {
            RemoveCache(_infos[index]);
            base.DoRemoveAt(index);
        }

        internal override void DoRemoveRange(int index, int count)
        {
            for (int i = index; i < index + count; i++)
                RemoveCache(_infos[i]);
            base.DoRemoveRange(index, count);
        }

        #endregion

        #region Clear

        internal override void DoClear()
        {
            _cache.Clear();
            base.DoClear();
        }

        #endregion

        #region Replace

        internal override void DoReplace(int index, T item)
        {
            RemoveCache(_infos[index]);
            base.DoReplace(index, item);
            AddCache(item);
        }

        #endregion
        
        #region Contains

        /// <summary>
        /// ȷ���Ƿ����ָ���ļ�
        /// </summary>
        /// <param name="keyLambda">�� lambda ���ʽ</param>
        /// <param name="key">��</param>
        public bool ContainsKey(Expression<Func<T, object>> keyLambda, object key)
        {
            return FetchCache(Utilities.GetMemberInfo(keyLambda)).ContainsKey(key);
        }

        /// <summary>
        /// ȷ���Ƿ�����ض�ֵ
        /// </summary>
        /// <param name="value">Ҫ��λ��ֵ. ������������, ��ֵ����Ϊ null</param>
        public bool ContainsValue(T value)
        {
            return Contains(value);
        }

        #endregion

        #region GetValue

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="keyLambda">�� lambda ���ʽ</param>
        /// <param name="key">��</param>
        /// <param name="value">���˷�������ֵʱ, ����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ��᷵�� item ����������Ĭ��ֵ</param>
        public bool TryGetValue(Expression<Func<T, object>> keyLambda, object key, out T value)
        {
            return FetchCache(Utilities.GetMemberInfo(keyLambda)).TryGetValue(key, out value);
        }

        #endregion

        #endregion
    }
}