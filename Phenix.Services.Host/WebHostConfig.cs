using Phenix.Core;

namespace Phenix.Services.Host
{
    public static class WebHostConfig
    {
        private static string _urls;
        /// <summary>
        /// Urls
        /// 默认：http://*:5000
        /// </summary>
        public static string Urls
        {
            get { return AppSettings.GetProperty(ref _urls, "http://*:5000"); }
            set { AppSettings.SetProperty(ref _urls, value); }
        }
    }
}
