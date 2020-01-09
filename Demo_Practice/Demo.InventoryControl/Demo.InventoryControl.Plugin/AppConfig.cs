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
        public static string FormatLocation(string locationArea, string locationAlley, string locationOrdinal)
        {
            return String.Format("{0}-{1}-{2}", locationArea, locationAlley, locationOrdinal);
        }

        /// <summary>
        /// 提取库区
        /// </summary>
        public static string ExtractArea(string location)
        {
            return location.Split('-')[0];
        }

        /// <summary>
        /// 提取巷道
        /// </summary>
        public static string ExtractAlley(string location)
        {
            return location.Split('-')[1];
        }

        /// <summary>
        /// 提取序号
        /// </summary>
        public static string ExtractOrdinal(string location)
        {
            return location.Split('-')[2];
        }
    }
}