using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business.Property
{
    /// <summary>
    /// 船图箱属性
    /// </summary>
    /// <param name="Bay">贝</param>
    /// <param name="Row">排</param>
    /// <param name="Tier">层</param>
    /// <param name="ContainerNumber">箱号</param>
    /// <param name="ContainerOwner">持箱人</param>
    /// <param name="LadingBillNumber">提单号</param>
    /// <param name="EmptyFull">空/重</param>
    /// <param name="ImportExport">进口/出口（如为空则为过境箱）</param>
    /// <param name="LoadingPort">装货港</param>
    /// <param name="DischargingPort">卸货港</param>
    /// <param name="DestinationPort">目的港</param>
    /// <param name="TransferPort">转运港（如是本港则为转运箱）</param>
    /// <param name="ContainerType">箱型</param>
    /// <param name="ContainerSize">箱尺寸</param>
    /// <param name="IsoCode">ISO代码</param>
    /// <param name="Weight">重量t</param>
    /// <param name="OverHeight">是否超高</param>
    /// <param name="OverFrontLength">前超长</param>
    /// <param name="OverBackLength">后超长</param>
    /// <param name="OverLeftWidth">左超宽</param>
    /// <param name="OverRightWidth">右超宽</param>
    /// <param name="Refrigerated">是否冷藏</param>
    /// <param name="DangerousCode">危险品代码</param>
    [Serializable]
    public readonly record struct BayPlanContainerProperty(
        int Bay,
        int Row,
        int Tier,
        string ContainerNumber,
        string ContainerOwner,
        string LadingBillNumber,
        EmptyFull EmptyFull,
        ImportExport? ImportExport,
        string LoadingPort,
        string DischargingPort,
        string DestinationPort,
        string TransferPort,
        string ContainerType,
        string ContainerSize,
        string IsoCode,
        decimal Weight,
        bool OverHeight,
        decimal? OverFrontLength,
        decimal? OverBackLength,
        decimal? OverLeftWidth,
        decimal? OverRightWidth,
        bool Refrigerated,
        string DangerousCode)
    {
        #region 属性

        /// <summary>
        /// 是否超限
        /// </summary>
        public bool OverLimit { get; init; } = OverHeight || OverFrontLength.HasValue || OverBackLength.HasValue || OverLeftWidth.HasValue || OverRightWidth.HasValue;

        #endregion;
    }
}