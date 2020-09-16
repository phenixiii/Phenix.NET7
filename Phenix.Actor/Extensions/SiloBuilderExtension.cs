using System;
using System.Threading.Tasks;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
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
                .AddMemoryGrainStorage("PubSubStore") //正式环境下请使用Event Hubs、ServiceBus、Azure Queues、Apache Kafka之一，要么自己实现PersistentStreamProvider组件
                .AddIncomingGrainCallFilter(async context =>
                {
                    Identity.CurrentIdentity = context.Grain is ISecurityContext && RequestContext.Get(ContextConfig.CurrentIdentityName) is string identityName && RequestContext.Get(ContextConfig.CurrentIdentityCultureName) is string identityCultureName
                        ? Identity.Fetch(identityName, identityCultureName)
                        : null;

                    if (context.Grain is ITraceLogContext && RequestContext.Get(ContextConfig.traceKey) is long traceKey && RequestContext.Get(ContextConfig.traceOrder) is int traceOrder)
                    {
                        Task.Run(() => EventLog.Save(context.ImplementationMethod, Phenix.Core.Reflection.Utilities.JsonSerialize(context.Arguments), traceKey, traceOrder));
                        try
                        {
                            await context.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Task.Run(() => EventLog.Save(context.ImplementationMethod, Phenix.Core.Reflection.Utilities.JsonSerialize(context.Arguments), traceKey, traceOrder, ex));
                            throw;
                        }
                    }
                    else
                        await context.Invoke();
                });
        }
    }
}