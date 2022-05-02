using System;

namespace Phenix.Core.SyncCollections
{
    /// <summary>
    /// 缓存对象
    /// </summary>
    /// <typeparam name="TValue">被缓存对象</typeparam>
    [Serializable]
    public class CachedObject<TValue> : ICachedObject
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="value">被缓存对象</param>
        /// <param name="invalidTime">失效时间</param>
        [Newtonsoft.Json.JsonConstructor]
        public CachedObject(TValue value, DateTime invalidTime)
        {
            _value = value;
            _invalidTime = invalidTime;
        }

        #region 属性

        private readonly TValue _value;

        /// <summary>
        /// 被缓存对象
        /// </summary>
        public TValue Value
        {
            get { return _value; }
        }

        private DateTime _invalidTime;

        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime InvalidTime
        {
            get { return _invalidTime; }
            set { _invalidTime = value; }
        }

        /// <summary>
        /// 是否失效
        /// </summary>
        public bool IsInvalid
        {
            get { return _invalidTime < DateTime.Now; }
        }

        #endregion
    }
}