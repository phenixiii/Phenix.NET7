using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Phenix.Core.SyncCollections
{
    /// <summary>
    /// 表示同一任意类型的实例的大小可变的后进先出(LIFO)集合
    /// </summary>
    /// <typeparam name="T">指定堆栈中的元素的类型</typeparam>
    [Serializable]
    public class SynchronizedStack<T> : IEnumerable<T>, ISerializable
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public SynchronizedStack()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new Stack<T>();
        }

        /// <summary>
        ///	初始化
        ///	该实例包含从指定的集合中复制的元素并且其容量足以容纳所复制的元素数
        /// </summary>
        /// <param name="collection">其元素被复制到新的集合中的集合</param>
        public SynchronizedStack(IEnumerable<T> collection)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new Stack<T>(collection);
        }

        /// <summary>
        /// 初始化
        /// 该实例为空并且具有指定的初始容量
        /// </summary>
        /// <param name="capacity">可包含的初始元素数</param>
        public SynchronizedStack(int capacity)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new Stack<T>(capacity);
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected SynchronizedStack(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (Stack<T>) info.GetValue("_infos", typeof(Stack<T>));
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

        private readonly Stack<T> _infos;

        /// <summary>
        /// 获取集合中包含的元素数
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

        #endregion

        #region 方法

        #region Contains

        /// <summary>
        /// 确定某元素是否在集合中
        /// </summary>
        /// <param name="item">要定位的对象. 对于引用类型, 该值可以为 null</param>
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

        #region Push

        /// <summary>
        /// 将对象插入集合的顶部
        /// </summary>
        /// <param name="item">要推入到集合中的对象. 对于引用类型, 该值可以为 null</param>
        /// <returns>是否成功添加</returns>
        public void Push(T item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Push(item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Pop

        /// <summary>
        /// 移除并返回位于集合顶部的对象
        /// </summary>
        public T Pop()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.Pop();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Peek

        /// <summary>
        /// 返回位于集合顶部的对象但不将其移除
        /// </summary>
        public T Peek()
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.Peek();
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Clear

        /// <summary>
        /// 移除所有节点
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

        #region IEnumerator

        /// <summary>
        /// 返回循环访问的枚举数, 为静态副本
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            Stack<T> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                result = new Stack<T>(_infos);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return result.GetEnumerator();
        }

        /// <summary>
        /// 返回循环访问的枚举数, 为静态副本
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            Stack<T> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                result = new Stack<T>(_infos);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return result.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// 如果元素数小于当前容量的 90%, 将容量设置为集合中的实际元素数
        /// </summary>
        public void TrimExcess()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.TrimExcess();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

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
        /// 从指定数组索引开始将集合中的元素复制到现有一维 Array 中
        /// </summary>
        /// <param name="array">作为从集合中复制的元素的目标位置的一维 Array. Array 必须具有从零开始的索引</param>
        /// <param name="arrayIndex">array 中从零开始的索引, 在此处开始复制</param>
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

        #endregion
    }
}