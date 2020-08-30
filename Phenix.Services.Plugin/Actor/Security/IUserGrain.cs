using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;
using Phenix.Core.Security;

namespace Phenix.Services.Plugin.Actor.Security
{
    /// <summary>
    /// 用户资料Grain接口
    /// </summary>
    public interface IUserGrain : IEntityGrain<User>, IGrainWithStringKey, ITraceLogContext
    {
        /// <summary>
        /// 登记/注册
        /// </summary>
        /// <param name="phone">手机(注册用可为空)</param>
        /// <param name="eMail">邮箱(注册用可为空)</param>
        /// <param name="regAlias">注册昵称(注册用可为空)</param>
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
        /// 修改登录口令
        /// </summary>
        /// <param name="password">登录口令</param>
        /// <param name="newPassword">新登录口令</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令不规范会抛出UserNotFoundException/UserLockedException/UserVerifyException/UserPasswordComplexityException异常而不是返回false</param>
        /// <returns>是否成功</returns>
        Task<bool> ChangePassword(string password, string newPassword, string requestAddress, bool throwIfNotConform);

        /// <summary>
        /// 更新顶层团体
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>顶层团体主键</returns>
        Task<long> PatchRootTeams(string name);

        /// <summary>
        /// 获取公司用户资料
        /// </summary>
        /// <returns>公司用户资料</returns>
        Task<IList<User>> FetchCompanyUsers();

        /// <summary>
        /// 登记/注册公司用户
        /// </summary>
        /// <param name="name">登录名</param>
        /// <param name="phone">手机(注册用可为空)</param>
        /// <param name="eMail">邮箱(注册用可为空)</param>
        /// <param name="regAlias">注册昵称(注册用可为空)</param>
        /// <param name="teamsId">所属团体ID</param>
        /// <param name="positionId">担任岗位ID</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <returns>返回信息</returns>
        Task<string> RegisterCompanyUser(string name, string phone, string eMail, string regAlias, string requestAddress, long teamsId, long positionId);

        /// <summary>
        /// 登记/注册
        /// </summary>
        /// <param name="phone">手机(注册用可为空)</param>
        /// <param name="eMail">邮箱(注册用可为空)</param>
        /// <param name="regAlias">注册昵称(注册用可为空)</param>
        /// <param name="rootTeamsId">所属顶层团体ID</param>
        /// <param name="teamsId">所属团体ID</param>
        /// <param name="positionId">担任岗位ID</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <returns>返回信息</returns>
        Task<string> Register(string phone, string eMail, string regAlias, string requestAddress, long rootTeamsId, long teamsId, long positionId);

        /// <summary>
        /// 更新公司用户
        /// </summary>
        /// <param name="name">登录名</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task PatchCompanyUser(string name, IDictionary<string, object> propertyValues);

        /// <summary>
        /// 更新属性
        /// </summary>
        /// <param name="rootTeamsId">所属顶层团体ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task Patch(long? rootTeamsId, IDictionary<string, object> propertyValues);
    }
}