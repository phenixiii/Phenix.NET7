using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;
using Phenix.Services.Business.Security;

namespace Phenix.Services.Contract.Security
{
    /// <summary>
    /// 用户资料Grain接口
    /// key: CompanyName
    /// keyExtension: UserName
    /// </summary>
    public interface IUserGrain : IEntityGrain<User>, IGrainWithStringKey
    {
        /// <summary>
        /// 注册(初始口令即登录名)
        /// </summary>
        /// <param name="phone">手机</param>
        /// <param name="eMail">邮箱</param>
        /// <param name="regAlias">昵称</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="teamsId">所属团体ID(注册公司管理员时为null)</param>
        /// <param name="positionId">担任岗位ID(注册公司管理员时为null)</param>
        /// <returns>返回信息</returns>
        Task<string> Register(string phone, string eMail, string regAlias, string requestAddress, long? teamsId = null, long? positionId = null);


        /// <summary>
        /// 登记(获取动态口令)
        /// </summary>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <returns>返回信息</returns>
        Task<string> CheckIn(string requestAddress);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="tag">捎带数据(默认是客户端时间也可以是修改的新密码)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        Task Logon(string signature, string tag, string requestAddress);

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        Task Verify(string signature, string requestAddress);

        /// <summary>
        /// 登出
        /// </summary>
        Task Logout();

        /// <summary>
        /// 重置登录口令(初始口令即登录名)
        /// </summary>
        Task ResetPassword();

        /// <summary>
        /// 加密
        /// Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="sourceText">原文</param>
        /// <returns>密文(Base64字符串)</returns>
        Task<string> Encrypt(string sourceText);

        /// <summary>
        /// 解密
        ///  Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="cipherText">密文(Base64字符串)</param>
        Task<string> Decrypt(string cipherText);

        /// <summary>
        /// 获取用户资料
        /// </summary>
        Task<User> FetchMyself();

        /// <summary>
        /// 获取公司用户资料
        /// </summary>
        Task<IList<User>> FetchCompanyUsers();
    }
}