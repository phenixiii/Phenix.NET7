using System;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.Messaging;
using Phenix.Core.Data;
using Phenix.Core.SyncCollections;

namespace Phenix.Actor
{
    /// <summary>
    /// Orleans服务集群客户端
    /// </summary>
    public static class ClusterClient
    {
        #region 属性

        private static readonly SynchronizedDictionary<string, IClusterClient> _cache =
            new SynchronizedDictionary<string, IClusterClient>(StringComparer.Ordinal);

        private static IClusterClient _default;

        /// <summary>
        /// 缺省Orleans服务集群客户端
        /// </summary>
        public static IClusterClient Default
        {
            get { return _default ?? (_default = Fetch()); }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取Orleans服务集群客户端
        /// Orleans集群的唯一ID = Database.Default.DataSourceKey
        /// Orleans服务的唯一ID = Database.Default.DataSourceKey
        /// Orleans数据库连接串 = Database.Default.ConnectionString
        /// </summary>
        /// <returns>Orleans服务集群客户端</returns>
        public static IClusterClient Fetch()
        {
            return Fetch(Database.Default.DataSourceKey, Database.Default.DataSourceKey, Database.Default.ConnectionString);
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
            return _cache.GetValue(String.Format("{0}*{1}", clusterId, serviceId), () =>
            {
                IClusterClient value = new ClientBuilder()
                    .ConfigureLogging(logging => logging.AddConsole())
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
#if MySQL
                        options.Invariant = "MySql.Data.MySqlClient";
#endif
#if ORA
                        options.Invariant = "Oracle.DataAccess.Client";
#endif
                    })
                    .ConfigureApplicationParts(parts =>
                    {
                        parts.AddPluginPart();
                    })
                    .AddSimpleMessageStreamProvider(StreamProvider.Name)
                    .Build();
                value.Connect().Wait();
                return value;
            });
        }

        #endregion
    }
}
