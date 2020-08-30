using System;

namespace Demo.InventoryControl.Plugin
{
    /// <summary>
    /// 应用系统配置信息
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// 格式化货架号
        /// </summary>
        /// <param name="locationArea">货架库区(按字符大小排序)</param>
        /// <param name="locationAlley">货架巷道(按字符大小排序)</param>
        /// <param name="locationOrdinal">货架序号(按字符大小排序)</param>
        public static string FormatLocation(string locationArea, string locationAlley, string locationOrdinal)
        {
            return String.Format("{0}-{1}-{2}", locationArea, locationAlley, locationOrdinal);
        }

        /// <summary>
        /// 提取库区
        /// </summary>
        /// <param name="location">货架号</param>
        public static string ExtractArea(string location)
        {
            return location.Split('-')[0];
        }

        /// <summary>
        /// 提取巷道
        /// </summary>
        /// <param name="location">货架号</param>
        public static string ExtractAlley(string location)
        {
            return location.Split('-')[1];
        }

        /// <summary>
        /// 提取序号
        /// </summary>
        /// <param name="location">货架号</param>
        public static string ExtractOrdinal(string location)
        {
            return location.Split('-')[2];
        }
    }
}