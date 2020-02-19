using System;

namespace Demo.InspectionStation.Plugin
{
    /// <summary>
    /// 应用系统配置信息
    /// </summary>
    public static class AppConfig
    {
        private static readonly Guid _operationPointStreamId = new Guid("676849CA-5CF1-4AC4-B4D9-C13BF7C89EC6");

        /// <summary>
        /// OperationPoint的StreamId
        /// </summary>
        public static Guid OperationPointStreamId
        {
            get { return _operationPointStreamId; }
        }
    }
}