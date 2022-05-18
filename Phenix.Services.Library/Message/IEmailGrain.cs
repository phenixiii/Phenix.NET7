using System.Threading.Tasks;
using Orleans;

namespace Phenix.Services.Library.Message
{
    /// <summary>
    /// 邮箱Grain接口
    /// key: Name
    /// </summary>
    public interface IEmailGrain : IGrainWithStringKey
    {
        #region 方法

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="receiver">接收用户</param>
        /// <param name="receiverAddress">接收地址</param>
        /// <param name="subject">主题</param>
        /// <param name="urgent">是否紧急</param>
        /// <param name="htmlBody">HTML内容</param>
        Task Send(string receiver, string receiverAddress, string subject, bool urgent, string htmlBody);

        #endregion
    }
}
