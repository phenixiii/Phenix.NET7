using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core.Threading;

namespace Phenix.Core.SyncCollections
{
    /// <summary>
    /// ��ʾ��������ļ�/ֵ�Եļ���
    /// ֧�� ICachedObject
    /// </summary>
    /// <typeparam name="TKey">�ֵ��еļ�������</typeparam>
    /// <typeparam name="TValue">�ֵ��е�ֵ������</typeparam>
    [Serializable]
    public class SynchronizedSortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializable
    {
        /// <summary>
        /// ��ʼ��
        /// ��ʵ��Ϊ��, ��ʹ�ü����͵�Ĭ����ȱȽ���
        /// </summary>
        public SynchronizedSortedDictionary()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedDictionary<TKey, TValue>();
        }

        /// <summary>
        /// ��ʼ��
        /// ��ʵ��������ָ���ļ����и��Ƶ�Ԫ��, ��ʹ�ü����͵�Ĭ�ϱ༭��
        /// </summary>
        /// <param name="dictionary">����Ԫ�ر����Ƶ��µļ�����</param>
        public SynchronizedSortedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedDictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// ��ʼ��
        /// ��ʵ��Ϊ�ղ�ʹ��ָ���ı༭��ʵ�����Ƚϼ�
        /// </summary>
        /// <param name="comparer">�Ƚ���. ��Ϊ null ��ʹ��Ĭ�ϱȽ���</param>
        public SynchronizedSortedDictionary(IComparer<TKey> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedDictionary<TKey, TValue>(comparer);
        }

