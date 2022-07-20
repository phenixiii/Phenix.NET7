using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Phenix.Core.SyncCollections
{
    /// <summary>
    /// 表示双向链表
    /// </summary>
    /// <typeparam name="T">指定链表的元素类型</typeparam>
    [Serializable]
    public class SynchronizedLinkedList<T> : ICollection<T>, ISerializable
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public SynchronizedLinkedList()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new LinkedList<T>();
        }

        /// <summary>
        /// 初始化
        /// 该实例包含从指定的集合中复制的元素并且其容量足以容纳所复制的元素数
        /// </summary>
        /// <param name="collection">它的元素被复制到本实例中</param>
        public SynchronizedLinkedList(IEnumerable<T> collection)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new LinkedList<T>(collection);
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected SynchronizedLinkedList(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (LinkedList<T>) info.GetValue("_infos", typeof(LinkedList<T>));
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

        private readonly LinkedList<T> _infos;

        /// <summary>
        /// 获取实际包含的节点数
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
        /// 获取第一个节点
        /// </summary>
        public LinkedListNode<T> First
        {
            get
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    return _infos.First;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// 获取最后一个节点
        /// </summary>
        public LinkedListNode<T> Last
        {
            get
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    return _infos.Last;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
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
        /// 确定是否包含指定的值
        /// </summary>
        /// <param name="value">要定位的值</param>
        public bool Contains(T value)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.Contains(value);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Find

        /// <summary>
        /// 查找包含指定值的第一个节点
        /// </summary>
        /// <param name="value">要定位的值</param>
        public LinkedListNode<T> Find(T value)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.Find(value);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// 查找包含指定值的最后一个节点
        /// </summary>
        /// <param name="value">要定位的值</param>
        public LinkedListNode<T> FindLast(T value)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.FindLast(value);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// 在指定的现有节点后添加指定的新节点
        /// </summary>
        /// <param name="node">要在其后插入新节点的节点</param>
        /// <param name="newNode">要添加的新节点</param>
        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.AddAfter(node, newNode);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        ///  在指定的现有节点后添加包含指定值的新节点
        /// </summary>
        /// <param name="node">要在其后插入新节点的节点</param>
        /// <param name="value">要添加的新节点的值</param>
        public void AddAfter(LinkedListNode<T> node, T value)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.AddAfter(node, value);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 在指定的现有节点前添加指定的新节点
        /// </summary>
        /// <param name="node">要在其前插入新节点的节点</param>
        /// <param name="newNode">要添加的新节点</param>
        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.AddBefore(node, newNode);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 在指定的现有节点前添加包含指定值的新节点
        /// </summary>
        /// <param name="node">要在其前插入新节点的节点</param>
        /// <param name="value">要添加的新节点的值</param>
        public void AddBefore(LinkedListNode<T> node, T value)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.AddBefore(node, value);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 在开头处添加指定的新节点
        /// </summary>
        /// <param name="node">要添加的新节点</param>
        public void AddFirst(LinkedListNode<T> node)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.AddFirst(node);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 在开头处添加包含指定值的新节点
        /// </summary>
        /// <param name="value">要添加的新节点的值</param>
        public void AddFirst(T value)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.AddFirst(value);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 在结尾处添加指定的新节点
        /// </summary>
        /// <param name="node">要添加的新节点</param>
        public void AddLast(LinkedListNode<T> node)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.AddLast(node);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 在结尾处添加包含指定值的新节点
        /// </summary>
        /// <param name="value">要添加的新节点的值</param>
        public void AddLast(T value)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.AddLast(value);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        void ICollection<T>.Add(T item)
        {
            AddLast(item);
        }

        #endregion

        #region Remove

        /// <summary>
        /// 移除指定的节点
        /// </summary>
        /// <param name="node">要移除的节点</param>
        public void Remove(LinkedListNode<T> node)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Remove(node);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 移除指定值的第一个匹配项
        /// </summary>
        /// <param name="item">项</param>
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
        /// 移除开头处的节点
        /// </summary>
        public void RemoveFirst()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.RemoveFirst();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 移除结尾处的节点
        /// </summary>
        public void RemoveLast()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.RemoveLast();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
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
            LinkedList<T> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                result = new LinkedList<T>(_infos);
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
            LinkedList<T> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                result = new LinkedList<T>(_infos);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return result.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// 从指定的数组索引开始, 将元素复制到一个数组中
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="arrayIndex">数组索引</param>
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