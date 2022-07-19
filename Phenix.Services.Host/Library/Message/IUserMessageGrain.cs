using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Phenix.Services.Host.Library.Message
{
    /// <summary>
    /// 用户消息Grain接口
    /// key: CompanyName
    /// keyExtension: UserName
    /// </summary>
    public interface IUserMessageGrain : IGrainWithStringKey
    {
        #region 方法

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="sender">发送用户</param>
        /// <param name="content">消息内容</param>
        Task Send(string sender, string content);

        /// <summary>
        /// 接收（PULL）
        /// </summary>
        /// <returns>结果集(消息ID-用户消息)</returns>
        Task<IDictionary<long, UserMessage>> Receive();

        /// <summary>
        /// 确认收到
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="burn">是否销毁</param>
        Task AffirmReceived(long id, bool burn);

        #endregion
    }
}
