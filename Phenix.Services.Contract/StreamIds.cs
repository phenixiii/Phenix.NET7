using System;

namespace Phenix.Services.Contract
{
    /// <summary>
    /// Stream配置信息
    /// </summary>
    public static class StreamIds
    {
        private static readonly Guid _groupStreamId = new Guid("54E67F1B-ABD7-4518-BA30-020A20AE3D49");

        /// <summary>
        /// GroupStreamId
        /// </summary>
        public static Guid GroupStreamId
        {
            get { return _groupStreamId; }
        }
    }
}
