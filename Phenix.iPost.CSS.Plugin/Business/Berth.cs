using System;
using System.Collections.Generic;
using System.Linq;
using Phenix.Core;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 泊位
    /// </summary>
    [Serializable]
    public class Berth
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected Berth()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected Berth(BerthProperty basis, IList<string> usableQuayCranes, IDictionary<string, IDictionary<int, long>> areasCarryCycles)
            : this(basis)
        {
            _usableQuayCranes = usableQuayCranes;
            _areasCarryCycles = areasCarryCycles;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="basis">基本要素</param>
        public Berth(BerthProperty basis)
        {
            _basis = basis;
        }

        #region 属性

        private readonly BerthProperty _basis;

        /// <summary>
        /// 基本要素
        /// </summary>
        public BerthProperty Basis
        {
            get { return _basis; }
        }

        private IList<string> _usableQuayCranes;

        /// <summary>
        /// 可用岸桥(从左到右编排)
        /// </summary>
        public IList<string> UsableQuayCranes
        {
            get { return _usableQuayCranes ??= new List<string>(); }
            private set { _usableQuayCranes = value; }
        }

        private IDictionary<string, IDictionary<int, long>> _areasCarryCycles;

        /// <summary>
        /// 箱区代码-拖车到箱区载运周期(秒)-次数
        /// </summary>
        public IDictionary<string, IDictionary<int, long>> AreasCarryCycles
        {
            get { return _areasCarryCycles ??= new Dictionary<string, IDictionary<int, long>>(); }
        }

        #region 配置项

        private static int? _statAreasCarryCyclePrecision;

        /// <summary>
        /// 拖车到箱区载运周期精度(间隔X秒)
        /// 默认：10(>=1)
        /// </summary>
        public static int StatAreasCarryCyclePrecision
        {
            get { return new[] { AppSettings.GetProperty(ref _statAreasCarryCyclePrecision, 10), 1 }.Max(); }
            set { AppSettings.SetProperty(ref _statAreasCarryCyclePrecision, new[] { value, 1 }.Max()); }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 统计拖车到箱区载运周期众数
        /// </summary>
        /// <param name="areaCode">箱区代码</param>
        /// <returns>拖车到箱区载运周期众数(秒)</returns>
        public int? StatAreasCarryCycleMode(string areaCode)
        {
            if (AreasCarryCycles.TryGetValue(areaCode, out IDictionary<int, long> value))
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

        /// <summary>
        /// 在拖车到箱区载运周期众数内
        /// </summary>
        /// <param name="areaCode">箱区代码</param>
        /// <param name="carryCycle">载运周期(秒)</param>
        public bool InAreasCarryCycleMode(string areaCode, int carryCycle)
        {
            int? mode = StatAreasCarryCycleMode(areaCode);
            return mode.HasValue && mode.Value == carryCycle / StatAreasCarryCyclePrecision * StatAreasCarryCyclePrecision;
        }

        #region Event

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="usableQuayCranes">可用岸桥(从左到右编排)</param>
        public void OnRefresh(IList<string> usableQuayCranes)
        {
            UsableQuayCranes = usableQuayCranes;
        }

        /// <summary>
        /// 拖车作业
        /// </summary>
        /// <param name="areaCode">箱区代码</param>
        /// <param name="carryCycle">载运周期(秒)</param>
        public bool OnVehicleOperation(string areaCode, int carryCycle)
        {
            IDictionary<int, long> value = AreasCarryCycles.GetValue(areaCode, () => new Dictionary<int, long>());
            int key = carryCycle / StatAreasCarryCyclePrecision * StatAreasCarryCyclePrecision;
            value.ReplaceValue(key, i => i + 1, () => 1);
            // 塔尖限制在正方形内
            if (value.TryGetValue(key, out long count))
            {
                long diff = count - AreasCarryCycles.Count;
                if (diff > 0)
                {
                    List<int> removedKeys = new List<int>();
                    List<int> keys2 = new List<int>();
                    foreach (KeyValuePair<int, long> kvp in value)
                        if (kvp.Key < diff)
                            removedKeys.Add(kvp.Key);
                        else
                            keys2.Add(kvp.Key);
                    foreach (int i in removedKeys)
                        value.Remove(i);
                    foreach (int i in keys2)
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