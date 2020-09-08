using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Actor;
using Phenix.Core.Message;

namespace Phenix.Services.Plugin.Api.Message
{
    /// <summary>
    /// 用户消息控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiMessageUserMessagePath)]
    [ApiController]
    public sealed class UserMessageController : Phenix.Core.Net.Api.ControllerBase
    {
        // phAjax.receiveMessage()
        /// <summary>
        /// 接收（PULL）
        /// </summary>
        /// <returns>结果集(消息ID-消息内容)</returns>
        [Authorize]
        [HttpGet]
        public IDictionary<long, string> Receive()
        {
            return UserMessage.Receive(User.Identity.Name);
        }

        // phAjax.sendMessage()
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="receiver">接收用户</param>
        [Authorize]
        [HttpPut]
        public async Task Send(long id, string receiver)
        {
            UserMessage.Send(new UserMessageInfo(id, User.Identity.Name, receiver, await Request.ReadBodyAsStringAsync()));
        }

        // phAjax.sendMessage()
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="receiver">接收用户</param>
        [Authorize]
        [HttpPost]
        public async Task Send(string receiver)
        {
            string content = await Request.ReadBodyAsStringAsync();
            //测试分组消息的推送
            //await ClusterClient.Default.GetStreamProvider().GetStream<string>(ActorConfig.GroupMessageStreamId, receiver).OnNextAsync(content);
            UserMessage.Send(User.Identity.Name, receiver, content);
        }

        // phAjax.affirmReceivedMessage()
        /// <summary>
        /// 确认收到
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="burn">是否销毁</param>
        [Authorize]
        [HttpDelete]
        public void AffirmReceived(long id, bool burn)
        {
            UserMessage.AffirmReceived(id, burn);
        }
    }
}
