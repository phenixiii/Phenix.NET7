using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Phenix.Core.SyncCollections
{
    /// <summary>
    /// ��ʾ˫������
    /// </summary>
    /// <typeparam name="T">ָ�������Ԫ������</typeparam>
    [Serializable]
    public class SynchronizedLinkedList<T> : ICollection<T>, ISerializable
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        public SynchronizedLinkedList()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new LinkedList<T>();
        }

        /// <summary>
        /// ��ʼ��
        /// ��ʵ��������ָ���ļ����и��Ƶ�Ԫ�ز����������������������Ƶ�Ԫ����
        /// </summary>
        /// <param name="collection">����Ԫ�ر����Ƶ���ʵ����</param>
        public SynchronizedLinkedList(IEnumerable<T> collection)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new LinkedList<T>(collection);
        }

        #region Serialization

        /// <summary>
        /// ���л�
        /// </summary>
        protected SynchronizedLinkedList(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (LinkedList<T>) info.GetValue("_infos", typeof(LinkedList<T>));
        }

        /// <summary>
        /// �����л�
        /// </summary>
        [System.Security.SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("_infos", _infos);
        }

        #endregion

        #region ����

        [NonSerialized]
        private readonly ReaderWriterLock _rwLock;

        private readonly LinkedList<T> _infos;

        /// <summary>
        /// ��ȡʵ�ʰ����Ľڵ���
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
        /// ��ȡ��һ���ڵ�
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
        /// ��ȡ���һ���ڵ�
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
        /// �Ƿ�ֻ��
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

        #region ����

        #region Contains

        /// <summary>
        /// ȷ���Ƿ����ָ����ֵ
        /// </summary>
        /// <param name="value">Ҫ��λ��ֵ</param>
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
        /// ���Ұ���ָ��ֵ�ĵ�һ���ڵ�
        /// </summary>
        /// <param name="value">Ҫ��λ��ֵ</param>
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
        /// ���Ұ���ָ��ֵ�����һ���ڵ�
        /// </summary>
        /// <param name="value">Ҫ��λ��ֵ</param>
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
        /// ��ָ�������нڵ�����ָ�����½ڵ�
        /// </summary>
        /// <param name="node">Ҫ���������½ڵ�Ľڵ�</param>
        /// <param name="newNode">Ҫ��ӵ��½ڵ�</param>
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
        ///  ��ָ�������нڵ����Ӱ���ָ��ֵ���½ڵ�
        /// </summary>
        /// <param name="node">Ҫ���������½ڵ�Ľڵ�</param>
        /// <param name="value">Ҫ��ӵ��½ڵ��ֵ</param>
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
        /// ��ָ�������нڵ�ǰ���ָ�����½ڵ�
        /// </summary>
        /// <param name="node">Ҫ����ǰ�����½ڵ�Ľڵ�</param>
        /// <param name="newNode">Ҫ��ӵ��½ڵ�</param>
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
        /// ��ָ�������нڵ�ǰ��Ӱ���ָ��ֵ���½ڵ�
        /// </summary>
        /// <param name="node">Ҫ����ǰ�����½ڵ�Ľڵ�</param>
        /// <param name="value">Ҫ��ӵ��½ڵ��ֵ</param>
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
        /// �ڿ�ͷ�����ָ�����½ڵ�
        /// </summary>
        /// <param name="node">Ҫ��ӵ��½ڵ�</param>
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
        /// �ڿ�ͷ����Ӱ���ָ��ֵ���½ڵ�
        /// </summary>
        /// <param name="value">Ҫ��ӵ��½ڵ��ֵ</param>
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
        /// �ڽ�β�����ָ�����½ڵ�
        /// </summary>
        /// <param name="node">Ҫ��ӵ��½ڵ�</param>
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
        /// �ڽ�β����Ӱ���ָ��ֵ���½ڵ�
        /// </summary>
        /// <param name="value">Ҫ��ӵ��½ڵ��ֵ</param>
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
        /// �Ƴ�ָ���Ľڵ�
        /// </summary>
        /// <param name="node">Ҫ�Ƴ��Ľڵ�</param>
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
        /// �Ƴ�ָ��ֵ�ĵ�һ��ƥ����
        /// </summary>
        /// <param name="item">��</param>
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
        /// �Ƴ���ͷ���Ľڵ�
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
        /// �Ƴ���β���Ľڵ�
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
        /// �Ƴ����нڵ�
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
        /// �Ƴ����нڵ�
        /// </summary>
        public void Clear(Action<IEnumerable<T>> doDispose)
        {
            if (doDispose == null)
                throw new ArgumentNullException(nameof(doDispose));

            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
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
        /// ����ѭ�����ʵ�ö����, Ϊ��̬����
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
        /// ����ѭ�����ʵ�ö����, Ϊ��̬����
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
        /// ��ָ��������������ʼ, ��Ԫ�ظ��Ƶ�һ��������
        /// </summary>
        /// <param name="array">����</param>
        /// <param name="arrayIndex">��������</param>
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