using System;

namespace Phenix.iPost.CSS.Plugin.Business.Property
{
    /// <summary>
    /// 泊位属性
    /// </summary>
    [Serializable]
    public readonly record struct BerthProperty
    {
        /// <summary>
        /// 泊位属性
        /// </summary>
        /// <param name="terminalCode">码头代码</param>
        /// <param name="berthNo">泊位号(从左到右递增)</param>
        [Newtonsoft.Json.JsonConstructor]
        public BerthProperty(string terminalCode, int berthNo)
        {
            this.TerminalCode = terminalCode;
            this.BerthNo = berthNo;
        }

        /// <summary>
        /// 码头代码
        /// </summary>
        public string TerminalCode { get; }

        /// <summary>
        /// 泊位号(从左到右递增)
        /// </summary>
        public int BerthNo { get; }
    }
}