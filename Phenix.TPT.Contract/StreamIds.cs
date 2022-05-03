using System;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// Stream配置信息
    /// </summary>
    public static class StreamIds
    {
        private static Guid _projectStreamId = new Guid("F18D8258-82FA-4BE9-A949-1F74EE907764");

        /// <summary>
        /// 项目StreamId
        /// </summary>
        public static Guid ProjectStreamId
        {
            get { return _projectStreamId; }
        }
    }
}
