using Phenix.Core;

namespace Phenix.Services.Host
{
    /// <summary>
    /// DaprClient配置信息
    /// </summary>
    public static class DaprClientConfig
    {
        private static string _daprHttpPort;

        /// <summary>
        /// DaprHttpPort
        /// 默认：3500
        /// </summary>
        public static string DaprHttpPort
        {
            get { return AppSettings.GetLocalProperty(ref _daprHttpPort, "3500"); }
            set { AppSettings.SetLocalProperty(ref _daprHttpPort, value); }
        }

        private static string _daprGrpcPort;

        /// <summary>
        /// DaprGrpcPort
        /// 默认：3500
        /// </summary>
        public static string DaprGrpcPort
        {
            get { return AppSettings.GetLocalProperty(ref _daprGrpcPort, "50001"); }
            set { AppSettings.SetLocalProperty(ref _daprGrpcPort, value); }
        }
    }
}
