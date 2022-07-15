using System;
using System.IO;
using System.Reflection;
using Orleans.Configuration;
using Orleans.Runtime.Messaging;
using Orleans.Serialization;
using Orleans.Versions.Compatibility;
using Orleans.Versions.Selector;
using Phenix.Actor;
using Phenix.Actor.Filters;
using Phenix.Core.Data;

namespace Orleans.Hosting
{
    /// <summary>
    /// SiloBuilder扩展
    /// </summary>
    public static class SiloBuilderExtension
    {
        /// <summary>
        /// 配置Orleans服务集群
        /// </summary>
        /// <param name="builder">ISiloBuilder</param>
        /// <param name="database">数据库入口</param>
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder, Database database)
        {
            return ConfigureCluster(builder, OrleansConfig.ClusterId, OrleansConfig.ServiceId,
                database != null ? database.ConnectionString : Database.Default.ConnectionString,
                TimeSpan.FromMinutes(OrleansConfig.DefaultGrainCollectionAgeMinutes),
                OrleansConfig.DefaultSiloPort, OrleansConfig.DefaultGatewayPort,
                OrleansConfig.ActiveStandbyMode);
        }

        /// <summary>
        /// 配置Orleans服务集群
        /// </summary>
        /// <param name="builder">ISiloBuilder</param>
        /// <param name="clusterId">Orleans集群的唯一ID</param>
        /// <param name="serviceId">Orleans服务的唯一ID</param>
        /// <param name="connectionString">Orleans数据库连接串</param>
        /// <param name="grainCollectionAge">默认的激活体垃圾回收年龄限</param>
        /// <param name="siloPort">Silo端口</param>
        /// <param name="gatewayPort">Gateway端口</param>
        /// <param name="activeStandbyMode">主备集群模式</param>
        public static ISiloBuilder ConfigureCluster(this ISiloBuilder builder, string clusterId, string serviceId,
            string connectionString, TimeSpan grainCollectionAge, int siloPort, int gatewayPort, bool activeStandbyMode)
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
                .Configure<GrainCollectionOptions>(options => { options.CollectionAge = grainCollectionAge; })
                .Configure<GrainVersioningOptions>(options =>
                {
                    if (activeStandbyMode) //主备集群流量切换升级策略
                    {
                        options.DefaultCompatibilityStrategy = nameof(BackwardCompatible); //向后兼容(默认)
                        options.DefaultVersionSelectorStrategy = nameof(MinimumVersion); //最小版本
                    }
                    else //单集群滚动服务升级策略
                    {
                        options.DefaultCompatibilityStrategy = nameof(BackwardCompatible); //向后兼容(默认)
                        options.DefaultVersionSelectorStrategy = nameof(AllCompatibleVersions); //全部兼容(默认)
                    }
                    //options.DefaultCompatibilityStrategy = nameof(BackwardCompatible); //向后兼容(默认)
                    //options.DefaultCompatibilityStrategy = nameof(AllVersionsCompatible); //任意兼容 
                    //options.DefaultCompatibilityStrategy = nameof(StrictVersionCompatible); //严格一致 
                    //options.DefaultVersionSelectorStrategy = nameof(AllCompatibleVersions); //全部兼容(默认)
                    //options.DefaultVersionSelectorStrategy = nameof(LatestVersion); //最新版本
                    //options.DefaultVersionSelectorStrategy = nameof(MinimumVersion); //最小版本
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
                    options.UseJsonFormat = true;
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
                .AddSimpleMessageStreamProvider(ContextKeys.SimpleMessageStreamProviderName)
                .AddMemoryGrainStorage("PubSubStore")
                .AddIncomingGrainCallFilter<IncomingGrainCallFilter>()
                .AddOutgoingGrainCallFilter<OutgoingGrainCallFilter>();
        }
    }
}