using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Phenix.Core.SyncCollections
{
    /// <summary>
    /// 表示元素的集合
    /// </summary>
    /// <typeparam name="T">元素的类型</typeparam>
    [Serializable]
    public class SynchronizedHashSet<T> : ISet<T>, ISerializable
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public SynchronizedHashSet()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new HashSet<T>();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public SynchronizedHashSet(IEnumerable<T> collection)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new HashSet<T>(collection);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public SynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new HashSet<T>(collection, comparer);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public SynchronizedHashSet(IEqualityComparer<T> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new HashSet<T>(comparer);
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected SynchronizedHashSet(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (HashSet<T>) info.GetValue("_infos", typeof(HashSet<T>));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        [System.Security.SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("_infos", _infos);
        }

        #endregion

        #region 属性

        [NonSerialized]
        private readonly ReaderWriterLock _rwLock;

        private readonly HashSet<T> _infos;

        /// <summary>
        /// 获取用于确定元素是否相等
        /// </summary>
        public IEqualityComparer<T> Comparer
        {
            get { return _infos.Comparer; }
        }

        /// <summary>
        /// 获取元素的数目
        /// </summary>
        public int Count
        {
            get
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    return _infos.Count;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// 获取元素的集合, 为静态副本
        /// </summary>
        public ICollection<T> Items
        {
            get
            {
                ICollection<T> result;

                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    result = new List<T>(_infos);
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }

                return result;
            }
        }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    return ((ICollection<T>) _infos).IsReadOnly;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
        }

        #endregion

        #region 方法

        #region Contains

        /// <summary>
        /// 确定是否包含指定的元素
        /// </summary>
        /// <param name="item">元素</param>
        public bool Contains(T item)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.Contains(item);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="item">元素</param>
        public bool Add(T item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.Add(item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        bool ISet<T>.Add(T item)
        {
            return Add(item);
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        #endregion

        #region Remove

        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="item">元素</param>
        public bool Remove(T item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.Remove(item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 移除与指定的谓词所定义的条件相匹配的所有元素
        /// </summary>
        /// <param name="match">用于定义要移除的元素应满足的条件</param>
        public int RemoveWhere(Predicate<T> match)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.RemoveWhere(match);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Clear

        /// <summary>
        /// 移除所有的元素
        /// </summary>
        public void Clear()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Clear();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 移除所有节点
        /// </summary>
        public void Clear(Action<IEnumerable<T>> doDispose)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                if (doDispose != null)
                    doDispose(new List<T>(_infos));
                _infos.Clear();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region 集合操作

        /// <summary>
        /// 包含存在于该对象中、指定集合中或两者中的所有元素
        /// </summary>
        /// <param name="other">指定集合</param>
        public void UnionWith(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.UnionWith(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 仅包含该对象和指定集合中存在的元素
        /// </summary>
        /// <param name="other">指定集合</param>
        public void IntersectWith(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.IntersectWith(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 移除指定集合中的所有元素
        /// </summary>
        /// <param name="other">指定集合</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.ExceptWith(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 仅包含存在于该对象中或存在于指定集合中的元素（但并非两者）
        /// </summary>
        /// <param name="other">指定集合</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.SymmetricExceptWith(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 是否为指定集合的子集
        /// </summary>
        /// <param name="other">指定集合</param>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.IsSubsetOf(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 是否为指定集合的超集
        /// </summary>
        /// <param name="other">指定集合</param>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.IsSupersetOf(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 是否为指定集合的真超集
        /// </summary>
        /// <param name="other">指定集合</param>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.IsProperSupersetOf(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 是否为指定集合的真子集
        /// </summary>
        /// <param name="other">指定集合</param>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.IsProperSubsetOf(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 是否和指定的集合共享通用元素
        /// </summary>
        /// <param name="other">指定集合</param>
        public bool Overlaps(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.Overlaps(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 是否和指定集合包含相同的元素
        /// </summary>
        /// <param name="other">指定集合</param>
        public bool SetEquals(IEnumerable<T> other)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.SetEquals(other);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region IEnumerator

        /// <summary>
        /// 返回循环访问的枚举数, 为静态副本
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            List<T> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                result = new List<T>(_infos);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return result.GetEnumerator();
        }

        /// <summary>
        ///	返回循环访问的枚举数, 为静态副本
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            List<T> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                result = new List<T>(_infos);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return result.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// 将集合的元素复制到新数组中
        /// <param name="clearSource">并清空源</param>
        /// </summary>
        public T[] ToArray(bool clearSource = false)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                T[] result = _infos.ToArray();
                if (clearSource)
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        _infos.Clear();
                    }
                    finally
                    {
                        _rwLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }

                return result;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// 将整个集合复制到兼容的一维数组中, 从目标数组的开头开始放置
        /// </summary>
        /// <param name="array">作为从集合中复制的元素的目标位置的一维 Array. Array 必须具有从零开始的索引</param>
        public void CopyTo(T[] array)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                _infos.CopyTo(array);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        ///  将整个集合复制到兼容的一维数组中, 从目标数组的指定索引位置开始放置
        /// </summary>
        /// <param name="array">作为从集合中复制的元素的目标位置的一维 Array. Array 必须具有从零开始的索引</param>
        /// <param name="arrayIndex">array 中从零开始的索引，在此处开始复制</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                _infos.CopyTo(array, arrayIndex);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// 将一定范围的元素从当前集合复制到兼容的一维数组中, 从目标数组的指定索引位置开始放置
        /// </summary>
        /// <param name="array">作为从集合中复制的元素的目标位置的一维 Array. Array 必须具有从零开始的索引</param>
        /// <param name="arrayIndex">array 中从零开始的索引, 在此处开始复制</param>
        /// <param name="count">要复制的元素数</param>
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                _infos.CopyTo(array, arrayIndex, count);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion
    }
}