using System;
using Phenix.Core.Security;

namespace Phenix.Services.Extend.Actor.Security
{
    /// <summary>
    /// 用户资料服务
    /// </summary>
    public sealed class UserService : IUserService
    {
        #region 方法

        /// <summary>
        /// 注册成功
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="initialPassword">初始口令</param>
        /// <returns>返回消息</returns>
        string IUserService.OnRegistered(User user, string initialPassword)
        {
            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为通过第三方渠道（邮箱或短信）将初始口令推送给到用户并返回提示信息
             */
            Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0}({1}) 的初始口令是'{2}'", user.RegAlias, user.Name, initialPassword));
            return String.CompareOrdinal(user.Name, initialPassword) == 0
                ? String.Format("您的初始口令和登录名相同，首次登录时请更改为符合条件的口令(长度需大于等于{0}个字符且至少包含数字、大小写字母、特殊字符之{1}种)", User.PasswordLengthMinimum, User.PasswordComplexityMinimum)
                : String.Format("您的初始口令存放于 {0} 目录下的日志文件里.", Phenix.Core.Log.EventLog.LocalDirectory);
        }

        /// <summary>
        /// 登记处理
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="dynamicPassword">动态口令</param>
        /// <returns>返回消息</returns>
        string IUserService.OnCheckIn(User user, string dynamicPassword)
        {
            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为通过第三方渠道（邮箱或短信）将动态口令推送给到用户并返回提示信息
             */
            Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0}({1}) 的动态口令是'{2}'(有效期 {3} 分钟)", user.RegAlias, user.Name, dynamicPassword, User.DynamicPasswordValidityMinutes));
            return String.Format("您的动态口令存放于 {0} 目录下的日志文件里, 有效期 {1} 分钟.", Phenix.Core.Log.EventLog.LocalDirectory, User.DynamicPasswordValidityMinutes);
        }

        /// <summary>
        /// 登录完成
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="tag">捎带数据(已解密, 默认是客户端当前时间)</param>
        void IUserService.OnLogon(User user, string tag)
        {
            /*
             * 本函数被执行到，说明用户已通过身份验证
             * 可利用客户端传过来的 tag 扩展出系统自己的用户登录功能
             */
        }

        #endregion
    }
}