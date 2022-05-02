using Phenix.Core;

namespace Phenix.Services.Host
{
    /// <summary>
    /// DaprClient配置信息
    /// </summary>
    public static class DaprClientConfig
    {
        private static string _httpPort;

        /// <summary>
        /// HttpPort
        /// 默认：3500（见Dapr.DaprDefaults.GetDefaultHttpEndpoint()）
        /// </summary>
        public static string HttpPort
        {
            get { return AppSettings.GetLocalProperty(ref _httpPort, "3500"); }
            set { AppSettings.SetLocalProperty(ref _httpPort, value); }
        }

        private static string _grpcPort;

        /// <summary>
        /// GrpcPort
        /// 默认：50001（见Dapr.DaprDefaults.GetDefaultGrpcEndpoint()）
        /// </summary>
        public static string GrpcPort
        {
            get { return AppSettings.GetLocalProperty(ref _grpcPort, "50001"); }
            set { AppSettings.SetLocalProperty(ref _grpcPort, value); }
        }
    }
}
