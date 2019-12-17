using System.Collections.Generic;

namespace Phenix.Algorithm.KnapsackProblem
{
    /// <summary>
    /// 物品组接口
    /// </summary>
    public interface IGoodsGroup : IGoods
    {
        #region 属性

        /// <summary>
        /// 忽略
        /// </summary>
        bool Ignore { get; }

        /// <summary>
        /// 子项
        /// </summary>
        IList<IGoods> SubList { get; }

        #endregion
    }
}
