namespace Demo.InspectionStation.Plugin.Api
{
    /// <summary>
    /// WebAPI配置信息
    /// </summary>
    public static class ApiConfig
    {
        /// <summary>
        /// /api/center/listen
        /// </summary>
        public const string ApiCenterListenPath = "/api/inspection-station/center/listen";

        /// <summary>
        /// /api/operation-point/weighbridge
        /// </summary>
        public const string ApiOperationPointWeighbridgePath = "/api/inspection-station/operation-point/weighbridge";

        /// <summary>
        /// /api/operation-point/license-plate
        /// </summary>
        public const string ApiOperationPointLicensePlatePath = "/api/inspection-station/operation-point/license-plate";

        /// <summary>
        /// /api/operation-point/barrier
        /// </summary>
        public const string ApiOperationPointBarrierPath = "/api/inspection-station/operation-point/barrier";
    }
}
