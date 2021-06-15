using System;
using System.Threading.Tasks;
using Phenix.Core.Security;

namespace Phenix.Services.Contract.Message
{
    /// <summary>
    /// 消息服务接口
    /// </summary>
    public interface IMessageService
    {
        #region 方法

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="identity">当前用户身份</param>
        /// <param name="connectionId">连接ID</param>
        Task OnConnected(Identity identity, string connectionId);

        /// <summary>
        /// 断开
        /// </summary>
        /// <param name="identity">当前用户身份</param>
        /// <param name="connectionId">连接ID</param>
        /// <param name="exception">Exception</param>
        Task OnDisconnected(Identity identity, string connectionId, Exception exception);

        #endregion
    }
}
