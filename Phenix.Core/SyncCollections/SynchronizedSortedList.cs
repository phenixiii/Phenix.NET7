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
    /// 表示键/值对的集合
    /// 这些键/值对基于关联的比较器实现按照键进行排序
    /// 支持 ICachedObject
    /// </summary>
    /// <typeparam name="TKey">集合中键的类型</typeparam>
    /// <typeparam name="TValue">集合中值的类型</typeparam>
    [Serializable]
    public class SynchronizedSortedList<TKey, TValue> : IDictionary<TKey, TValue>, ISerializable
    {
        /// <summary>
        /// 初始化
        /// 该实例为空且具有默认的初始容量, 并使用键类型的默认相等比较器
        /// </summary>
        public SynchronizedSortedList()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedList<TKey, TValue>();
        }

        /// <summary>
        /// 初始化
        /// 该实例为空且具有指定的初始容量, 并为键类型使用默认的相等比较器
        /// </summary>
        /// <param name="capacity">可包含的初始元素数</param>
        public SynchronizedSortedList(int capacity)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedList<TKey, TValue>(capacity);
        }

        /// <summary>
        /// 初始化
        /// 该实例包含从指定的集合中复制的元素并为键类型使用默认的相等比较器
        /// </summary>
        /// <param name="dictionary">它的元素被复制到本实例中</param>
        public SynchronizedSortedList(IDictionary<TKey, TValue> dictionary)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedList<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// 初始化
        /// 该实例为空且具有默认的初始容量，并使用指定的比较器
        /// </summary>
        /// <param name="comparer">比较器. 或为 null 以使用默认比较器</param>
        public SynchronizedSortedList(IComparer<TKey> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedList<TKey, TValue>(comparer);
        }

        /// <summary>
        /// 初始化
        /// 该实例包含从指定的集合中复制的元素, 并使用指定的比较器
        /// </summary>
        /// <param name="dictionary">它的元素被复制到新的集合中</param>
        /// <param name="comparer">比较器. 或为 null 以使用默认比较器</param>
        public SynchronizedSortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedList<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// 初始化
        /// 该实例为空且具有指定的初始容量，并使用指定的比较器
        /// </summary>
        /// <param name="capacity">可包含的初始元素数</param>
        /// <param name="comparer">比较器. 或为 null 以使用默认比较器</param>
        public SynchronizedSortedList(int capacity, IComparer<TKey> comparer)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new SortedList<TKey, TValue>(capacity, comparer);
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected SynchronizedSortedList(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (SortedList<TKey, TValue>) info.GetValue("_infos", typeof(SortedList<TKey, TValue>));
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

        private readonly SortedList<TKey, TValue> _infos;

        /// <summary>
        /// 获取用于对集合的元素进行排序的比较器
        /// </summary>
        public IComparer<TKey> Comparer
        {
            get { return _infos.Comparer; }
        }

        /// <summary>
        /// 获取包含在集合中的键/值对的数目
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
        /// 获取或设置与指定的键相关联的值
        /// </summary>
        /// <param name="key">要获取或设置的值的键</param>
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
        /// 获取键的集合, 为静态副本
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
        /// 获取值的集合, 为静态副本
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
        /// 是否只读
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

        /// <summary>
        /// 获取或设置集合可包含的元素数
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

        #region 方法

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
        /// 确定是否包含项
        /// </summary>
        /// <param name="item">项</param>
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
        /// 确定是否包含指定的键
        /// </summary>
        /// <param name="key">键</param>
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
        /// 确定是否包含特定值
        /// </summary>
        /// <param name="value">要定位的值. 对于引用类型, 该值可以为 null</param>
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
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">当此方法返回值时, 如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会返回 item 参数的类型默认值</param>
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
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="doCreate">如果没有该键, 构建值的函数</param>
        /// <param name="lockCreate">构建值时加锁</param>
        /// <param name="reset">如果找到该键, 是否重置</param>
        /// <returns>如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会执行 doCreate 函数构建构建 item 的值关联到键并返回</returns>
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
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="doCreate">如果没有该键, 构建值的函数</param>
        /// <param name="lockCreate">构建值时加锁</param>
        /// <param name="reset">如果找到该键, 是否重置的函数(null代表false)</param>
        /// <returns>如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会执行 doCreate 函数构建构建 item 的值关联到键并返回</returns>
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
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="doCreate">如果没有该键, 构建值的函数</param>
        /// <param name="lockCreate">构建值时加锁</param>
        /// <param name="reset">如果找到该键, 是否重置</param>
        /// <returns>如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会执行 doCreate 函数构建构建 item 的值关联到键并返回</returns>
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
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="doCreate">如果没有该键, 构建值的函数</param>
        /// <param name="lockCreate">构建值时加锁</param>
        /// <param name="reset">如果找到该键, 是否重置的函数(null代表false)</param>
        /// <returns>如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会执行 doCreate 函数构建构建 item 的值关联到键并返回</returns>
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

        #region IndexOf

        /// <summary>
        /// 在整个集合中搜索指定键并返回从零开始的索引
        /// </summary>
        /// <param name="key">要在集合中定位的键</param>
        public int IndexOfKey(TKey key)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TrimValidValue();
                return _infos.IndexOfKey(key);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// 在整个集合中搜索指定的值, 并返回第一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="value">要定位的值. 对于引用类型, 该值可以为 null</param>
        public int IndexOfValue(TValue value)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TrimValidValue();
                return _infos.IndexOfValue(value);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// 添加项
        /// </summary>
        /// <param name="item">项</param>
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
        /// 一次添加项(如果已含则不添加)
        /// </summary>
        /// <param name="item">项</param>
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
        /// 将指定的键和值添加到集合中
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">要添加的元素的值. 对于引用类型, 该值可以为 null</param>
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
        /// 一次将指定的键和值添加到字典中(如果已含则不添加)
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">要添加的元素的值. 对于引用类型, 该值可以为 null</param>
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
        /// 移除项
        /// </summary>
        /// <param name="item">项</param>
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
        /// 移除所指定的键的值
        /// </summary>
        /// <param name="key">键</param>
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
        /// 移除所指定的键的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="allow">是否允许的函数(null代表true)</param>
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

        /// <summary>
        /// 移除集合中的指定索引处的元素
        /// </summary>
        /// <param name="index">要移除的元素的从零开始的索引</param>
        public void RemoveAt(int index)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _infos.RemoveAt(index);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        #endregion

        #region Clear

        /// <summary>
        /// 移除所有的键和值
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
        /// 移除所有的键和值
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
        /// 替换值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="doReplace">替换值的函数</param>
        /// <param name="doSetIfNotFound">找不到时添加值</param>
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
        /// 返回循环访问的枚举数, 为静态副本
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            SortedList<TKey, TValue> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TrimValidValue();
                result = new SortedList<TKey, TValue>(_infos);
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
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            SortedList<TKey, TValue> result;

            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TrimValidValue();
                result = new SortedList<TKey, TValue>(_infos);
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
        /// 从指定的数组索引开始, 将元素复制到一个数组中
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="arrayIndex">数组索引</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                TrimValidValue();
                ((ICollection<KeyValuePair<TKey, TValue>>) _infos).CopyTo(array, arrayIndex);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        #endregion
    }
}