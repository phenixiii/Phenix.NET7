using System;
using System.Collections.Generic;

namespace Phenix.Algorithm.ElementaryStatistics
{
    /// <summary>
    /// 堆积数值的众数
    /// </summary>
    [Serializable]
    public class AccumulatedMode
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected AccumulatedMode()
        {
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected AccumulatedMode(IDictionary<long, AccumulatedTimes> keyTimes, long mode, long maxTimes, DateTime lastActionTime, decimal precision)
            : this(precision)
        {
            _keyTimes = keyTimes ?? new Dictionary<long, AccumulatedTimes>();
            _mode = mode;
            _maxTimes = maxTimes;
            _lastActionTime = lastActionTime;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public AccumulatedMode(decimal precision)
        {
            _precision = precision != 0 ? precision : 1;
        }

        #region 属性

        private IDictionary<long, AccumulatedTimes> _keyTimes = new Dictionary<long, AccumulatedTimes>();

        /// <summary>
        /// 数值-规模
        /// </summary>
        public IDictionary<long, AccumulatedTimes> KeyTimes
        {
            get { return _keyTimes; }
        }

        private long _mode;

        /// <summary>
        /// 众数
        /// </summary>
        public long Mode
        {
            get { return _mode; }
        }

        private long _maxTimes;

        /// <summary>
        /// 重数
        /// </summary>
        public long MaxTimes
        {
            get { return _maxTimes; }
        }

        private DateTime _lastActionTime;

        /// <summary>
        /// 最近发生时间
        /// </summary>
        public DateTime LastActionTime
        {
            get { return _lastActionTime; }
        }

        private readonly decimal _precision = 1;

        /// <summary>
        /// 统计精度(Key间隔)
        /// </summary>
        public decimal Precision
        {
            get { return _precision; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 堆积
        /// </summary>
        /// <param name="key">数值</param>
        /// <param name="times">规模</param>
        public bool Accumulate(long key, long times = 1)
        {
            key = (int)Math.Round(key / Precision * Precision);
            lock (_keyTimes)
            {
                AccumulatedTimes keyTimes = _keyTimes.GetValue(key, () => new AccumulatedTimes());
                keyTimes.Accumulate(times);
                // 整理内容
                if (_maxTimes < keyTimes.Value)
                {
                    // 更换众数、重数
                    _mode = key;
                    _maxTimes = keyTimes.Value;
                    _lastActionTime = keyTimes.LastActionTime;
                }
                else
                {
                    // 清理对过过时的
                    long oppositeKey = _mode * 2 - key;
                    if (_keyTimes.TryGetValue(oppositeKey, out AccumulatedTimes opposite) &&
                        keyTimes.LastActionTime.Subtract(opposite.LastActionTime).TotalDays >= keyTimes.LastActionTime.Subtract(_lastActionTime).TotalDays * 2)
                        _keyTimes.Remove(oppositeKey);
                }
            }

            return false;
        }

        #endregion
    }
}