using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Phenix.Common.SyncCollections
{
    /// <summary>
    /// ��ʾ������Ƚ��ȳ�����
    /// </summary>
    /// <typeparam name="T">ָ��������Ԫ�ص�����</typeparam>
    [Serializable]
    public class SynchronizedQueue<T> : IEnumerable<T>, ISerializable
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        public SynchronizedQueue()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new Queue<T>();
        }

        /// <summary>
        ///	��ʼ��
        ///	��ʵ��������ָ���ļ����и��Ƶ�Ԫ�ز����������������������Ƶ�Ԫ����
        /// </summary>
        /// <param name="collection">��Ԫ�ر����Ƶ��µļ����еļ���</param>
        public SynchronizedQueue(IEnumerable<T> collection)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new Queue<T>(collection);
        }

        /// <summary>
        /// ��ʼ��
        /// ��ʵ��Ϊ�ղ��Ҿ���ָ���ĳ�ʼ����
        /// </summary>
        /// <param name="capacity">�ɰ����ĳ�ʼԪ����</param>
        public SynchronizedQueue(int capacity)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new Queue<T>(capacity);
        }

        #region Serialization

        /// <summary>
        /// ���л�
        /// </summary>
        protected SynchronizedQueue(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (Queue<T>) info.GetValue("_infos", typeof(Queue<T>));
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

        private readonly Queue<T> _infos;

        /// <summary>
        /// ��ȡ�����а�����Ԫ����
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

        #region ����

        #region Contains

        /// <summary>
        /// ȷ��ĳԪ���Ƿ��ڼ�����
        /// </summary>
        /// <param name="item">Ҫ��λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
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

        #region Peek

        /// <summary>
        /// ����λ�ڼ��Ͽ�ʼ���Ķ��󵫲������Ƴ�
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

        /// <summary>
        /// ����λ�ڼ��Ͽ�ʼ���Ķ��󵫲������Ƴ�
        /// </summary>
        public bool TryPeek(out T result)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.TryPeek(out result);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Enqueue

        /// <summary>
        /// ��������ӵ����ϵĽ�β��
        /// </summary>
        /// <param name="item">Ҫ��ӵ������еĶ���. ������������, ��ֵ����Ϊ null</param>
        public void Enqueue(T item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Enqueue(item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Dequeue

        /// <summary>
        ///	�Ƴ�������λ�ڼ��Ͽ�ʼ���Ķ���
        /// </summary>
        public T Dequeue()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.Dequeue();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        ///	�Ƴ�������λ�ڼ��Ͽ�ʼ���Ķ���
        /// </summary>
        public bool TryDequeue(out T result)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.TryDequeue(out result);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Clear

        /// <summary>
        /// �Ƴ����ж���
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
            Queue<T> result;
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                result = new Queue<T>(_infos);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return result.GetEnumerator();
        }

        /// <summary>
        ///	����ѭ�����ʵ�ö����, Ϊ��̬����
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            Queue<T> result;
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                result = new Queue<T>(_infos);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }

            return result.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// ���Ԫ����С�ڵ�ǰ������ 90%, ����������Ϊ�����е�ʵ��Ԫ����
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
        /// �����ϵ�Ԫ�ظ��Ƶ���������
        /// <param name="clearSource">�����Դ</param>
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
        /// ��ָ������������ʼ�������е�Ԫ�ظ��Ƶ�����һά Array ��
        /// </summary>
        /// <param name="array">��Ϊ�Ӽ����и��Ƶ�Ԫ�ص�Ŀ��λ�õ�һά Array. Array ������д��㿪ʼ������</param>
        /// <param name="arrayIndex">array �д��㿪ʼ������, �ڴ˴���ʼ����</param>
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