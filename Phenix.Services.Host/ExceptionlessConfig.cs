using Phenix.Core;

namespace Phenix.Services.Host
{
    /// <summary>
    /// Exceptionless配置信息
    /// </summary>
    public static class ExceptionlessConfig
    {
        private static string _serverUrl;

        /// <summary>
        /// 服务地址
        /// </summary>
        public static string ServerUrl
        {
            get { return AppSettings.GetLocalProperty(ref _serverUrl, "http://localhost:50000"); }
            set { AppSettings.SetLocalProperty(ref _serverUrl, value); }
        }

        private static string _apiKey;

        /// <summary>
        /// 服务密钥
        /// </summary>
        public static string ApiKey
        {
            get { return AppSettings.GetLocalProperty(ref _apiKey, "Please fill in the registered Exceptionless project ApiKey"); }
            set { AppSettings.SetLocalProperty(ref _apiKey, value); }
        }
    }
}
