using Orleans;
using Orleans.Streams;

namespace Phenix.Actor
{
    /// <summary>
    /// StreamProvider扩展
    /// </summary>
    public static class StreamProviderExtension
    {
        #region 属性

        #region 配置项

        /// <summary>
        /// 名称
        /// </summary>
        public static string StreamProviderName
        {
            get { return "SMSProvider"; }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 获取Orleans流提供者
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <returns>流提供者</returns>
        public static IStreamProvider GetStreamProvider(this IClusterClient clusterClient)
        {
            return clusterClient.GetStreamProvider(StreamProviderName);
        }

        #endregion
    }
}