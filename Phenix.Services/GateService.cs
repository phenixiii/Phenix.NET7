using System;
using Phenix.Core.Net.Api.Security;
using Phenix.Core.Security;

namespace Phenix.Services
{
    /// <summary>
    /// 系统入口服务
    /// </summary>
    public class GateService : IGateService
    {
        /// <summary>
        /// 当用户获取动态口令时被CheckIn触发(可推送动态口令给到用户) 
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="dynamicPassword">动态口令(6位数字一般作为验证码用短信发送给到用户)</param>
        /// <param name="validityMinutes">动态口令有效周期(分钟>=3)</param>
        public void PushDynamicPassword(IUser user, string dynamicPassword, int validityMinutes)
        {
            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为通过第三方渠道（邮箱或短信）推送给到用户
             */
            Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0} 的动态口令是'{1}'，有效期 {2} 分钟", user.Name, dynamicPassword, validityMinutes));
        }

        /// <summary>
        /// 当新用户注册时被CheckIn触发(可推送初始口令/动态口令给到用户) 
        /// </summary>
        /// <param name="newUser">用户资料</param>
        /// <param name="initialPassword">初始口令(一般通过邮箱发送给到用户)</param>
        /// <param name="dynamicPassword">动态口令(6位数字一般作为验证码用短信发送给到用户)</param>
        /// <param name="validityMinutes">动态口令有效周期(分钟>=3)</param>
        public void PushPassword(IUser newUser, string initialPassword, string dynamicPassword, int validityMinutes)
        {
            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为通过第三方渠道（邮箱或短信）推送给到用户
             */
            Phenix.Core.Log.EventLog.SaveLocal(String.Format("{0} 的初始口令是'{1}'，动态口令是'{2}'", newUser.Name, initialPassword, dynamicPassword));
        }

        /// <summary>
        /// 当完成CheckIn时触发
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="error">非null时说明有异常发生</param>
        /// <returns>提示信息</returns>
        public string AfterCheckIn(IUser user, Exception error)
        {
            /*
             * 以下代码直接抛出异常给到客户端
             * 生产环境下，可根据需要更换为其他处理异常的方式
             */
            if (error != null)
                throw error;

            /*
             * 以下代码供你自己测试用
             * 生产环境下，请替换为提示用户留意查看邮箱或短信以收取动态口令
             */
            return String.Format("{0} 的动态口令存放于 {1} 目录下的日志文件里", user.Name, Phenix.Core.Log.EventLog.LocalDirectory);
        }

        /// <summary>
        /// 当完成Logon时触发
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <param name="tag">捎带数据(默认是客户端当前时间)</param>
        public void AfterLogon(IUser user, string tag)
        {
            /*
             * 本函数被执行到，说明当前用户 user 已经登录成功
             * 可利用客户端传过来的 tag 扩展出系统自己的用户登录功能
             */
        }
    }
}
