using Phenix.Core;

namespace Phenix.Services.Host
{
    /// <summary>
    /// WebHost配置信息
    /// </summary>
    public static class WebHostConfig
    {
        private static string _urls;

        /// <summary>
        /// 服务地址
        /// </summary>
        public static string Urls
        {
            get { return AppSettings.GetLocalProperty(ref _urls, "http://*:5000"); }
            set { AppSettings.SetLocalProperty(ref _urls, value); }
        }
    }
}
