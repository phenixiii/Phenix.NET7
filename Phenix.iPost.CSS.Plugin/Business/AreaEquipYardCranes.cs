using System;
using System.Collections.Generic;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱区装备场桥
    /// </summary>
    [Serializable]
    public readonly record struct AreaEquipYardCranes
    {
        /// <summary>
        /// 箱区装备场桥
        /// </summary>
        /// <param name="info">场桥ID(从小到大贝位编排)</param>
        [Newtonsoft.Json.JsonConstructor]
        public AreaEquipYardCranes(IList<long> info)
        {
            this.Info = info ??= new List<long>();
        }

        #region 属性

        /// <summary>
        /// 场桥ID(从小到大贝位编排)
        /// </summary>
        public IList<long> Info { get; }

        #endregion
    }
}