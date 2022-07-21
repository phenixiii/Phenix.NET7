using System;
using System.Collections.Generic;
using System.Linq;
using Phenix.Core;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 泊位到箱区载运周期
    /// </summary>
    [Serializable]
    public class BerthAreaCarryCycle
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected internal BerthAreaCarryCycle()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected BerthAreaCarryCycle(IDictionary<long, IDictionary<int, long>> areaCarryCycles)
        {
            _areaCarryCycles = areaCarryCycles;
        }

        #region 属性

        private IDictionary<long, IDictionary<int, long>> _areaCarryCycles;

        /// <summary>
        /// 箱区ID-拖车从泊位到箱区载运周期(秒)-次数
        /// </summary>
        public IDictionary<long, IDictionary<int, long>> AreasCarryCycles
        {
            get { return _areaCarryCycles ??= new Dictionary<long, IDictionary<int, long>>(); }
        }

        #region 配置项

        private static int? _statAreaCarryCyclePrecision;

        /// <summary>
        /// 拖车从泊位到箱区载运周期统计精度(间隔X秒)
        /// 默认：10(>=1)
        /// </summary>
        public static int StatAreaCarryCyclePrecision
        {
            get { return new[] { AppSettings.GetProperty(ref _statAreaCarryCyclePrecision, 10), 1 }.Max(); }
            set { AppSettings.SetProperty(ref _statAreaCarryCyclePrecision, new[] { value, 1 }.Max()); }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 统计拖车从泊位到箱区载运周期众数
        /// </summary>
        /// <param name="areaId">箱区ID</param>
        /// <returns>拖车从泊位到箱区载运周期众数(秒)</returns>
        public int? StatAreaCarryCycleMode(long areaId)
        {
            if (AreasCarryCycles.TryGetValue(areaId, out IDictionary<int, long> value))
            {
                int result = int.MaxValue;
                long count = int.MinValue;
                foreach (KeyValuePair<int, long> kvp in value)
                    if (count < kvp.Value)
                    {
                        count = kvp.Value;
                        result = kvp.Key;
                    }

                return result;
            }

            return null;
        }

        #region Event

        /// <summary>
        /// 拖车作业
        /// </summary>
        /// <param name="areaId">箱区ID</param>
        /// <param name="carryCycle">载运周期(秒)</param>
        public bool OnVehicleOperation(long areaId, int carryCycle)
        {
            int key = carryCycle / StatAreaCarryCyclePrecision * StatAreaCarryCyclePrecision;
            IDictionary<int, long> value = AreasCarryCycles.GetValue(areaId, () => new Dictionary<int, long>());
            value.ReplaceValue(key, i => i + 1, () => 1);
            // 留下塔尖且限于正方形内
            if (value.TryGetValue(key, out long count))
            {
                long diff = count - AreasCarryCycles.Count;
                if (diff > 0)
                {
                    List<int> beRemoveKeys = new List<int>();
                    List<int> beTrimKeys = new List<int>();
                    foreach (KeyValuePair<int, long> kvp in value)
                        if (kvp.Value <= diff)
                            beRemoveKeys.Add(kvp.Key);
                        else
                            beTrimKeys.Add(kvp.Key);
                    foreach (int i in beRemoveKeys)
                        value.Remove(i);
                    foreach (int i in beTrimKeys)
                        value[i] = value[i] - diff;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion
    }
}