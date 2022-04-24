using Phenix.Core;

namespace Phenix.Services.Host
{
    /// <summary>
    /// Dashboard配置信息
    /// </summary>
    public static class DashboardConfig
    {
        private static string _username;

        /// <summary>
        /// 登录用户名
        /// 默认：null
        /// </summary>
        public static string Username
        {
            get { return AppSettings.GetLocalProperty(ref _username, (string) null); }
            set { AppSettings.SetLocalProperty(ref _username, value); }
        }

        private static string _password;

        /// <summary>
        /// 登录用户口令
        /// 默认：null
        /// </summary>
        public static string Password
        {
            get { return AppSettings.GetLocalProperty(ref _password, (string) null, true); }
            set { AppSettings.SetLocalProperty(ref _password, value, true); }
        }

        private static string _host;

        /// <summary>
        /// 绑定http主机名
        /// 默认：*
        /// </summary>
        public static string Host
        {
            get { return AppSettings.GetLocalProperty(ref _host, "*"); }
            set { AppSettings.SetLocalProperty(ref _host, value); }
        }

        private static int? _port; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化

        /// <summary>
        /// 绑定http访问的端口
        /// 默认：8088
        /// </summary>
        public static int Port
        {
            get { return AppSettings.GetLocalProperty(ref _port, 8088); }
            set { AppSettings.SetLocalProperty(ref _port, value); }
        }

        private static bool? _hostSelf; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化

        /// <summary>
        /// 托管自己的http服务器
        /// 默认：true
        /// </summary>
        public static bool HostSelf
        {
            get { return AppSettings.GetLocalProperty(ref _hostSelf, true); }
            set { AppSettings.SetLocalProperty(ref _hostSelf, value); }
        }

        private static int? _counterUpdateIntervalMs; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化

        /// <summary>
        /// 采样更新间隔(毫秒)
        /// 默认：10000
        /// </summary>
        public static int CounterUpdateIntervalMs
        {
            get { return AppSettings.GetLocalProperty(ref _counterUpdateIntervalMs, 10000); }
            set { AppSettings.SetLocalProperty(ref _counterUpdateIntervalMs, value); }
        }
    }
}
