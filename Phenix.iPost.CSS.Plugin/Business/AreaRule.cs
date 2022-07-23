using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱区规范
    /// </summary>
    [Serializable]
    public readonly record struct AreaRule
    {
        /// <summary>
        /// 箱区规范
        /// </summary>
        /// <param name="bayNumber">贝数</param>
        /// <param name="rowNumber">排数</param>
        /// <param name="tierNumber">层数</param>
        /// <param name="emptyFull">空/重</param>
        /// <param name="isRefrigerant">是否冷箱</param>
        /// <param name="dangerousCode">危险品代码</param>
        [Newtonsoft.Json.JsonConstructor]
        public AreaRule(int bayNumber, int rowNumber, int tierNumber,
            EmptyFull emptyFull, bool isRefrigerant, string dangerousCode)
        {
            this.BayNumber = bayNumber;
            this.RowNumber = rowNumber;
            this.TierNumber = tierNumber;
            this.EmptyFull = emptyFull;
            this.IsRefrigerant = isRefrigerant;
            this.DangerousCode = dangerousCode;
        }
        
        #region 属性

        /// <summary>
        /// 贝数
        /// </summary>
        public int BayNumber { get; }

        /// <summary>
        /// 排数
        /// </summary>
        public int RowNumber { get; }

        /// <summary>
        /// 层数
        /// </summary>
        public int TierNumber { get; }

        /// <summary>
        /// 空/重
        /// </summary>
        public EmptyFull EmptyFull { get; }

        /// <summary>
        /// 是否冷箱
        /// </summary>
        public bool IsRefrigerant { get; }

        /// <summary>
        /// 危险品代码
        /// </summary>
        public string DangerousCode { get; }

        #endregion
    }
}