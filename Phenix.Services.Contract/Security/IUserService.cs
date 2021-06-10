using Phenix.Services.Business.Security;

namespace Phenix.Services.Contract.Security
{
    /// <summary>
    /// 用户资料服务接口
    /// </summary>
    public interface IUserService
    {
        #region 方法

        /// <summary>
        /// 注册成功
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="initialPassword">初始口令</param>
        /// <returns>返回消息</returns>
        string OnRegistered(User user, string initialPassword);

        /// <summary>
        /// 登记处理
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="dynamicPassword">动态口令</param>
        /// <returns>返回消息</returns>
        string OnCheckIn(User user, string dynamicPassword);

        /// <summary>
        /// 登录完成
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="tag">捎带数据(已解密, 默认是客户端当前时间)</param>
        void OnLogon(User user, string tag);

        #endregion
    }
}