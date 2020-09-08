using Orleans.Configuration;
using Phenix.Core;
using Phenix.Core.Data;

namespace Phenix.Actor
{
    /// <summary>
    /// Orleans配置信息
    /// </summary>
    public static class OrleansConfig
    {
        private static string _clusterId;

        /// <summary>
        /// 集群的唯一ID
        /// 默认：Database.Default.DataSourceKey
        /// </summary>
        public static string ClusterId
        {
            get { return AppSettings.GetLocalProperty(ref _clusterId, Database.Default.DataSourceKey); }
            set { AppSettings.SetLocalProperty(ref _clusterId, value); }
        }

        private static string _serviceId;

        /// <summary>
        /// 服务的唯一ID
        /// 默认：Database.Default.DataSourceKey
        /// </summary>
        public static string ServiceId
        {
            get { return AppSettings.GetLocalProperty(ref _serviceId, Database.Default.DataSourceKey); }
            set { AppSettings.SetLocalProperty(ref _serviceId, value); }
        }

        private static string _connectionString;

        /// <summary>
        /// 数据库连接串
        /// 默认：Database.Default.ConnectionString
        /// </summary>
        public static string ConnectionString
        {
            get { return AppSettings.GetLocalProperty(ref _connectionString, Database.Default.ConnectionString, true); }
            set { AppSettings.SetLocalProperty(ref _connectionString, value, true); }
        }

        private static int? _defaultSiloPort;

        /// <summary>
        /// Silo端口
        /// 默认：EndpointOptions.DEFAULT_SILO_PORT
        /// </summary>
        public static int DefaultSiloPort
        {
            get { return AppSettings.GetLocalProperty(ref _defaultSiloPort, EndpointOptions.DEFAULT_SILO_PORT); }
            set { AppSettings.SetLocalProperty(ref _defaultSiloPort, value); }
        }

        private static int? _defaultGatewayPort;

        /// <summary>
        /// Gateway端口
        /// 默认：EndpointOptions.DEFAULT_GATEWAY_PORT
        /// </summary>
        public static int DefaultGatewayPort
        {
            get { return AppSettings.GetLocalProperty(ref _defaultGatewayPort, EndpointOptions.DEFAULT_GATEWAY_PORT); }
            set { AppSettings.SetLocalProperty(ref _defaultGatewayPort, value); }
        }
    }
}
