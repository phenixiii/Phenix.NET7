using Phenix.Core;

namespace Phenix.Services.Host
{
    public static class OrleansConfig
    {
        private static string _dashboardUsername;

        /// <summary>
        /// Dashboard登录用户名
        /// 默认：null
        /// </summary>
        public static string DashboardUsername
        {
            get { return AppSettings.GetLocalProperty(ref _dashboardUsername, (string) null); }
            set { AppSettings.SetLocalProperty(ref _dashboardUsername, value); }
        }

        private static string _dashboardPassword;

        /// <summary>
        /// Dashboard登录用户口令
        /// 默认：null
        /// </summary>
        public static string DashboardPassword
        {
            get { return AppSettings.GetLocalProperty(ref _dashboardPassword, (string) null, true); }
            set { AppSettings.SetLocalProperty(ref _dashboardPassword, value, true); }
        }

        private static string _dashboardHost;

        /// <summary>
        /// Dashboard绑定http主机名
        /// 默认：*
        /// </summary>
        public static string DashboardHost
        {
            get { return AppSettings.GetLocalProperty(ref _dashboardHost, "*"); }
            set { AppSettings.SetLocalProperty(ref _dashboardHost, value); }
        }

        private static int? _dashboardPort; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化

        /// <summary>
        /// Dashboard绑定http访问的端口
        /// 默认：8088
        /// </summary>
        public static int DashboardPort
        {
            get { return AppSettings.GetLocalProperty(ref _dashboardPort, 8088); }
            set { AppSettings.SetLocalProperty(ref _dashboardPort, value); }
        }

        private static bool? _dashboardHostSelf; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化

        /// <summary>
        /// Dashboard托管自己的http服务器
        /// 默认：true
        /// </summary>
        public static bool DashboardHostSelf
        {
            get { return AppSettings.GetLocalProperty(ref _dashboardHostSelf, true); }
            set { AppSettings.SetLocalProperty(ref _dashboardHostSelf, value); }
        }

        private static int? _dashboardCounterUpdateIntervalMs; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化

        /// <summary>
        /// Dashboard采样更新间隔(毫秒)
        /// 默认：10000
        /// </summary>
        public static int DashboardCounterUpdateIntervalMs
        {
            get { return AppSettings.GetLocalProperty(ref _dashboardCounterUpdateIntervalMs, 10000); }
            set { AppSettings.SetLocalProperty(ref _dashboardCounterUpdateIntervalMs, value); }
        }
    }
}
