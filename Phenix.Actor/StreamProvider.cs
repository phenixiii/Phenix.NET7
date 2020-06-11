using System;
using Orleans.Streams;
using Phenix.Core.SyncCollections;

namespace Phenix.Actor
{
    /// <summary>
    /// Orleans流提供者
    /// </summary>
    public static class StreamProvider
    {
        #region 属性

        #region 配置项

        /// <summary>
        /// 名称
        /// </summary>
        public static string Name
        {
            get { return typeof(StreamProvider).Assembly.FullName; }
        }

        #endregion

        private static readonly SynchronizedDictionary<string, IStreamProvider> _cache = new SynchronizedDictionary<string, IStreamProvider>(StringComparer.Ordinal);

        private static IStreamProvider _default;

        /// <summary>
        /// 缺省Orleans流提供者
        /// </summary>
        public static IStreamProvider Default
        {
            get { return _default ?? (_default = Fetch()); }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取Orleans流提供者
        /// </summary>
        /// <returns>Orleans流提供者</returns>
        public static IStreamProvider Fetch()
        {
            return _cache.GetValue(Name, () => ClusterClient.Fetch().GetStreamProvider(Name));
        }

        /// <summary>
        /// 获取Orleans流提供者
        /// </summary>
        /// <param name="clusterId">Orleans集群的唯一ID</param>
        /// <param name="serviceId">Orleans服务的唯一ID</param>
        /// <param name="connectionString">Orleans数据库连接串</param>
        /// <returns>流提供者</returns>
        public static IStreamProvider Fetch(string clusterId, string serviceId, string connectionString)
        {
            return _cache.GetValue(String.Format("{0}*{1}", clusterId, serviceId), () => ClusterClient.Fetch(clusterId, serviceId, connectionString).GetStreamProvider(Name));
        }

        #endregion
    }
}
