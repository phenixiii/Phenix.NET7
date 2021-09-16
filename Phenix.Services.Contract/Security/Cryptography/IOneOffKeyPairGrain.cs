using System.Threading.Tasks;
using Orleans;

namespace Phenix.Services.Contract.Security.Cryptography
{
    /// <summary>
    /// 一次性公钥私钥对Grain接口
    /// key: DiscardIntervalSeconds
    /// keyExtension: Name
    /// </summary>
    public interface IOneOffKeyPairGrain : IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 获取公钥
        /// </summary>
        /// <returns>公钥</returns>
        Task<string> GetPublicKey();

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="cipherText">密文(Base64字符串)</param>
        /// <param name="fOAEP">如果为 true，则使用OAEP填充（仅在运行Microsoft Windows XP或更高版本的计算机上可用）执行直接的RSA加密；否则，如果为false，则使用PKCS#1 1.5版填充</param>
        /// <returns>原文</returns>
        Task<string> Decrypt(string cipherText, bool fOAEP);
    }
}
