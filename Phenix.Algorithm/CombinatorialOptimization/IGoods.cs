namespace Phenix.Algorithm.CombinatorialOptimization
{
    /// <summary>
    /// 物品接口
    /// </summary>
    public interface IGoods
    {
        #region 属性

        /// <summary>
        /// 规格
        /// </summary>
        int Size { get; }

        /// <summary>
        /// 价值
        /// </summary>
        int Value { get; }

        #endregion
    }
}
