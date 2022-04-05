using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Phenix.Common.Reflection;

namespace Phenix.Common.SyncCollections
{
    /// <summary>
    /// 表示可通过索引访问的对象的强类型列表
    /// 提供用于对列表进行多维搜索、排序和操作的方法
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    [Serializable]
    public class SynchronizedMultiSortedList<T> : SynchronizedList<T>
        where T : class
    {
        #region 属性

        [NonSerialized]
        private readonly SynchronizedDictionary<MemberInfo, SynchronizedDictionary<object, T>> _cache =
            new SynchronizedDictionary<MemberInfo, SynchronizedDictionary<object, T>>();

        #endregion

        #region 方法

        private IDictionary<object, T> FetchCache(MemberInfo memberInfo)
        {
            return _cache.GetValue(memberInfo, () =>
            {
                SynchronizedDictionary<object, T> result = new SynchronizedDictionary<object, T>();
                foreach (T item in _infos)
                {
                    object memberValue = Utilities.GetMemberValue(item, memberInfo);
                    if (result.ContainsKey(memberValue))
                        throw new InvalidOperationException(String.Format("不允许在用于索引的 {0}.{1} 属性上出现重复的值: {2}", typeof(T).FullName, memberInfo, memberValue));
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
                    throw new InvalidOperationException(String.Format("不允许在用于索引的 {0}.{1} 属性上添加重复的值: {2}", typeof(T).FullName, kvp.Key.Name, memberValue));
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
        /// 确定是否包含指定的键
        /// </summary>
        /// <param name="keyLambda">键 lambda 表达式</param>
        /// <param name="key">键</param>
        public bool ContainsKey(Expression<Func<T, object>> keyLambda, object key)
        {
            return FetchCache(Utilities.GetMemberInfo(keyLambda)).ContainsKey(key);
        }

        /// <summary>
        /// 确定是否包含特定值
        /// </summary>
        /// <param name="value">要定位的值. 对于引用类型, 该值可以为 null</param>
        public bool ContainsValue(T value)
        {
            return Contains(value);
        }

        #endregion

        #region GetValue

        /// <summary>
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="keyLambda">键 lambda 表达式</param>
        /// <param name="key">键</param>
        /// <param name="value">当此方法返回值时, 如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会返回 item 参数的类型默认值</param>
        public bool TryGetValue(Expression<Func<T, object>> keyLambda, object key, out T value)
        {
            return FetchCache(Utilities.GetMemberInfo(keyLambda)).TryGetValue(key, out value);
        }

        #endregion

        #endregion
    }
}