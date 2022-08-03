using System;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 作业位置属性
    /// </summary>
    [Serializable]
    public readonly record struct OperationLocationInfo
    {
        /// <summary>
        /// 作业位置属性
        /// </summary>
        /// <param name="site">场地（装卸点/换电站/停车位）</param>
        /// <param name="bayNo">贝位（目的地是装卸点才有意义）</param>
        /// <param name="lane">车道（目的地是装卸点才有意义）</param>
        [Newtonsoft.Json.JsonConstructor]
        public OperationLocationInfo(string site, int bayNo, int lane)
        {
            this.Site = site;
            this.BayNo = bayNo;
            this.Lane = lane;
        }

        #region 属性

        /// <summary>
        /// 场地（装卸点/换电站/停车位）
        /// </summary>
        public string Site { get;}

        /// <summary>
        /// 贝位（目的地是装卸点才有意义）
        /// </summary>
        public int BayNo { get; }

        /// <summary>
        /// 车道（目的地是装卸点才有意义）
        /// </summary>
        public int Lane { get; }
        
        #endregion
    }
}
