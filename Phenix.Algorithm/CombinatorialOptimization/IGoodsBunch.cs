using System.Collections.Generic;

namespace Phenix.Algorithm.CombinatorialOptimization
{
  /// <summary>
  /// 物品集束接口
  /// </summary>
  public interface IGoodsBunch : IGoods
  {
    #region 属性

    /// <summary>
    /// 子项
    /// </summary>
    IList<IGoods> Items { get; }

    #endregion
  }
}
