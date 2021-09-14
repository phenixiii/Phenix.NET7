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

        private static int? _defaultGrainCollectionAgeMinutes;

        /// <summary>
        /// 默认的激活体垃圾回收年龄限
        /// 默认：60*2分钟
        /// </summary>
        public static int DefaultGrainCollectionAgeMinutes
        {
            get { return AppSettings.GetLocalProperty(ref _defaultGrainCollectionAgeMinutes, 60 * 2); }
            set { AppSettings.SetLocalProperty(ref _defaultGrainCollectionAgeMinutes, value); }
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