        /// <summary>
        /// ��ʼ��
        /// ��ʵ��������ָ���ļ����и��Ƶ�Ԫ��, ��ʹ��ָ���ıȽ���
        /// </summary>
        /// <param name="dictionary">����Ԫ�ر����Ƶ��µļ�����</param>
        /// <param name="comparer">�Ƚ���. ��Ϊ null ��ʹ��Ĭ�ϱȽ���</param>
        public SynchronizedSortedDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedDictionary<TKey, TValue>(dictionary, comparer);
        }

        #region Serialization

        /// <summary>
        /// ���л�
        /// </summary>
        protected SynchronizedSortedDictionary(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (SortedDictionary<TKey, TValue>) info.GetValue("_infos", typeof(SortedDictionary<TKey, TValue>));
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

        private readonly SortedDictionary<TKey, TValue> _infos;

        /// <summary>
        /// ��ȡ���ڶԼ��ϵ�Ԫ�ؽ�������ıȽ���
        /// </summary>
        public IComparer<TKey> Comparer
        {
            get { return _infos.Comparer; }
        }

        /// <summary>
        /// ��ȡ�����ڼ����еļ�/ֵ�Ե���Ŀ
        /// </summary>
        public int Count
        {
            get
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    TrimValidValue();
                    return _infos.Count;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// ��ȡ��������ָ���ļ��������ֵ
        /// </summary>
        /// <param name="key">Ҫ��ȡ�����õ�ֵ�ļ�</param>
        public TValue this[TKey key]
        {
            get
            {
                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    TrimValidValue();
                    return _infos[key];
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
                    _infos[key] = value;
                }
                finally
                {
                    _rwLock.ReleaseWriterLock();
                }
            }
        }

        /// <summary>
        /// ��ȡ���ļ���, Ϊ��̬����
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                ICollection<TKey> result;

                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    TrimValidValue();
                    result = new List<TKey>(_infos.Keys);
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }

                return result;
            }
        }

        /// <summary>
        /// ��ȡֵ�ļ���, Ϊ��̬����
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> result;

                _rwLock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    TrimValidValue();
                    result = new List<TValue>(_infos.Values);
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
                    return ((ICollection<KeyValuePair<TKey, TValue>>) _infos).IsReadOnly;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
        }

        private static bool? _isCached;

        private static bool IsCached
        {
            get
            {
                if (!_isCached.HasValue)
                    _isCached = typeof(ICachedObject).IsAssignableFrom(typeof(TValue));
                return _isCached.Value;
            }
        }

        #endregion

        #region ����

        private void TrimValidValue()
        {
            if (IsCached)
            {
                List<TKey> invalids = new List<TKey>();
                foreach (KeyValuePair<TKey, TValue> kvp in _infos)
                    if (((ICachedObject) kvp.Value).IsInvalid)
                        invalids.Add(kvp.Key);

                if (invalids.Count > 0)
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        foreach (TKey item in invalids)
                            _infos.Remove(item);
                    }
                    finally
                    {
                        _rwLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }
            }
        }

        private bool IsValidValue(TKey key, TValue value)
        {
            if (IsCached)
            {
                if (key != null && value == null)
                    if (!_infos.TryGetValue(key, out value))
                        return false;

                if (((ICachedObject) value).IsInvalid)
                {
                    if (key != null)
                    {
                        LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                        try
                        {
                            _infos.Remove(key);
                        }
                        finally
                        {
                            _rwLock.DowngradeFromWriterLock(ref lockCookie);
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        #region Contains

        /// <summary>
        /// ȷ���Ƿ������
        /// </summary>
        /// <param name="item">��</param>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return IsValidValue(item.Key, item.Value) && ((ICollection<KeyValuePair<TKey, TValue>>) _infos).Contains(item);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ȷ���Ƿ����ָ���ļ�
        /// </summary>
        /// <param name="key">��</param>
        public bool ContainsKey(TKey key)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return IsValidValue(key, default) && _infos.ContainsKey(key);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ȷ���Ƿ�����ض�ֵ
        /// </summary>
        /// <param name="value">Ҫ��λ��ֵ. ������������, ��ֵ����Ϊ null</param>
        public bool ContainsValue(TValue value)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return IsValidValue(default, value) && _infos.ContainsValue(value);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region GetValue

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="value">���˷�������ֵʱ, ����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ��᷵�� item ����������Ĭ��ֵ</param>
        public bool TryGetValue(TKey key, out TValue value)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return _infos.TryGetValue(key, out value) && IsValidValue(key, value);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="doCreate">���û�иü�, ����ֵ�ĺ���</param>
        /// <param name="lockCreate">����ֵʱ����</param>
        /// <param name="reset">����ҵ��ü�, �Ƿ�����</param>
        /// <returns>����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ���ִ�� doCreate ������������ item ��ֵ��������������</returns>
        public TValue GetValue(TKey key, Func<TValue> doCreate, bool lockCreate = true, bool reset = false)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (!reset && _infos.TryGetValue(key, out TValue result))
                    return IsValidValue(key, result) ? result : default;
                return CreateValue(key, doCreate, lockCreate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="doCreate">���û�иü�, ����ֵ�ĺ���</param>
        /// <param name="lockCreate">����ֵʱ����</param>
        /// <param name="reset">����ҵ��ü�, �Ƿ����õĺ���(null����false)</param>
        /// <returns>����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ���ִ�� doCreate ������������ item ��ֵ��������������</returns>
        public TValue GetValue(TKey key, Func<TValue> doCreate, bool lockCreate, Func<TValue, bool> reset)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (_infos.TryGetValue(key, out TValue result))
                    if (reset == null || !reset(result))
                        return IsValidValue(key, result) ? result : default;
                return CreateValue(key, doCreate, lockCreate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        private TValue CreateValue(TKey key, Func<TValue> doCreate, bool lockCreate)
        {
            TValue result = default;
            if (!lockCreate)
                result = doCreate != null ? doCreate() : (TValue) Activator.CreateInstance(typeof(TValue), true);

            LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
            try
            {
                if (lockCreate)
                    result = doCreate != null ? doCreate() : (TValue) Activator.CreateInstance(typeof(TValue), true);
                _infos[key] = result;
                return result;
            }
            finally
            {
                _rwLock.DowngradeFromWriterLock(ref lockCookie);
            }
        }

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="doCreate">���û�иü�, ����ֵ�ĺ���</param>
        /// <param name="lockCreate">����ֵʱ����</param>
        /// <param name="reset">����ҵ��ü�, �Ƿ�����</param>
        /// <returns>����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ���ִ�� doCreate ������������ item ��ֵ��������������</returns>
        public TValue GetValue(TKey key, Func<Task<TValue>> doCreate, bool lockCreate = true, bool reset = false)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (!reset && _infos.TryGetValue(key, out TValue result))
                    return IsValidValue(key, result) ? result : default;
                return CreateValue(key, doCreate, lockCreate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="doCreate">���û�иü�, ����ֵ�ĺ���</param>
        /// <param name="lockCreate">����ֵʱ����</param>
        /// <param name="reset">����ҵ��ü�, �Ƿ����õĺ���(null����false)</param>
        /// <returns>����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ���ִ�� doCreate ������������ item ��ֵ��������������</returns>
        public TValue GetValue(TKey key, Func<Task<TValue>> doCreate, bool lockCreate, Func<TValue, bool> reset)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (_infos.TryGetValue(key, out TValue result))
                    if (reset == null || !reset(result))
                        return IsValidValue(key, result) ? result : default;
                return CreateValue(key, doCreate, lockCreate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        private TValue CreateValue(TKey key, Func<Task<TValue>> doCreate, bool lockCreate)
        {
            TValue result = default;
            if (!lockCreate)
                result = doCreate != null ? AsyncHelper.RunSync(doCreate) : (TValue) Activator.CreateInstance(typeof(TValue), true);

            LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
            try
            {
                if (lockCreate)
                    result = doCreate != null ? AsyncHelper.RunSync(doCreate) : (TValue) Activator.CreateInstance(typeof(TValue), true);
                _infos[key] = result;
                return result;
            }
            finally
            {
                _rwLock.DowngradeFromWriterLock(ref lockCookie);
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// �����
        /// </summary>
        /// <param name="item">��</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                ((ICollection<KeyValuePair<TKey, TValue>>) _infos).Add(item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// һ�������(����Ѻ������)
        /// </summary>
        /// <param name="item">��</param>
        public bool AddOnce(KeyValuePair<TKey, TValue> item)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (!((ICollection<KeyValuePair<TKey, TValue>>) _infos).Contains(item))
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        ((ICollection<KeyValuePair<TKey, TValue>>) _infos).Add(item);
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

        /// <summary>
        /// ��ָ���ļ���ֵ��ӵ��ֵ���
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="value">Ҫ��ӵ�Ԫ�ص�ֵ. ������������, ��ֵ����Ϊ null</param>
        public void Add(TKey key, TValue value)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.Add(key, value);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// һ�ν�ָ���ļ���ֵ��ӵ��ֵ���(����Ѻ������)
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="value">Ҫ��ӵ�Ԫ�ص�ֵ. ������������, ��ֵ����Ϊ null</param>
        public bool AddOnce(TKey key, TValue value)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (!_infos.ContainsKey(key))
                {
                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        _infos.Add(key, value);
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

        #endregion

        #region Remove

        /// <summary>
        /// �Ƴ���
        /// </summary>
        /// <param name="item">��</param>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>) _infos).Remove(item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// �Ƴ���ָ���ļ���ֵ
        /// </summary>
        /// <param name="key">��</param>
        public bool Remove(TKey key)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return _infos.Remove(key);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// �Ƴ���ָ���ļ���ֵ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="allow">�Ƿ�����ĺ���(null����true)</param>
        public bool Remove(TKey key, Func<TValue, bool> allow)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (_infos.TryGetValue(key, out TValue value))
                {
                    if (allow != null && !allow(value))
                        return false;

                    LockCookie lockCookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        return _infos.Remove(key);
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

        #endregion

        #region Clear

        /// <summary>
        /// �Ƴ����еļ���ֵ
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
        /// �Ƴ����еļ���ֵ
        /// </summary>
        public void Clear(Action<IEnumerable<KeyValuePair<TKey, TValue>>> doDispose)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                if (doDispose != null)
                    doDispose(new Dictionary<TKey, TValue>(_infos));
                _infos.Clear();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Replace

        /// <summary>
        /// �滻ֵ
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="doReplace">�滻ֵ�ĺ���</param>
        /// <param name="doSetIfNotFound">�Ҳ���ʱ���ֵ</param>
        public void ReplaceValue(TKey key, Func<TValue, TValue> doReplace, Func<TValue> doSetIfNotFound = null)
        {
            if (doReplace == null)
                throw new ArgumentNullException(nameof(doReplace));

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TValue result;
                if (_infos.TryGetValue(key, out TValue value))
                    result = doReplace(value);
                else if (doSetIfNotFound != null)
                    result = doSetIfNotFound();
                else
                    return;
                this[key] = result;
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region IEnumerator

        /// <summary>
        /// ����ѭ�����ʵ�ö����, Ϊ��̬����
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            SortedDictionary<TKey, TValue> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TrimValidValue();
                result = new SortedDictionary<TKey, TValue>(_infos);
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
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            SortedDictionary<TKey, TValue> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TrimValidValue();
                result = new SortedDictionary<TKey, TValue>(_infos);
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
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TrimValidValue();
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