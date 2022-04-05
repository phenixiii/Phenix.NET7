using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading;

namespace Phenix.Common.SyncCollections
{
    /// <summary>
    /// ��ʾ��ͨ���������ʵĶ����ǿ�����б�
    /// �ṩ���ڶ��б��������������Ͳ����ķ���
    /// </summary>
    /// <typeparam name="T">�б���Ԫ�ص�����</typeparam>
    [Serializable]
    public class SynchronizedList<T> : IList<T>, ISerializable
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        public SynchronizedList()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new List<T>();
        }

        /// <summary>
        /// ��ʼ��
        /// ��ʵ��������ָ�����ϸ��Ƶ�Ԫ�ز��Ҿ����㹻�����������������Ƶ�Ԫ��
        /// </summary>
        /// <param name="collection">һ������, ��Ԫ�ر����Ƶ����б���</param>
        public SynchronizedList(IEnumerable<T> collection)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new List<T>(collection);
        }

        /// <summary>
        /// ��ʼ��
        /// ��ʵ��Ϊ�ղ��Ҿ���ָ���ĳ�ʼ����
        /// </summary>
        /// <param name="capacity">���б�������Դ洢��Ԫ����</param>
        public SynchronizedList(int capacity)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new List<T>(capacity);
        }

        #region Serialization

        /// <summary>
        /// ���л�
        /// </summary>
        protected SynchronizedList(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (List<T>) info.GetValue("_infos", typeof(List<T>));
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

        internal readonly List<T> _infos;

        /// <summary>
        /// ��ȡ�����ø��ڲ����ݽṹ�ڲ�������С��������ܹ����ɵ�Ԫ������
        /// </summary>
        public int Capacity
        {
            get
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    return _infos.Capacity;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }

            set
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    _infos.Capacity = value;
                }
                finally
                {
                    _rwLock.ReleaseWriterLock();
                }
            }
        }

        /// <summary>
        /// ��ȡʵ�ʰ�����Ԫ����
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
        /// ��ȡ������ָ����������Ԫ��
        /// </summary>
        /// <param name="index">Ҫ��û����õ�Ԫ�ش��㿪ʼ������</param>
        public T this[int index]
        {
            get
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    return _infos[index];
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }

            set
            {
                _rwLock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    DoReplace(index, value);
                }
                finally
                {
                    _rwLock.ReleaseWriterLock();
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
                    return ((IList) _infos).IsReadOnly;
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
        /// ȷ���Ƿ�����ض�ֵ
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

        #region BinarySearch

        /// <summary>
        /// ʹ��Ĭ�ϵıȽ���������������ĵ�ǰ����������Ԫ��, �����ظ�Ԫ�ش��㿪ʼ������
        /// </summary>
        /// <param name="item">Ҫ��λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        public int BinarySearch(T item)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.BinarySearch(item);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ʹ��ָ���ıȽ���������������ĵ�ǰ����������Ԫ��, �����ظ�Ԫ�ش��㿪ʼ������
        /// </summary>
        /// <param name="item">Ҫ��λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        /// <param name="comparer">�Ƚ���. ��Ϊ null ��ʹ��Ĭ�ϱȽ���</param>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.BinarySearch(item, comparer);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ʹ��ָ���ıȽ�����������ĵ�ǰ�����е�ĳ��Ԫ�ط�Χ������Ԫ��, �����ظ�Ԫ�ش��㿪ʼ������
        /// </summary>
        /// <param name="index">Ҫ�����ķ�Χ���㿪ʼ����ʼ����</param>
        /// <param name="count">Ҫ�����ķ�Χ�ĳ���</param>
        /// <param name="item">Ҫ��λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        /// <param name="comparer">�Ƚ���. ��Ϊ null ��ʹ��Ĭ�ϱȽ���</param>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.BinarySearch(index, count, item, comparer);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region IndexOf

        /// <summary>
        /// ����ָ���Ķ���, ���������������е�һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="item">Ҫ��λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        public int IndexOf(T item)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.IndexOf(item);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ����ָ���Ķ���, �����ؼ����д�ָ�����������һ��Ԫ�ص�Ԫ�ط�Χ�ڵ�һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="item">Ҫ��λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        /// <param name="index">���㿪ʼ����������ʼ����</param>
        public int IndexOf(T item, int index)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.IndexOf(item, index);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ����ָ���Ķ���, �����ؼ����д�ָ����������ʼ������ָ����Ԫ������Ԫ�ط�Χ�ڵ�һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="item">Ҫ��λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        /// <param name="index">���㿪ʼ����������ʼ����</param>
        /// <param name="count">Ҫ�����Ĳ����е�Ԫ����</param>
        public int IndexOf(T item, int index, int count)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.IndexOf(item, index, count);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ����ָ���Ķ���, �������������������һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="item">Ҫ�ڼ����ж�λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        public int LastIndexOf(T item)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.LastIndexOf(item);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ����ָ���Ķ���, �����ؼ����дӵ�һ��Ԫ�ص�ָ��������Ԫ�ط�Χ�����һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="item">Ҫ�ڼ����ж�λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        /// <param name="index">��������Ĵ��㿪ʼ����ʼ����</param>
        public int LastIndexOf(T item, int index)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.LastIndexOf(item, index);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ����ָ���Ķ���, �����ؼ����а���ָ����Ԫ��������ָ��������������Ԫ�ط�Χ�����һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="item">Ҫ�ڼ����ж�λ�Ķ���. ������������, ��ֵ����Ϊ null</param>
        /// <param name="index">��������Ĵ��㿪ʼ����ʼ����</param>
        /// <param name="count">Ҫ�����Ĳ����е�Ԫ����</param>
        public int LastIndexOf(T item, int index, int count)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.LastIndexOf(item, index, count);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Exists

        /// <summary>
        /// ȷ���Ƿ������ָ��ν���������������ƥ���Ԫ��
        /// </summary>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public bool Exists(Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.Exists(match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ȷ���Ƿ񼯺��е�ÿ��Ԫ�ض���ָ����ν���������������ƥ��
        /// </summary>
        /// <param name="match">Ҫ���Լ��Ԫ�ص�����</param>
        public bool TrueForAll(Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.TrueForAll(match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Find

        /// <summary>
        /// ������ָ��ν���������������ƥ���Ԫ�أ����������������еĵ�һ��ƥ��Ԫ��
        /// </summary>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public T Find(Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.Find(match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ������ָ��ν�ʶ��������ƥ�������Ԫ��
        /// </summary>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public List<T> FindAll(Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.FindAll(match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ������ָ��ν���������������ƥ���Ԫ��, ���������������е�һ��ƥ��Ԫ�صĴ��㿪ʼ������
        /// </summary>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public int FindIndex(Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.FindIndex(match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ������ָ��ν���������������ƥ���Ԫ��, ���������������д�ָ�����������һ��Ԫ�ص�Ԫ�ط�Χ�ڵ�һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="startIndex">���㿪ʼ����������ʼ����</param>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.FindIndex(startIndex, match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ������ָ��ν���������������ƥ���Ԫ��, ���������������е����һ��ƥ��Ԫ��
        /// </summary>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public T FindLast(Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.FindLast(match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ������ָ��ν���������������ƥ���Ԫ��, �������������������һ��ƥ��Ԫ�صĴ��㿪ʼ������
        /// </summary>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public int FindLastIndex(Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.FindLastIndex(match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ��������ָ��ν�ʶ����������ƥ���Ԫ��, �����ؼ����дӵ�һ��Ԫ�ص�ָ��������Ԫ�ط�Χ�����һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="startIndex">��������Ĵ��㿪ʼ����ʼ����</param>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.FindLastIndex(startIndex, match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ������ָ��ν���������������ƥ���Ԫ��, �����ؼ����а���ָ��Ԫ�ظ�������ָ������������Ԫ�ط�Χ�����һ��ƥ����Ĵ��㿪ʼ������
        /// </summary>
        /// <param name="startIndex">��������Ĵ��㿪ʼ����ʼ����</param>
        /// <param name="count">Ҫ�����Ĳ����е�Ԫ����</param>
        /// <param name="match">���ڶ���Ҫ������Ԫ��Ӧ���������</param>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.FindLastIndex(startIndex, count, match);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Reverse

        /// <summary>
        /// ������������Ԫ�ص�˳��ת
        /// </summary>
        public void Reverse()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Reverse();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// ��ָ����Χ��Ԫ�ص�˳��ת
        /// </summary>
        /// <param name="index">Ҫ��ת�ķ�Χ�Ĵ��㿪ʼ����ʼ����.</param>
        /// <param name="count">Ҫ��ת�ķ�Χ�ڵ�Ԫ����</param>
        public void Reverse(int index, int count)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Reverse(index, count);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Sort

        /// <summary>
        /// ʹ��Ĭ�ϱȽ��������������е�Ԫ�ؽ�������
        /// </summary>
        public void Sort()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Sort();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="comparison">�ȽϷ���</param>
        public void Sort(Comparison<T> comparison)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Sort(comparison);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="comparer">�Ƚ���. ��Ϊ null ��ʹ��Ĭ�ϱȽ���</param>
        public void Sort(IComparer<T> comparer)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Sort(comparer);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="index">Ҫ����ķ�Χ�Ĵ��㿪ʼ����ʼ����</param>
        /// <param name="count">Ҫ����ķ�Χ�ĳ���</param>
        /// <param name="comparer">�Ƚ���. ��Ϊ null ��ʹ��Ĭ�ϱȽ���</param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Sort(index, count, comparer);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Add

        internal virtual void DoAdd(T item)
        {
            _infos.Add(item);
        }

        /// <summary>
        /// ��������ӵ���β��
        /// </summary>
        /// <param name="item">Ҫ��ӵĶ���. ������������, ��ֵ����Ϊ null</param>
        public void Add(T item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                DoAdd(item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// һ�ν�������ӵ���β��(����Ѻ������)
        /// </summary>
        /// <param name="item">Ҫ��ӵĶ���. ������������, ��ֵ����Ϊ null</param>
        public bool AddOnce(T item)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (!_infos.Contains(item))
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        DoAdd(item);
                        return true;
                    }
                    finally
                    {
                        _rwLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }

                return false;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        internal virtual void DoAddRange(IEnumerable<T> collection)
        {
            _infos.AddRange(collection);
        }

        /// <summary>
        /// ��ָ�����ϵ�Ԫ����ӵ�ĩβ
        /// </summary>
        /// <param name="collection">һ������, ��Ԫ��Ӧ�����ĩβ. ������������Ϊ null, �������԰��� null ��Ԫ��(������� T Ϊ��������)</param>
        public void AddRange(IEnumerable<T> collection)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                DoAddRange(collection);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Insert

        internal virtual void DoInsert(int index, T item)
        {
            _infos.Insert(index, item);
        }

        /// <summary>
        /// ��Ԫ�ز��뼯�ϵ�ָ��������
        /// </summary>
        /// <param name="index">���㿪ʼ������, Ӧ�ڸ�λ�ò��� item</param>
        /// <param name="item">Ҫ����Ķ���. ������������, ��ֵ����Ϊ null</param>
        public void Insert(int index, T item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                DoInsert(index, item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        internal virtual void DoInsertRange(int index, IEnumerable<T> collection)
        {
            _infos.InsertRange(index, collection);
        }

        /// <summary>
        /// ��һ�������е�ĳ��Ԫ�ز��뵽���ϵ�ָ��������
        /// </summary>
        /// <param name="index">Ӧ�ڴ˴�������Ԫ�صĴ��㿪ʼ������</param>
        /// <param name="collection">һ������, Ӧ����Ԫ�ز��뵽������. �ü�����������Ϊ null, �������԰���Ϊ null ��Ԫ��(������� T Ϊ��������)</param>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                DoInsertRange(index, collection);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Remove

        internal virtual bool DoRemove(T item)
        {
            return _infos.Remove(item);
        }

        /// <summary>
        /// �Ӽ������Ƴ��ض�����ĵ�һ��ƥ����
        /// </summary>
        /// <param name="item">Ҫ�Ӽ������Ƴ��Ķ���. ������������, ��ֵ����Ϊ null</param>
        public bool Remove(T item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return DoRemove(item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        internal virtual int DoRemoveAll(Predicate<T> match)
        {
            return _infos.RemoveAll(match);
        }

        /// <summary>
        /// �Ƴ���ָ����ν���������������ƥ�������Ԫ��
        /// </summary>
        /// <param name="match">���ڶ���Ҫ�Ƴ���Ԫ��Ӧ���������</param>
        public int RemoveAll(Predicate<T> match)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return DoRemoveAll(match);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        internal virtual void DoRemoveAt(int index)
        {
            _infos.RemoveAt(index);
        }

        /// <summary>
        /// �Ƴ�ָ����������Ԫ��
        /// </summary>
        /// <param name="index">Ҫ�Ƴ���Ԫ�صĴ��㿪ʼ������</param>
        public void RemoveAt(int index)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                DoRemoveAt(index);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        internal virtual void DoRemoveRange(int index, int count)
        {
            _infos.RemoveRange(index, count);
        }

        /// <summary>
        /// �Ƴ�һ����Χ��Ԫ��
        /// </summary>
        /// <param name="index">Ҫ�Ƴ���Ԫ�صķ�Χ���㿪ʼ����ʼ����</param>
        /// <param name="count">Ҫ�Ƴ���Ԫ����</param>
        public void RemoveRange(int index, int count)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                DoRemoveRange(index, count);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Clear

        internal virtual void DoClear()
        {
            _infos.Clear();
        }

        /// <summary>
        /// �Ƴ�����Ԫ��
        /// </summary>
        public void Clear()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                DoClear();
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
                DoClear();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Replace

        internal virtual void DoReplace(int index, T item)
        {
            _infos[index] = item;
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
        /// ����������Ϊ�����е�ʵ��Ԫ����Ŀ(�������ĿС��ĳ����ֵ)
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
        /// �Լ�����ÿ��Ԫ��ִ��ָ������
        /// </summary>
        /// <param name="action">Ҫ�Լ�����ÿ��Ԫ��ִ�е�ί��</param>
        public void ForEach(Action<T> action)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.ForEach(action);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// ����Դ�����е�Ԫ�ط�Χ��ǳ����
        /// </summary>
        /// <param name="index">��Χ��ʼ���Ĵ��㿪ʼ�ļ�������</param>
        /// <param name="count">��Χ�е�Ԫ����</param>
        public List<T> GetRange(int index, int count)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.GetRange(index, count);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ���ص�ǰ���ϵ�ֻ����װ
        /// </summary>
        public ReadOnlyCollection<T> AsReadOnly()
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.AsReadOnly();
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
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
                        DoClear();
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
        /// ����ǰ�����е�Ԫ��ת��Ϊ��һ������, �����ذ���ת�����Ԫ�ص��б�
        /// </summary>
        /// <typeparam name="TOutput">Ŀ������Ԫ�ص�����</typeparam>
        /// <param name="converter">��ÿ��Ԫ�ش�һ������ת��Ϊ��һ�����͵�ί��</param>
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.ConvertAll<TOutput>(converter);
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
        /// <param name="index">Դ�����и��ƿ�ʼλ�õĴ��㿪ʼ������</param>
        /// <param name="array">��Ϊ�Ӽ����и��Ƶ�Ԫ�ص�Ŀ��λ�õ�һά Array. Array ������д��㿪ʼ������</param>
        /// <param name="arrayIndex">array �д��㿪ʼ������, �ڴ˴���ʼ����</param>
        /// <param name="count">Ҫ���Ƶ�Ԫ����</param>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                _infos.CopyTo(index, array, arrayIndex, count);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion
    }
}