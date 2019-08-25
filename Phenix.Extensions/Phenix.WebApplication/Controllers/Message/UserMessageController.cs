using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Message;
using Phenix.Core.Net;
using Phenix.Core.Reflection;

namespace Phenix.WebApplication.Controllers.Message
{
    /// <summary>
    /// 用户消息控制器
    /// </summary>
    [EnableCors]
    [Route(NetConfig.MessageUserMessagePath)]
    [ApiController]
    public sealed class UserMessageController : Phenix.Core.Net.ControllerBase
    {
        // POST: /api/message/user-message
        /// <summary>
        /// 发送
        /// </summary>
        [Authorize]
        [HttpPost]
        public void Send()
        {
            UserMessage.Send(Utilities.JsonDeserialize<UserMessageInfo>(Request.ReadBodyAsString()));
        }

        // GET: /api/message/user-message?receiver=林冲
        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="receiver">接收用户</param>
        /// <returns>消息ID,消息内容</returns>
        [Authorize]
        [HttpGet]
        public IList<UserMessageInfo> Receive(string receiver)
        {
            return UserMessage.Receive(receiver);
        }

        // POST: /api/message/user-message?id=66666666&burn=true
        /// <summary>
        /// 确认收到
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="burn">是否销毁</param>
        [Authorize]
        [HttpPatch]
        public void AffirmReceived(long id, bool burn)
        {
            UserMessage.AffirmReceived(id, burn);
        }
    }
}
