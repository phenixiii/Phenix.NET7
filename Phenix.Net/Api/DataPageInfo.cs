using System;

namespace Phenix.Net.Api
{
    /// <summary>
    /// 数据页信息
    /// </summary>
    [Serializable]
    public sealed class DataPageInfo<T>
        where T : class
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        public DataPageInfo(string dataName, long dataSize, int pageNo, int pageSize, string pageBody)
            : this(dataSize, pageNo, pageSize, pageBody)
        {
            _dataName = dataName;
        }

        internal DataPageInfo(long dataSize, int pageNo, int pageSize, string pageBody)
        {
            _dataSize = dataSize;
            _pageNo = pageNo;
            _pageSize = pageSize;
            _pageBody = pageBody;
        }

        #region 属性

        /// <summary>
        /// 数据格式
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public T DataFormat
        {
            get { return default(T); }
        }

        private string _dataName;

        /// <summary>
        /// 数据名
        /// </summary>
        public string DataName
        {
            get { return _dataName ??= typeof(T).FullName; }
        }

        private readonly long _dataSize;

        /// <summary>
        /// 数据量
        /// </summary>
        public long DataSize
        {
            get { return _dataSize; }
        }

        private readonly int _pageNo;

        /// <summary>
        /// 页码(1..N, 0为不分页)
        /// </summary>
        public int PageNo
        {
            get { return _pageNo; }
        }

        private readonly int _pageSize;

        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
        }

        private readonly string _pageBody;

        /// <summary>
        /// 页体(JSON格式)
        /// </summary>
        public string PageBody
        {
            get { return _pageBody; }
        }

        #endregion
    }
}