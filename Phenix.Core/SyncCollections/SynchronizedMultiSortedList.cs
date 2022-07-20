using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Phenix.Core.Reflection;

namespace Phenix.Core.SyncCollections
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

        /// <summary>
        /// 将对象添加到结尾处
        /// </summary>
        /// <param name="item">要添加的对象. 对于引用类型, 该值可以为 null</param>
        protected override void DoAdd(T item)
        {
            base.DoAdd(item);
            AddCache(item);
        }

        /// <summary>
        /// 将指定集合的元素添加到末尾
        /// </summary>
        /// <param name="collection">一个集合, 其元素应被添加末尾. 集合自身不允许为 null, 但它可以包含 null 的元素(如果类型 T 为引用类型)</param>
        protected override void DoAddRange(IEnumerable<T> collection)
        {
            T[] enumerable = collection as T[] ?? collection.ToArray();
            base.DoAddRange(enumerable);
            foreach (T item in enumerable)
                AddCache(item);
        }

        #endregion

        #region Insert

        /// <summary>
        /// 将元素插入集合的指定索引处
        /// </summary>
        /// <param name="index">从零开始的索引, 应在该位置插入 item</param>
        /// <param name="item">要插入的对象. 对于引用类型, 该值可以为 null</param>
        protected override void DoInsert(int index, T item)
        {
            base.DoInsert(index, item);
            AddCache(item);
        }

        /// <summary>
        /// 将一个集合中的某个元素插入到集合的指定索引处
        /// </summary>
        /// <param name="index">应在此处插入新元素的从零开始的索引</param>
        /// <param name="collection">一个集合, 应将其元素插入到集合中. 该集合自身不允许为 null, 但它可以包含为 null 的元素(如果类型 T 为引用类型)</param>
        protected override void DoInsertRange(int index, IEnumerable<T> collection)
        {
            T[] enumerable = collection as T[] ?? collection.ToArray();
            base.DoInsertRange(index, enumerable);
            foreach (T item in enumerable)
                AddCache(item);
        }

        #endregion

        #region Remove

        /// <summary>
        /// 从集合中移除特定对象的第一个匹配项
        /// </summary>
        /// <param name="item">要从集合中移除的对象. 对于引用类型, 该值可以为 null</param>
        protected override bool DoRemove(T item)
        {
            RemoveCache(item);
            return base.DoRemove(item);
        }

        /// <summary>
        /// 移除与指定的谓词所定义的条件相匹配的所有元素
        /// </summary>
        /// <param name="match">用于定义要移除的元素应满足的条件</param>
        protected override int DoRemoveAll(Predicate<T> match)
        {
            foreach (T item in _infos)
                if (match(item))
                    RemoveCache(item);
            return base.DoRemoveAll(match);
        }

        /// <summary>
        /// 移除指定索引处的元素
        /// </summary>
        /// <param name="index">要移除的元素的从零开始的索引</param>
        protected override void DoRemoveAt(int index)
        {
            RemoveCache(_infos[index]);
            base.DoRemoveAt(index);
        }

        /// <summary>
        /// 移除一定范围的元素
        /// </summary>
        /// <param name="index">要移除的元素的范围从零开始的起始索引</param>
        /// <param name="count">要移除的元素数</param>
        protected override void DoRemoveRange(int index, int count)
        {
            for (int i = index; i < index + count; i++)
                RemoveCache(_infos[i]);
            base.DoRemoveRange(index, count);
        }

        #endregion

        #region Clear

        /// <summary>
        /// 移除所有元素
        /// </summary>
        protected override void DoClear()
        {
            _cache.Clear();
            base.DoClear();
        }

        #endregion

        #region Replace

        /// <summary>
        /// 替换值
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="item">要从集合中替换的对象. 对于引用类型, 该值可以为 null</param>
        protected override void DoReplace(int index, T item)
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