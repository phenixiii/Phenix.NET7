using System;
using System.Net;
using System.Net.Sockets;

namespace Phenix.Core.Net
{
    /// <summary>
    /// 网络配置信息
    /// </summary>
    public static class NetConfig
    {
        #region 属性

        private static string _localAddress;

        /// <summary>
        /// 本机IP地址
        /// </summary>
        public static string LocalAddress
        {
            get
            {
                if (String.IsNullOrEmpty(_localAddress))
                {
                    string result = null;
                    foreach (IPAddress address in Dns.GetHostAddresses(Dns.GetHostName()))
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            result = address.ToString();
                            break;
                        }

                    if (String.IsNullOrEmpty(result))
                        result = "127.0.0.1";
                    _localAddress = result;
                }

                return _localAddress;
            }
        }

        #endregion
    }
}