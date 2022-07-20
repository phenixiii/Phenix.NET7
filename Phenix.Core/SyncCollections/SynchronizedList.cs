using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading;

namespace Phenix.Core.SyncCollections
{
    /// <summary>
    /// 表示可通过索引访问的对象的强类型列表
    /// 提供用于对列表进行搜索、排序和操作的方法
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    [Serializable]
    public class SynchronizedList<T> : IList<T>, ISerializable
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public SynchronizedList()
        {
            _rwLock = new ReaderWriterLock();
            _infos = new List<T>();
        }

        /// <summary>
        /// 初始化
        /// 该实例包含从指定集合复制的元素并且具有足够的容量来容纳所复制的元素
        /// </summary>
        /// <param name="collection">一个集合, 其元素被复制到新列表中</param>
        public SynchronizedList(IEnumerable<T> collection)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new List<T>(collection);
        }

        /// <summary>
        /// 初始化
        /// 该实例为空并且具有指定的初始容量
        /// </summary>
        /// <param name="capacity">新列表最初可以存储的元素数</param>
        public SynchronizedList(int capacity)
        {
            _rwLock = new ReaderWriterLock();
            _infos = new List<T>(capacity);
        }

        #region Serialization

        /// <summary>
        /// 序列化
        /// </summary>
        protected SynchronizedList(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _rwLock = new ReaderWriterLock();
            _infos = (List<T>) info.GetValue("_infos", typeof(List<T>));
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

        /// <summary>
        /// 线程不安全的内容
        /// </summary>
        protected readonly List<T> _infos;

        /// <summary>
        /// 获取或设置该内部数据结构在不调整大小的情况下能够容纳的元素总数
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
        /// 获取实际包含的元素数
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
        /// 获取或设置指定索引处的元素
        /// </summary>
        /// <param name="index">要获得或设置的元素从零开始的索引</param>
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
        /// 是否只读
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

        #region 方法

        #region Contains

        /// <summary>
        /// 确定是否包含特定值
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

        #region BinarySearch

        /// <summary>
        /// 使用默认的比较器在整个已排序的当前集合中搜索元素, 并返回该元素从零开始的索引
        /// </summary>
        /// <param name="item">要定位的对象. 对于引用类型, 该值可以为 null</param>
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
        /// 使用指定的比较器在整个已排序的当前集合中搜索元素, 并返回该元素从零开始的索引
        /// </summary>
        /// <param name="item">要定位的对象. 对于引用类型, 该值可以为 null</param>
        /// <param name="comparer">比较器. 或为 null 以使用默认比较器</param>
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
        /// 使用指定的比较器在已排序的当前集合中的某个元素范围中搜索元素, 并返回该元素从零开始的索引
        /// </summary>
        /// <param name="index">要搜索的范围从零开始的起始索引</param>
        /// <param name="count">要搜索的范围的长度</param>
        /// <param name="item">要定位的对象. 对于引用类型, 该值可以为 null</param>
        /// <param name="comparer">比较器. 或为 null 以使用默认比较器</param>
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
        /// 搜索指定的对象, 并返回整个集合中第一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="item">要定位的对象. 对于引用类型, 该值可以为 null</param>
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
        /// 搜索指定的对象, 并返回集合中从指定索引到最后一个元素的元素范围内第一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="item">要定位的对象. 对于引用类型, 该值可以为 null</param>
        /// <param name="index">从零开始的搜索的起始索引</param>
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
        /// 搜索指定的对象, 并返回集合中从指定的索引开始并包含指定的元素数的元素范围内第一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="item">要定位的对象. 对于引用类型, 该值可以为 null</param>
        /// <param name="index">从零开始的搜索的起始索引</param>
        /// <param name="count">要搜索的部分中的元素数</param>
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
        /// 搜索指定的对象, 并返回整个集合中最后一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="item">要在集合中定位的对象. 对于引用类型, 该值可以为 null</param>
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
        /// 搜索指定的对象, 并返回集合中从第一个元素到指定索引的元素范围内最后一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="item">要在集合中定位的对象. 对于引用类型, 该值可以为 null</param>
        /// <param name="index">向后搜索的从零开始的起始索引</param>
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
        /// 搜索指定的对象, 并返回集合中包含指定的元素数并在指定索引处结束的元素范围内最后一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="item">要在集合中定位的对象. 对于引用类型, 该值可以为 null</param>
        /// <param name="index">向后搜索的从零开始的起始索引</param>
        /// <param name="count">要搜索的部分中的元素数</param>
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
        /// 确定是否包含与指定谓词所定义的条件相匹配的元素
        /// </summary>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 确定是否集合中的每个元素都与指定的谓词所定义的条件相匹配
        /// </summary>
        /// <param name="match">要据以检查元素的条件</param>
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
        /// 搜索与指定谓词所定义的条件相匹配的元素，并返回整个集合中的第一个匹配元素
        /// </summary>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 检索与指定谓词定义的条件匹配的所有元素
        /// </summary>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 搜索与指定谓词所定义的条件相匹配的元素, 并返回整个集合中第一个匹配元素的从零开始的索引
        /// </summary>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 搜索与指定谓词所定义的条件相匹配的元素, 并返回整个集合中从指定索引到最后一个元素的元素范围内第一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="startIndex">从零开始的搜索的起始索引</param>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 搜索与指定谓词所定义的条件相匹配的元素, 并返回整个集合中的最后一个匹配元素
        /// </summary>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 搜索与指定谓词所定义的条件相匹配的元素, 并返回整个集合中最后一个匹配元素的从零开始的索引
        /// </summary>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 搜索与由指定谓词定义的条件相匹配的元素, 并返回集合中从第一个元素到指定索引的元素范围内最后一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="startIndex">向后搜索的从零开始的起始索引</param>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 搜索与指定谓词所定义的条件相匹配的元素, 并返回集合中包含指定元素个数、到指定索引结束的元素范围内最后一个匹配项的从零开始的索引
        /// </summary>
        /// <param name="startIndex">向后搜索的从零开始的起始索引</param>
        /// <param name="count">要搜索的部分中的元素数</param>
        /// <param name="match">用于定义要搜索的元素应满足的条件</param>
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
        /// 将整个集合中元素的顺序反转
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
        /// 将指定范围中元素的顺序反转
        /// </summary>
        /// <param name="index">要反转的范围的从零开始的起始索引.</param>
        /// <param name="count">要反转的范围内的元素数</param>
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
        /// 使用默认比较器对整个集合中的元素进行排序
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
        /// 排序
        /// </summary>
        /// <param name="comparison">比较方法</param>
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
        /// 排序
        /// </summary>
        /// <param name="comparer">比较器. 或为 null 以使用默认比较器</param>
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
        /// 排序
        /// </summary>
        /// <param name="index">要排序的范围的从零开始的起始索引</param>
        /// <param name="count">要排序的范围的长度</param>
        /// <param name="comparer">比较器. 或为 null 以使用默认比较器</param>
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

        /// <summary>
        /// 将对象添加到结尾处
        /// </summary>
        /// <param name="item">要添加的对象. 对于引用类型, 该值可以为 null</param>
        protected virtual void DoAdd(T item)
        {
            _infos.Add(item);
        }

        /// <summary>
        /// 将对象添加到结尾处
        /// </summary>
        /// <param name="item">要添加的对象. 对于引用类型, 该值可以为 null</param>
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
        /// 一次将对象添加到结尾处(如果已含则不添加)
        /// </summary>
        /// <param name="item">要添加的对象. 对于引用类型, 该值可以为 null</param>
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

        /// <summary>
        /// 将指定集合的元素添加到末尾
        /// </summary>
        /// <param name="collection">一个集合, 其元素应被添加末尾. 集合自身不允许为 null, 但它可以包含 null 的元素(如果类型 T 为引用类型)</param>
        protected virtual void DoAddRange(IEnumerable<T> collection)
        {
            _infos.AddRange(collection);
        }

        /// <summary>
        /// 将指定集合的元素添加到末尾
        /// </summary>
        /// <param name="collection">一个集合, 其元素应被添加末尾. 集合自身不允许为 null, 但它可以包含 null 的元素(如果类型 T 为引用类型)</param>
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

        /// <summary>
        /// 将元素插入集合的指定索引处
        /// </summary>
        /// <param name="index">从零开始的索引, 应在该位置插入 item</param>
        /// <param name="item">要插入的对象. 对于引用类型, 该值可以为 null</param>
        protected virtual void DoInsert(int index, T item)
        {
            _infos.Insert(index, item);
        }

        /// <summary>
        /// 将元素插入集合的指定索引处
        /// </summary>
        /// <param name="index">从零开始的索引, 应在该位置插入 item</param>
        /// <param name="item">要插入的对象. 对于引用类型, 该值可以为 null</param>
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

        /// <summary>
        /// 将一个集合中的某个元素插入到集合的指定索引处
        /// </summary>
        /// <param name="index">应在此处插入新元素的从零开始的索引</param>
        /// <param name="collection">一个集合, 应将其元素插入到集合中. 该集合自身不允许为 null, 但它可以包含为 null 的元素(如果类型 T 为引用类型)</param>
        protected virtual void DoInsertRange(int index, IEnumerable<T> collection)
        {
            _infos.InsertRange(index, collection);
        }

        /// <summary>
        /// 将一个集合中的某个元素插入到集合的指定索引处
        /// </summary>
        /// <param name="index">应在此处插入新元素的从零开始的索引</param>
        /// <param name="collection">一个集合, 应将其元素插入到集合中. 该集合自身不允许为 null, 但它可以包含为 null 的元素(如果类型 T 为引用类型)</param>
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

        /// <summary>
        /// 从集合中移除特定对象的第一个匹配项
        /// </summary>
        /// <param name="item">要从集合中移除的对象. 对于引用类型, 该值可以为 null</param>
        protected virtual bool DoRemove(T item)
        {
            return _infos.Remove(item);
        }

        /// <summary>
        /// 从集合中移除特定对象的第一个匹配项
        /// </summary>
        /// <param name="item">要从集合中移除的对象. 对于引用类型, 该值可以为 null</param>
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

        /// <summary>
        /// 移除与指定的谓词所定义的条件相匹配的所有元素
        /// </summary>
        /// <param name="match">用于定义要移除的元素应满足的条件</param>
        protected virtual int DoRemoveAll(Predicate<T> match)
        {
            return _infos.RemoveAll(match);
        }

        /// <summary>
        /// 移除与指定的谓词所定义的条件相匹配的所有元素
        /// </summary>
        /// <param name="match">用于定义要移除的元素应满足的条件</param>
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

        /// <summary>
        /// 移除指定索引处的元素
        /// </summary>
        /// <param name="index">要移除的元素的从零开始的索引</param>
        protected virtual void DoRemoveAt(int index)
        {
            _infos.RemoveAt(index);
        }

        /// <summary>
        /// 移除指定索引处的元素
        /// </summary>
        /// <param name="index">要移除的元素的从零开始的索引</param>
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

        /// <summary>
        /// 移除一定范围的元素
        /// </summary>
        /// <param name="index">要移除的元素的范围从零开始的起始索引</param>
        /// <param name="count">要移除的元素数</param>
        protected virtual void DoRemoveRange(int index, int count)
        {
            _infos.RemoveRange(index, count);
        }

        /// <summary>
        /// 移除一定范围的元素
        /// </summary>
        /// <param name="index">要移除的元素的范围从零开始的起始索引</param>
        /// <param name="count">要移除的元素数</param>
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

        /// <summary>
        /// 移除所有元素
        /// </summary>
        protected virtual void DoClear()
        {
            _infos.Clear();
        }

        /// <summary>
        /// 移除所有元素
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
        /// 移除所有节点
        /// </summary>
        public void Clear(Action<IEnumerable<T>> doDispose)
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                if (doDispose != null)
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

        /// <summary>
        /// 替换值
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="item">要从集合中替换的对象. 对于引用类型, 该值可以为 null</param>
        protected virtual void DoReplace(int index, T item)
        {
            _infos[index] = item;
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
        /// 将容量设置为集合中的实际元素数目(如果该数目小于某个阈值)
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
        /// 对集合中每个元素执行指定操作
        /// </summary>
        /// <param name="action">要对集合中每个元素执行的委托</param>
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
        /// 创建源集合中的元素范围的浅表副本
        /// </summary>
        /// <param name="index">范围开始处的从零开始的集合索引</param>
        /// <param name="count">范围中的元素数</param>
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
        /// 返回当前集合的只读包装
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
        /// 将当前集合中的元素转换为另一种类型, 并返回包含转换后的元素的列表
        /// </summary>
        /// <typeparam name="TOutput">目标数组元素的类型</typeparam>
        /// <param name="converter">将每个元素从一种类型转换为另一种类型的委托</param>
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
        /// <param name="index">源集合中复制开始位置的从零开始的索引</param>
        /// <param name="array">作为从集合中复制的元素的目标位置的一维 Array. Array 必须具有从零开始的索引</param>
        /// <param name="arrayIndex">array 中从零开始的索引, 在此处开始复制</param>
        /// <param name="count">要复制的元素数</param>
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