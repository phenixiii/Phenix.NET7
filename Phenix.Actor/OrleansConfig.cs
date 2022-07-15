using System.Linq;
using Orleans.Configuration;
using Phenix.Actor.Security;
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
        /// 默认的激活体垃圾回收年龄限(分钟)
        /// 默认：Principal.RequestIdleIntervalLimitMinutes(>=Principal.RequestIdleIntervalLimitMinutes)
        /// </summary>
        public static int DefaultGrainCollectionAgeMinutes
        {
            get { return new[] { AppSettings.GetLocalProperty(ref _defaultGrainCollectionAgeMinutes, User.RequestIdleIntervalLimitMinutes), User.RequestIdleIntervalLimitMinutes }.Max(); }
            set { AppSettings.SetLocalProperty(ref _defaultGrainCollectionAgeMinutes, new[] { value, User.RequestIdleIntervalLimitMinutes }.Max()); }
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

        private static bool? _activeStandbyMode;

        /// <summary>
        /// 主备集群模式
        /// 默认：单集群
        /// </summary>
        public static bool ActiveStandbyMode
        {
            get { return AppSettings.GetLocalProperty(ref _activeStandbyMode, false); }
            set { AppSettings.SetLocalProperty(ref _activeStandbyMode, value); }
        }
    }
}
