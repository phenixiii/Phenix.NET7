using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Orleans.Serialization;
using Phenix.Actor;
using Phenix.Core.Data;
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
        /// 
        /// 配置项见Phenix.Actor.OrleansConfig
        /// 设置集群ID、服务ID：Phenix.Core.Data.Database.Default.DataSourceKey
        /// 设置默认的激活体垃圾收集年龄限为：OrleansConfig.DefaultGrainCollectionAgeMinutes
        /// 设置Clustering、GrainStorage、Reminder数据库：Phenix.Core.Data.Database.Default
        /// 设置Silo端口：EndpointOptions.DEFAULT_SILO_PORT
        /// 设置Gateway端口：EndpointOptions.DEFAULT_GATEWAY_PORT
        /// 设置SimpleMessageStreamProvider：Phenix.Actor.StreamProviderExtension.StreamProviderName
        /// </summary>
        /// <param name="builder">ISiloBuilder</param>
        /// <param name="database">数据库入口</param>
        /// <returns>ISiloBuilder</returns>
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder, Database database)
        {
            return ConfigureCluster(builder, OrleansConfig.ClusterId, OrleansConfig.ServiceId, 
                database != null ? database.ConnectionString : Database.Default.ConnectionString,
                OrleansConfig.DefaultGrainCollectionAgeMinutes, OrleansConfig.DefaultSiloPort, OrleansConfig.DefaultGatewayPort);
        }

        /// <summary>
        /// 配置Orleans服务集群
        /// </summary>
        /// <param name="builder">ISiloBuilder</param>
        /// <param name="clusterId">Orleans集群的唯一ID</param>
        /// <param name="serviceId">Orleans服务的唯一ID</param>
        /// <param name="connectionString">Orleans数据库连接串</param>
        /// <param name="grainCollectionAgeMinutes">默认的激活体垃圾收集年龄限(分钟)</param>
        /// <param name="siloPort">Silo端口</param>
        /// <param name="gatewayPort">Gateway端口</param>
        /// <returns>ISiloBuilder</returns>
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder, string clusterId, string serviceId, string connectionString,
            int grainCollectionAgeMinutes, int siloPort, int gatewayPort)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder
                .Configure<SerializationProviderOptions>(options =>
                {
                    options.SerializationProviders.Add(typeof(BondSerializer));
                    options.FallbackSerializationProvider = typeof(BondSerializer);
                })
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
                     * 业务程序集都应该统一采用"*.Business.dll"作为文件名的后缀
                     * 契约程序集都应该统一采用"*.Contract.dll"作为文件名的后缀
                     * 插件程序集都应该统一采用"*.Plugin.dll"作为文件名的后缀
                     * 以上程序集都应该被部署到主程序的执行目录下
                     */
                    foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Business.dll"))
                        parts.AddApplicationPart(Assembly.LoadFrom(fileName)).WithReferences().WithCodeGeneration();
                    foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Contract.dll"))
                        parts.AddApplicationPart(Assembly.LoadFrom(fileName)).WithReferences().WithCodeGeneration();
                    foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Plugin.dll"))
                        parts.AddApplicationPart(Assembly.LoadFrom(fileName)).WithReferences().WithCodeGeneration();
                })
                .AddSimpleMessageStreamProvider(StreamProviderProxy.StreamProviderName)
                .AddMemoryGrainStorage("PubSubStore") //正式环境下请使用Event Hubs、ServiceBus、Azure Queues、Apache Kafka之一，要么自己实现PersistentStreamProvider组件
                .AddIncomingGrainCallFilter(context =>
                {
                    Principal.CurrentIdentity = context.Grain is ISecurityContext &&
                                                RequestContext.Get(ContextConfig.CurrentIdentityCompanyName) is string companyName &&
                                                RequestContext.Get(ContextConfig.CurrentIdentityUserName) is string userName &&
                                                RequestContext.Get(ContextConfig.CurrentIdentityCultureName) is string cultureName
                        ? Principal.FetchIdentity(companyName, userName, cultureName, null)
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