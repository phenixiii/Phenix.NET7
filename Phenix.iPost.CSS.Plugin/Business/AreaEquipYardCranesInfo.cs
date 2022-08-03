using System;
using System.Collections.Generic;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱区装备场桥
    /// </summary>
    [Serializable]
    public readonly record struct AreaEquipYardCranesInfo
    {
        /// <summary>
        /// 箱区装备场桥
        /// </summary>
        /// <param name="value">场桥ID(从小到大贝位编排)</param>
        [Newtonsoft.Json.JsonConstructor]
        public AreaEquipYardCranesInfo(IList<long> value)
        {
            this.Value = value ??= new List<long>();
        }

        #region 属性

        /// <summary>
        /// 场桥ID(从小到大贝位编排)
        /// </summary>
        public IList<long> Value { get; }

        #endregion
    }
}