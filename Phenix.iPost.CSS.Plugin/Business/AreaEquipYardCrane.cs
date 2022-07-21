using System;
using System.Collections.Generic;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱区装备场桥
    /// </summary>
    [Serializable]
    public readonly record struct AreaEquipYardCrane
    {
        /// <summary>
        /// 箱区装备场桥
        /// </summary>
        /// <param name="yardCraneIds">场桥ID(从小到大贝位编排)</param>
        [Newtonsoft.Json.JsonConstructor]
        public AreaEquipYardCrane(IList<long> yardCraneIds)
        {
            this.YardCraneIds = yardCraneIds ??= new List<long>();
        }

        #region 属性

        /// <summary>
        /// 场桥ID(从小到大贝位编排)
        /// </summary>
        public IList<long> YardCraneIds { get; }

        #endregion
    }
}