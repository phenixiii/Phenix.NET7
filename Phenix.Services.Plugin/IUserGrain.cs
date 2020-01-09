using System.Threading.Tasks;
using Orleans;
using Phenix.Actor;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料Grain接口
    /// </summary>
    public interface IUserGrain : IRootEntityGrain, IGrainWithIntegerKey
    {
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
        /// <returns>原文</returns>
        Task<string> Decrypt(string cipherText);

        /// <summary>
        /// 核对登录有效性
        /// </summary>
        /// <param name="timestamp">时间戳(9位长随机数+ISO格式当前时间)</param>
        /// <param name="signature">签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令错误或口令失效时会抛出UserNotFoundException/UserLockedException/UserVerifyException异常而不是返回false</param>
        /// <returns>是否正确</returns>
        Task<bool> IsValidLogon(string timestamp, string signature, string requestAddress, bool throwIfNotConform);

        /// <summary>
        /// 核对有效性
        /// </summary>
        /// <param name="timestamp">时间戳(9位长随机数+ISO格式当前时间)</param>
        /// <param name="signature">签名(二次MD5登录口令/动态口令AES加密时间戳的Base64字符串)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令错误或禁止多终端登录时会抛出UserNotFoundException/UserLockedException/UserVerifyException/UserMultiAddressRequestException异常而不是返回false</param>
        /// <returns>是否正确</returns>
        Task<bool> IsValid(string timestamp, string signature, string requestAddress, bool throwIfNotConform);

        /// <summary>
        /// 修改登录口令
        /// </summary>
        /// <param name="newPassword">新登录口令</param>
        /// <param name="throwIfNotConform">如果为 true, 账号无效或口令不规范会抛出UserNotFoundException/UserLockedException/UserVerifyException/UserPasswordComplexityException异常而不是返回false</param>
        /// <returns>是否成功</returns>
        Task<bool> ChangePassword(string newPassword, bool throwIfNotConform);

        /// <summary>
        /// 申请动态口令
        /// </summary>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <param name="throwIfNotConform">如果为 true, 核对有效性不符会抛出UserNotFoundException/UserLockedException异常而不是返回false</param>
        /// <returns>动态口令(6位数字一般作为验证码用短信发送给到用户)</returns>
        Task<string> ApplyDynamicPassword(string requestAddress, bool throwIfNotConform);
    }
}