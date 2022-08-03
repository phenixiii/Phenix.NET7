using System;

namespace Phenix.Algorithm.ElementaryStatistics
{
    /// <summary>
    /// 堆积数值规模
    /// </summary>
    [Serializable]
    public class AccumulatedTimes
    {
        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected internal AccumulatedTimes(long value = 0, DateTime lastActionTime = new DateTime())
        {
            _value = value;
            _lastActionTime = lastActionTime;
        }

        #region 属性

        private long _value;

        /// <summary>
        /// 规模
        /// </summary>
        public long Value
        {
            get { return _value; }
        }

        private DateTime _lastActionTime;

        /// <summary>
        /// 最近发生时间
        /// </summary>
        public DateTime LastActionTime
        {
            get { return _lastActionTime; }
        }

        #endregion

        #region 方法

        internal void Accumulate(long times)
        {
            _value = _value + times;
            _lastActionTime = DateTime.Now;
        }

        #endregion
    }
}