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
        /// 登记(获取动态口令)/注册(静态口令即登录名)
        /// </summary>
        /// <param name="phone">手机(注册时可空)</param>
        /// <param name="eMail">邮箱(注册时可空)</param>
        /// <param name="regAlias">昵称(注册时可空)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <returns>返回信息</returns>
        Task<string> CheckIn(string phone, string eMail, string regAlias, string requestAddress);

        /// <summary>
        /// 核对登录有效性
        /// </summary>
        /// <param name="timestamp">时间戳(9位长随机数+ISO格式当前时间)</param>
        /// <param name="signature">签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)</param>
        /// <param name="tag">捎带数据(未解密, 默认是客户端当前时间)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="requestSession">服务请求会话签名</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令错误或口令失效时会抛出UserNotFoundException/UserLockedException/UserVerifyException异常而不是返回false</param>
        /// <returns>是否正确</returns>
        Task<bool> IsValidLogon(string timestamp, string signature, string tag, string requestAddress, string requestSession, bool throwIfNotConform);

        /// <summary>
        /// 核对有效性
        /// </summary>
        /// <param name="timestamp">时间戳(9位长随机数+ISO格式当前时间)</param>
        /// <param name="signature">签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="requestSession">服务请求会话签名</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令错误或禁止多终端登录时会抛出UserNotFoundException/UserLockedException/UserVerifyException/UserMultiAddressRequestException异常而不是返回false</param>
        /// <returns>是否正确</returns>
        Task<bool> IsValid(string timestamp, string signature, string requestAddress, string requestSession, bool throwIfNotConform);

        /// <summary>
        /// 登出
        /// </summary>
        Task Logout();

        /// <summary>
        /// 重置登录口令(静态口令即登录名)
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> ResetPassword();

        /// <summary>
        /// 修改登录口令
        /// </summary>
        /// <param name="password">登录口令</param>
        /// <param name="newPassword">新登录口令</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令不规范会抛出UserNotFoundException/UserLockedException/UserVerifyException/UserPasswordComplexityException异常而不是返回false</param>
        /// <returns>是否成功</returns>
        Task<bool> ChangePassword(string password, string newPassword, string requestAddress, bool throwIfNotConform);

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
        /// 获取自己用户资料
        /// </summary>
        Task<User> FetchMyself();

        /// <summary>
        /// 获取公司用户资料
        /// </summary>
        Task<IList<User>> FetchCompanyUsers();

        /// <summary>
        /// 注册(静态口令即登录名)
        /// </summary>
        /// <param name="phone">手机(注册时可空)</param>
        /// <param name="eMail">邮箱(注册时可空)</param>
        /// <param name="regAlias">昵称(注册时可空)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="teamsId">所属团体ID</param>
        /// <param name="positionId">担任岗位ID</param>
        /// <returns>返回信息</returns>
        Task<string> Register(string phone, string eMail, string regAlias, string requestAddress, long teamsId, long positionId);
    }
}