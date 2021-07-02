using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Phenix.Services.Contract.Message
{
    /// <summary>
    /// 用户消息Grain接口
    /// key：CompanyName'\u0004'UserName
    /// </summary>
    public interface IUserMessageGrain : IGrainWithStringKey
    {
        #region 方法

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="receiver">接收用户</param>
        /// <param name="content">消息内容</param>
        Task Send(string receiver, string content);

        /// <summary>
        /// 接收（PULL）
        /// </summary>
        /// <returns>结果集(消息ID-消息内容)</returns>
        Task<IDictionary<long, string>> Receive();

        /// <summary>
        /// 确认收到
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="burn">是否销毁</param>
        Task AffirmReceived(long id, bool burn);

        #endregion
    }
}
