using System;
using Orleans;
using Orleans.Streams;

namespace Phenix.Actor
{
    /// <summary>
    /// StreamProvider扩展
    /// </summary>
    public static class StreamProviderProxy
    {
        #region 属性

        #region 配置项

        /// <summary>
        /// 遵循FIFO原则的SimpleMessageStream提供者名称
        /// </summary>
        public static string SimpleMessageStreamProviderName
        {
            get { return "SMSProvider"; }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 获取遵循FIFO原则的SimpleMessageStream提供者
        /// </summary>
        /// <param name="clusterClient">Orleans服务集群客户端</param>
        /// <returns>流提供者</returns>
        public static IStreamProvider GetSimpleMessageStreamProvider(this IClusterClient clusterClient)
        {
            if (clusterClient == null)
                throw new ArgumentNullException(nameof(clusterClient));

            return clusterClient.GetStreamProvider(SimpleMessageStreamProviderName);
        }

        #endregion
    }
}