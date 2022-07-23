using System;
using System.Collections.Generic;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 泊位装备岸桥
    /// </summary>
    [Serializable]
    public readonly record struct BerthEquipQuayCranes
    {
        /// <summary>
        /// 泊位装备岸桥
        /// </summary>
        /// <param name="info">岸桥ID(从小到大坐标编排)</param>
        [Newtonsoft.Json.JsonConstructor]
        public BerthEquipQuayCranes(IList<long> info)
        {
            this.Info = info ??= new List<long>();
        }

        #region 属性

        /// <summary>
        /// 岸桥ID(从小到大坐标编排)
        /// </summary>
        public IList<long> Info { get; }

        #endregion
    }
}