using System;
using System.Collections.Generic;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 泊位装备岸桥
    /// </summary>
    [Serializable]
    public readonly record struct BerthEquipQuayCranesInfo
    {
        /// <summary>
        /// 泊位装备岸桥
        /// </summary>
        /// <param name="value">岸桥ID(从小到大坐标编排)</param>
        [Newtonsoft.Json.JsonConstructor]
        public BerthEquipQuayCranesInfo(IList<long> value)
        {
            this.Value = value ??= new List<long>();
        }

        #region 属性

        /// <summary>
        /// 岸桥ID(从小到大坐标编排)
        /// </summary>
        public IList<long> Value { get; }

        #endregion
    }
}