using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Phenix.Core.SyncCollections
{
    /// <summary>
    /// ��ʾԪ�صļ���
    /// </summary>
    /// <typeparam name="T">Ԫ�ص�����</typeparam>
    [Serializable]
    public class SynchronizedHashSet<T> : ISet<T>, ISerializable
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        public SynchronizedHashSet()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new HashSet<T>();
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public SynchronizedHashSet(IEnumerable<T> collection)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new HashSet<T>(collection);
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public SynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new HashSet<T>(collection, comparer);
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public SynchronizedHashSet(IEqualityComparer<T> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new HashSet<T>(comparer);
        }

        #region Serialization

        /// <summary>
        /// ���л�
        /// </summary>
        protected SynchronizedHashSet(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (HashSet<T>) info.GetValue("_infos", typeof(HashSet<T>));
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

        private readonly HashSet<T> _infos;

        /// <summary>
        /// ��ȡ����ȷ��Ԫ���Ƿ����
        /// </summary>
        public IEqualityComparer<T> Comparer
        {
            get { return _infos.Comparer; }
        }

        /// <summary>
        /// ��ȡԪ�ص���Ŀ
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
        /// ��ȡԪ�صļ���, Ϊ��̬����
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
        /// ȷ���Ƿ����ָ����Ԫ��
        /// </summary>
        /// <param name="item">Ԫ��</param>
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
        /// ���Ԫ��
        /// </summary>
        /// <param name="item">Ԫ��</param>
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
        /// �Ƴ�Ԫ��
        /// </summary>
        /// <param name="item">Ԫ��</param>
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
        /// �Ƴ���ָ����ν���������������ƥ�������Ԫ��
        /// </summary>
        /// <param name="match">���ڶ���Ҫ�Ƴ���Ԫ��Ӧ���������</param>
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
        /// �Ƴ����е�Ԫ��
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

        #region ���ϲ���

        /// <summary>
        /// ���������ڸö����С�ָ�������л������е�����Ԫ��
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �������ö����ָ�������д��ڵ�Ԫ��
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �Ƴ�ָ�������е�����Ԫ��
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �����������ڸö����л������ָ�������е�Ԫ�أ����������ߣ�
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �Ƿ�Ϊָ�����ϵ��Ӽ�
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �Ƿ�Ϊָ�����ϵĳ���
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �Ƿ�Ϊָ�����ϵ��泬��
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �Ƿ�Ϊָ�����ϵ����Ӽ�
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �Ƿ��ָ���ļ��Ϲ���ͨ��Ԫ��
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// �Ƿ��ָ�����ϰ�����ͬ��Ԫ��
        /// </summary>
        /// <param name="other">ָ������</param>
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
        /// ����ѭ�����ʵ�ö����, Ϊ��̬����
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
        ///	����ѭ�����ʵ�ö����, Ϊ��̬����
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
        /// ���������ϸ��Ƶ����ݵ�һά������, ��Ŀ������Ŀ�ͷ��ʼ����
        /// </summary>
        /// <param name="array">��Ϊ�Ӽ����и��Ƶ�Ԫ�ص�Ŀ��λ�õ�һά Array. Array ������д��㿪ʼ������</param>
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
        ///  ���������ϸ��Ƶ����ݵ�һά������, ��Ŀ�������ָ������λ�ÿ�ʼ����
        /// </summary>
        /// <param name="array">��Ϊ�Ӽ����и��Ƶ�Ԫ�ص�Ŀ��λ�õ�һά Array. Array ������д��㿪ʼ������</param>
        /// <param name="arrayIndex">array �д��㿪ʼ���������ڴ˴���ʼ����</param>
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
        /// ��һ����Χ��Ԫ�شӵ�ǰ���ϸ��Ƶ����ݵ�һά������, ��Ŀ�������ָ������λ�ÿ�ʼ����
        /// </summary>
        /// <param name="array">��Ϊ�Ӽ����и��Ƶ�Ԫ�ص�Ŀ��λ�õ�һά Array. Array ������д��㿪ʼ������</param>
        /// <param name="arrayIndex">array �д��㿪ʼ������, �ڴ˴���ʼ����</param>
        /// <param name="count">Ҫ���Ƶ�Ԫ����</param>
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