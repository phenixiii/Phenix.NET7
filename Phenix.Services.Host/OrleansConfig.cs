using Orleans.Configuration;
using Phenix.Core;
using Phenix.Core.Data;

namespace Phenix.Services.Host
{
    public static class OrleansConfig
    {
        private static string _clusterId;
        /// <summary>
        /// 集群的唯一ID
        /// 默认：Database.Default.DataSourceKey
        /// </summary>
        public static string ClusterId
        {
            get { return AppSettings.GetProperty(ref _clusterId, Database.Default.DataSourceKey); }
            set { AppSettings.SetProperty(ref _clusterId, value); }
        }

        private static string _serviceId;
        /// <summary>
        /// 服务的唯一ID
        /// 默认：Database.Default.DataSourceKey
        /// </summary>
        public static string ServiceId
        {
            get { return AppSettings.GetProperty(ref _serviceId, Database.Default.DataSourceKey); }
            set { AppSettings.SetProperty(ref _serviceId, value); }
        }

        private static int? _defaultSiloPort; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化
        /// <summary>
        /// Silo端口
        /// 默认：EndpointOptions.DEFAULT_SILO_PORT
        /// </summary>
        public static int DefaultSiloPort
        {
            get { return AppSettings.GetProperty(ref _defaultSiloPort, EndpointOptions.DEFAULT_SILO_PORT); }
            set { AppSettings.SetProperty(ref _defaultSiloPort, value); }
        }

        private static int? _defaultGatewayPort; //注意: 需将字段定义为Nullable<T>类型，以便AppSettings区分是否曾被自己初始化
        /// <summary>
        /// Gateway端口
        /// 默认：EndpointOptions.DEFAULT_GATEWAY_PORT
        /// </summary>
        public static int DefaultGatewayPort
        {
            get { return AppSettings.GetProperty(ref _defaultGatewayPort, EndpointOptions.DEFAULT_GATEWAY_PORT); }
            set { AppSettings.SetProperty(ref _defaultGatewayPort, value); }
        }
    }
}
