using System;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Log;
using Phenix.Core.Security;

namespace Orleans.Hosting
{
    /// <summary>
    /// SiloBuilder扩展
    /// </summary>
    public static class SiloBuilderExtension
    {
        /// <summary>
        /// 配置Orleans服务集群
        /// 设置集群ID、服务ID：Phenix.Core.Data.Database.Default.DataSourceKey
        /// 设置Clustering、GrainStorage、Reminder数据库：Phenix.Core.Data.Database.Default
        /// 设置Silo端口：EndpointOptions.DEFAULT_SILO_PORT
        /// 设置Gateway端口：EndpointOptions.DEFAULT_GATEWAY_PORT
        /// 设置SimpleMessageStreamProvider：Phenix.Actor.StreamProvider.Name
        /// </summary>
        /// <param name="builder">ISiloBuilder</param>
        /// <returns>ISiloBuilder</returns>
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder)
        {
            return ConfigureCluster(builder, OrleansConfig.ClusterId, OrleansConfig.ServiceId, OrleansConfig.ConnectionString, OrleansConfig.DefaultSiloPort, OrleansConfig.DefaultGatewayPort);
        }

        /// <summary>
        /// 配置Orleans服务集群
        /// </summary>
        /// <param name="builder">ISiloBuilder</param>
        /// <param name="clusterId">Orleans集群的唯一ID</param>
        /// <param name="serviceId">Orleans服务的唯一ID</param>
        /// <param name="connectionString">Orleans数据库连接串</param>
        /// <param name="siloPort">Silo端口</param>
        /// <param name="gatewayPort">Gateway端口</param>
        /// <returns>ISiloBuilder</returns>
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder, string clusterId, string serviceId, string connectionString,
            int siloPort, int gatewayPort)
        {
            return builder
                .Configure<ConnectionOptions>(options => { options.ProtocolVersion = NetworkProtocolVersion.Version2; })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterId;
                    options.ServiceId = serviceId;
                })
                .UseAdoNetClustering(options =>
                {
                    options.ConnectionString = connectionString;
#if PgSQL
                    options.Invariant = "Npgsql";
#endif
#if MsSQL
                    options.Invariant = "System.Data.SqlClient";
#endif
#if MySQL
                    options.Invariant = "MySql.Data.MySqlClient";
#endif
#if ORA
                    options.Invariant = "Oracle.DataAccess.Client";
#endif
                })
                .AddAdoNetGrainStorageAsDefault(options =>
                {
                    options.ConnectionString = connectionString;
#if PgSQL
                    options.Invariant = "Npgsql";
#endif
#if MsSQL
                    options.Invariant = "System.Data.SqlClient";
#endif
#if MySQL
                    options.Invariant = "MySql.Data.MySqlClient";
#endif
#if ORA
                    options.Invariant = "Oracle.DataAccess.Client";
#endif
                })
                .UseAdoNetReminderService(options =>
                {
                    options.ConnectionString = connectionString;
#if PgSQL
                    options.Invariant = "Npgsql";
#endif
#if MsSQL
                    options.Invariant = "System.Data.SqlClient";
#endif
#if MySQL
                    options.Invariant = "MySql.Data.MySqlClient";
#endif
#if ORA
                    options.Invariant = "Oracle.DataAccess.Client";
#endif
                })
                .ConfigureEndpoints(siloPort, gatewayPort)
                .ConfigureApplicationParts(parts => { parts.AddPluginPart(); })
                .AddSimpleMessageStreamProvider(StreamProviderExtension.StreamProviderName)
                .AddMemoryGrainStorage("PubSubStore") //正式环境下请使用Event Hubs、ServiceBus、Azure Queues、Apache Kafka，要么自己实现PersistentStreamProvider组件
                .AddIncomingGrainCallFilter(context =>
                {
                    Identity.CurrentIdentity = context.Grain is ISecurityContext
                        ? Identity.Fetch((string) RequestContext.Get(ContextConfig.CurrentIdentityName), (string) RequestContext.Get(ContextConfig.CurrentIdentityCultureName))
                        : null;

                    if (context.Grain is ITraceLogContext && RequestContext.Get(ContextConfig.PrimaryCallerKey) is long key)
                    {
                        EventLog.Save(context.ImplementationMethod, Phenix.Core.Reflection.Utilities.JsonSerialize(context.Arguments), key.ToString());
                        try
                        {
                            return context.Invoke();
                        }
                        catch (Exception ex)
                        {
                            EventLog.Save(context.ImplementationMethod, Phenix.Core.Reflection.Utilities.JsonSerialize(context.Arguments), key.ToString(), ex);
                        }
                    }

                    return context.Invoke();
                });
        }
    }
}