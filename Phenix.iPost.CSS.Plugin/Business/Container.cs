using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 箱属性
    /// </summary>
    [Serializable]
    public readonly record struct Container
    {
        /// <summary>
        /// 箱属性
        /// </summary>
        /// <param name="containerNumber">箱号</param>
        /// <param name="containerOwner">持箱人</param>
        /// <param name="voyage">航次</param>
        /// <param name="ladingBillNumber">提单号</param>
        /// <param name="importExport">进口/出口（如为空则为过境箱）</param>
        /// <param name="loadingPort">装货港</param>
        /// <param name="dischargingPort">卸货港</param>
        /// <param name="destinationPort">目的港</param>
        /// <param name="transferPort">转运港（如是本港则为转运箱）</param>
        /// <param name="containerType">箱型</param>
        /// <param name="containerSize">箱尺寸</param>
        /// <param name="isoCode">ISO代码</param>
        /// <param name="weight">重量t</param>
        /// <param name="overHeight">是否超高</param>
        /// <param name="overFrontLength">前超长</param>
        /// <param name="overBackLength">后超长</param>
        /// <param name="overLeftWidth">左超宽</param>
        /// <param name="overRightWidth">右超宽</param>
        /// <param name="emptyFull">空/重</param>
        /// <param name="isRefrigerant">是否冷箱</param>
        /// <param name="dangerousCode">危险品代码</param>
        /// <param name="bayNo">贝位</param>
        /// <param name="rowNo">排号</param>
        /// <param name="tierNo">层号</param>
        [Newtonsoft.Json.JsonConstructor]
        public Container(string containerNumber,
            string containerOwner, string voyage, string ladingBillNumber, ImportExport? importExport,
            string loadingPort, string dischargingPort, string destinationPort, string transferPort,
            string containerType, string containerSize, string isoCode,
            decimal weight, bool overHeight, decimal? overFrontLength, decimal? overBackLength, decimal? overLeftWidth, decimal? overRightWidth,
            EmptyFull emptyFull, bool isRefrigerant, string dangerousCode,
            int bayNo, int rowNo, int tierNo)
        {
            ContainerNumber = containerNumber;
            ContainerOwner = containerOwner;
            Voyage = voyage;
            LadingBillNumber = ladingBillNumber;
            ImportExport = importExport;
            LoadingPort = loadingPort;
            DischargingPort = dischargingPort;
            DestinationPort = destinationPort;
            TransferPort = transferPort;
            ContainerType = containerType;
            ContainerSize = containerSize;
            IsoCode = isoCode;
            Weight = weight;
            OverHeight = overHeight;
            OverFrontLength = overFrontLength;
            OverBackLength = overBackLength;
            OverLeftWidth = overLeftWidth;
            OverRightWidth = overRightWidth;
            EmptyFull = emptyFull;
            IsRefrigerant = isRefrigerant;
            DangerousCode = dangerousCode;
            BayNo = bayNo;
            RowNo = rowNo;
            TierNo = tierNo;
            _overLimit = overHeight || overFrontLength.HasValue || overBackLength.HasValue || overLeftWidth.HasValue || overRightWidth.HasValue;
        }

        #region 属性

        /// <summary>箱号</summary>
        public string ContainerNumber { get; }

        /// <summary>持箱人</summary>
        public string ContainerOwner { get; }

        /// <summary>航次</summary>
        public string Voyage { get; }

        /// <summary>提单号</summary>
        public string LadingBillNumber { get; }

        /// <summary>进口/出口（如为空则为过境箱）</summary>
        public ImportExport? ImportExport { get; }

        /// <summary>装货港</summary>
        public string LoadingPort { get; }

        /// <summary>卸货港</summary>
        public string DischargingPort { get; }

        /// <summary>目的港</summary>
        public string DestinationPort { get; }

        /// <summary>转运港（如是本港则为转运箱）</summary>
        public string TransferPort { get; }

        /// <summary>箱型</summary>
        public string ContainerType { get; }

        /// <summary>箱尺寸</summary>
        public string ContainerSize { get; }

        /// <summary>ISO代码</summary>
        public string IsoCode { get; }

        /// <summary>重量t</summary>
        public decimal Weight { get; }

        /// <summary>是否超高</summary>
        public bool OverHeight { get; }

        /// <summary>前超长</summary>
        public decimal? OverFrontLength { get; }

        /// <summary>后超长</summary>
        public decimal? OverBackLength { get; }

        /// <summary>左超宽</summary>
        public decimal? OverLeftWidth { get; }

        /// <summary>右超宽</summary>
        public decimal? OverRightWidth { get; }

        /// <summary>空/重</summary>
        public EmptyFull EmptyFull { get; }

        /// <summary>是否冷箱</summary>
        public bool IsRefrigerant { get; }

        /// <summary>危险品代码</summary>
        public string DangerousCode { get; }

        /// <summary>贝位</summary>
        public int BayNo { get; }

        /// <summary>排号</summary>
        public int RowNo { get; }

        /// <summary>层号</summary>
        public int TierNo { get; }

        [NonSerialized]
        private readonly bool _overLimit;

        /// <summary>
        /// 是否超限
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool OverLimit => _overLimit;

        #endregion;
    }
}