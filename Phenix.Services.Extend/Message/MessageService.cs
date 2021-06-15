using System;
using System.Threading.Tasks;
using Phenix.Core.Security;
using Phenix.Services.Contract.Message;

namespace Phenix.Services.Extend.Message
{
    /// <summary>
    /// 用户消息服务
    /// </summary>
    public sealed class MessageService : IMessageService
    {
        #region 方法

        Task IMessageService.OnConnected(Identity identity, string connectionId)
        {
            /*
             * 本函数被执行到，说明客户端已连接到 SignalR hub 中间件
             * 当前登录用户身份为 identity
             */
            return Task.CompletedTask;
        }

        Task IMessageService.OnDisconnected(Identity identity, string connectionId, Exception exception)
        {
            /*
             * 本函数被执行到，说明客户端已断开 SignalR hub 中间件
             * 当前登录用户身份为 identity
             */
            return Task.CompletedTask;
        }

        #endregion
    }
}