using System;
using System.Collections.Generic;
using System.Linq;
using Phenix.Algorithm.ElementaryStatistics;
using Phenix.Core;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 拖车作业周期
    /// </summary>
    [Serializable]
    public class VehicleCarryCycles
    {
        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected internal VehicleCarryCycles(IDictionary<long, AccumulatedMode> info = null)
        {
            _info = info;
        }

        #region 属性

        #region 配置项

        private static int? _precision;

        /// <summary>
        /// 统计精度(间隔X秒)
        /// 默认：10(>=1)
        /// </summary>
        public static int Precision
        {
            get { return new[] { AppSettings.GetProperty(ref _precision, 10), 1 }.Max(); }
            set { AppSettings.SetProperty(ref _precision, new[] { value, 1 }.Max()); }
        }

        #endregion

        private IDictionary<long, AccumulatedMode> _info;

        /// <summary>
        /// 箱区-拖车作业周期众数
        /// </summary>
        public IDictionary<long, AccumulatedMode> Info
        {
            get { return _info ??= new Dictionary<long, AccumulatedMode>(); }
        }

        #endregion

        #region 方法

        #region Event

        /// <summary>
        /// 拖车作业
        /// </summary>
        /// <param name="areaId">箱区ID</param>
        /// <param name="carryCycle">载运周期(秒)</param>
        public bool OnVehicleOperation(long areaId, int carryCycle)
        {
            return Info.GetValue(areaId, () => new AccumulatedMode(Precision)).Accumulate(carryCycle);
        }

        #endregion

        #endregion
    }
}