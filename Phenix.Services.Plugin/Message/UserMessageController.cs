using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Services.Contract.Message;

namespace Phenix.Services.Plugin.Message
{
    /// <summary>
    /// 用户消息控制器
    /// </summary>
    [Route(Phenix.Net.Api.StandardPaths.MessageUserMessagePath)]
    [ApiController]
    public sealed class UserMessageController : Phenix.Net.Api.ControllerBase
    {
        // phAjax.sendMessage()
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="receiver">接收用户</param>
        [Authorize]
        [HttpPost]
        public async Task Send(string receiver)
        {
            await ClusterClient.Default.GetGrain<IUserMessageGrain>(User.Identity.PrimaryKey).Send(receiver, await Request.ReadBodyAsStringAsync());
            //可替换为以下代码测试分组消息的推送
            //await ClusterClient.Default.GetSimpleMessageStreamProvider().GetStream<string>(StreamConfig.GroupStreamId, receiver).OnNextAsync(await Request.ReadBodyAsStringAsync());
        }

        // phAjax.receiveMessage()
        /// <summary>
        /// 接收（PULL）
        /// </summary>
        /// <returns>结果集(消息ID-消息内容)</returns>
        [Authorize]
        [HttpGet]
        public async Task<IDictionary<long, string>> Receive()
        {
            return await ClusterClient.Default.GetGrain<IUserMessageGrain>(User.Identity.PrimaryKey).Receive();
        }

        // phAjax.affirmReceivedMessage()
        /// <summary>
        /// 确认收到
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="burn">是否销毁</param>
        [Authorize]
        [HttpDelete]
        public async Task AffirmReceived(long id, bool burn)
        {
            await ClusterClient.Default.GetGrain<IUserMessageGrain>(User.Identity.PrimaryKey).AffirmReceived(id, burn);
        }
    }
}
