using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Orleans.Serialization;
using Orleans.Streams;
using Phenix.Core.Data;
using Phenix.Core.Log;
using Phenix.Core.Security;
using Phenix.Core.SyncCollections;
using Phenix.Core.Threading;

namespace Phenix.Actor
{
    /// <summary>
    /// Orleans服务集群客户端
    /// </summary>
    public static class ClusterClient
    {
        #region 属性

        private static readonly SynchronizedDictionary<string, IClusterClient> _cache = new SynchronizedDictionary<string, IClusterClient>(StringComparer.Ordinal);

        private static IClusterClient _default;

        /// <summary>
        /// 缺省Orleans服务集群客户端
        /// </summary>
        public static IClusterClient Default
        {
            get { return _default ??= Fetch(Database.Default); }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取Orleans服务集群客户端
        /// 设置集群ID、服务ID：Phenix.Core.Data.Database.Default.DataSourceKey
        /// 设置Clustering、GrainStorage、Reminder数据库：Phenix.Core.Data.Database.Default
        /// 设置SimpleMessageStreamProvider：Phenix.Actor.StreamProvider.Name
        /// </summary>
        /// <param name="database">数据库入口</param>
        /// <returns>Orleans服务集群客户端</returns>
        public static IClusterClient Fetch(Database database)
        {
            return Fetch(OrleansConfig.ClusterId, OrleansConfig.ServiceId, database.ConnectionString);
        }

        /// <summary>
        /// 获取Orleans服务集群客户端
        /// </summary>
        /// <param name="clusterId">Orleans集群的唯一ID</param>
        /// <param name="serviceId">Orleans服务的唯一ID</param>
        /// <param name="connectionString">Orleans数据库连接串</param>
        /// <returns>Orleans服务集群客户端</returns>
        public static IClusterClient Fetch(string clusterId, string serviceId, string connectionString)
        {
            return _cache.GetValue(Standards.FormatCompoundKey(clusterId, serviceId), () =>
            {
                IClusterClient value = new ClientBuilder()
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
                    .ConfigureApplicationParts(parts =>
                    {
                        /*
                         * 装配Actor协议
                         * 业务程序集都应该统一采用"*.Business.dll"作为文件名的后缀
                         * 契约程序集都应该统一采用"*.Contract.dll"作为文件名的后缀
                         * 以上程序集都应该被部署到主程序的执行目录下
                         */
                        foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Business.dll"))
                            parts.AddApplicationPart(Assembly.LoadFrom(fileName)).WithReferences().WithCodeGeneration();
                        foreach (string fileName in Directory.GetFiles(Phenix.Core.AppRun.BaseDirectory, "*.Contract.dll"))
                            parts.AddApplicationPart(Assembly.LoadFrom(fileName)).WithReferences().WithCodeGeneration();
                    })
                    .AddSimpleMessageStreamProvider(ContextKeys.SimpleMessageStreamProviderName)
                    .AddOutgoingGrainCallFilter(context =>
                    {
                        if (context.Grain is ISecurityContext)
                        {
                            IIdentity currentIdentity = Principal.CurrentIdentity;
                            if (currentIdentity != null)
                            {
                                RequestContext.Set(ContextKeys.CurrentIdentityCompanyName, currentIdentity.CompanyName);
                                RequestContext.Set(ContextKeys.CurrentIdentityUserName, currentIdentity.UserName);
                                RequestContext.Set(ContextKeys.CurrentIdentityCultureName, currentIdentity.CultureName);
                            }
                        }

                        if (context.Grain is ITraceLogContext)
                        {
                            long traceKey;
                            if (RequestContext.Get(ContextKeys.TraceKey) == null)
                            {
                                traceKey = Database.Default.Sequence.Value;
                                RequestContext.Set(ContextKeys.TraceKey, traceKey);
                            }
                            else
                                traceKey = (long) RequestContext.Get(ContextKeys.TraceKey);

                            int traceOrder = RequestContext.Get(ContextKeys.TraceOrder) != null ? (int) RequestContext.Get(ContextKeys.TraceOrder) + 1 : 0;
                            RequestContext.Set(ContextKeys.TraceOrder, traceOrder);

                            Task.Run(() => EventLog.Save(context.InterfaceMethod, Phenix.Core.Reflection.Utilities.JsonSerialize(context.Arguments), traceKey, traceOrder));
                            try
                            {
                                return context.Invoke();
                            }
                            catch (Exception ex)
                            {
                                Task.Run(() => EventLog.Save(context.InterfaceMethod, Phenix.Core.Reflection.Utilities.JsonSerialize(context.Arguments), traceKey, traceOrder, ex));
                                throw;
                            }
                        }

                        return context.Invoke();
                    })
                    .Build();
                AsyncHelper.RunSync(() => value.Connect());
                return value;
            });
        }

        #region 扩展

        /// <summary>
        /// 获取遵循FIFO原则的SimpleMessageStream提供者
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <exception cref="ArgumentNullException">clusterClient不允许为空</exception>
        /// <returns>流提供者</returns>
        public static IStreamProvider GetSimpleMessageStreamProvider(this IClusterClient clusterClient)
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return clusterClient.GetStreamProvider(ContextKeys.SimpleMessageStreamProviderName);
        }

        #endregion

        #endregion
    }
}
