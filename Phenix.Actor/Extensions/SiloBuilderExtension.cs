using System;
using System.IO;
using System.Reflection;
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
        /// 配置项见Phenix.Actor.OrleansConfig
        /// 设置集群ID、服务ID：Phenix.Core.Data.Database.Default.DataSourceKey
        /// 设置默认的激活体垃圾收集年龄限为2天
        /// 设置Clustering、GrainStorage、Reminder数据库：Phenix.Core.Data.Database.Default
        /// 设置Silo端口：EndpointOptions.DEFAULT_SILO_PORT
        /// 设置Gateway端口：EndpointOptions.DEFAULT_GATEWAY_PORT
        /// 设置SimpleMessageStreamProvider：Phenix.Actor.StreamProviderExtension.StreamProviderName
        /// 装配Actor插件
        /// 插件程序集都应该统一采用"*.Plugin.dll"、"*.Contract.dll"作为文件名的后缀
        /// 插件程序集都应该被部署到本服务容器的执行目录下
        /// </summary>
        /// <param name="builder">ISiloBuilder</param>
        /// <returns>ISiloBuilder</returns>
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder)
        {
            return ConfigureCluster(builder, OrleansConfig.ClusterId, OrleansConfig.ServiceId,
               OrleansConfig.DefaultGrainCollectionAgeMinutes, OrleansConfig.ConnectionString, OrleansConfig.DefaultSiloPort, OrleansConfig.DefaultGatewayPort);
        }

        /// <summary>
        /// 配置Orleans服务集群
        /// </summary>
        /// <param name="builder">ISiloBuilder</param>
        /// <param name="clusterId">Orleans集群的唯一ID</param>
        /// <param name="serviceId">Orleans服务的唯一ID</param>
        /// <param name="grainCollectionAgeMinutes">默认的激活体垃圾收集年龄限(分钟)</param>
        /// <param name="connectionString">Orleans数据库连接串</param>
        /// <param name="siloPort">Silo端口</param>
        /// <param name="gatewayPort">Gateway端口</param>
        /// <returns>ISiloBuilder</returns>
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder, string clusterId, string serviceId, 
            int grainCollectionAgeMinutes, string connectionString, int siloPort, int gatewayPort)
        {
            return builder
                .Configure<ConnectionOptions>(options => { options.ProtocolVersion = NetworkProtocolVersion.Version2; })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterId;
                    options.ServiceId = serviceId;
                })
                .Configure<GrainCollectionOptions>(options => { options.CollectionAge = TimeSpan.FromMinutes(grainCollectionAgeMinutes); })
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
                .ConfigureApplicationParts(parts =>
                {
                    /*
                     * 装配Actor插件
                     * 插件程序集都应该统一采用"*.Contract.dll"、"*.Plugin.dll"作为文件名的后缀
                     * 插件程序集都应该被部署到本服务容器的执行目录下
                     */
                    foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Contract.dll"))
                        parts.AddApplicationPart(Assembly.LoadFrom(fileName)).WithReferences().WithCodeGeneration();
                    foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Plugin.dll"))
                        parts.AddApplicationPart(Assembly.LoadFrom(fileName)).WithReferences().WithCodeGeneration();
                })
                .AddSimpleMessageStreamProvider(StreamProviderExtension.StreamProviderName)
                .AddMemoryGrainStorage("PubSubStore") //正式环境下请使用Event Hubs、ServiceBus、Azure Queues、Apache Kafka之一，要么自己实现PersistentStreamProvider组件
                .AddIncomingGrainCallFilter(context =>
                {
                    Identity.CurrentIdentity = context.Grain is ISecurityContext && RequestContext.Get(ContextConfig.CurrentIdentityName) is string identityName && RequestContext.Get(ContextConfig.CurrentIdentityCultureName) is string identityCultureName
                        ? Identity.Fetch(identityName, identityCultureName)
                        : null;

                    if (context.Grain is ITraceLogContext && RequestContext.Get(ContextConfig.traceKey) is long traceKey && RequestContext.Get(ContextConfig.traceOrder) is int traceOrder)
                    {
                        Task.Run(() => EventLog.Save(context.ImplementationMethod, Phenix.Core.Reflection.Utilities.JsonSerialize(context.Arguments), traceKey, traceOrder));
                        try
                        {
                            return context.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Task.Run(() => EventLog.Save(context.ImplementationMethod, Phenix.Core.Reflection.Utilities.JsonSerialize(context.Arguments), traceKey, traceOrder, ex));
                            throw;
                        }
                    }

                    return context.Invoke();
                });
        }
    }
}