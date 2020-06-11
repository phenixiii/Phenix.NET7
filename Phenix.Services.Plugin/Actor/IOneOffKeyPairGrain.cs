using System.Threading.Tasks;
using Orleans;

namespace Phenix.Services.Plugin.Actor
{
    /// <summary>
    /// 一次性公钥私钥对Grain接口
    /// </summary>
    public interface IOneOffKeyPairGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// 获取公钥
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>公钥</returns>
        Task<string> GetPublicKey(string name);

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="cipherText">密文(Base64字符串)</param>
        /// <param name="fOAEP">如果为 true，则使用OAEP填充（仅在运行Microsoft Windows XP或更高版本的计算机上可用）执行直接的RSA加密；否则，如果为false，则使用PKCS#1 1.5版填充</param>
        /// <returns>原文</returns>
        Task<string> Decrypt(string name, string cipherText, bool fOAEP);
    }
}
