using System;
using System.Security.Cryptography;
using System.Text;

namespace Phenix.Core.Security.Cryptography
{
    /// <summary>
    /// RSA加解密字符串
    /// </summary>
    public static class RSACryptoTextProvider
    {
        #region 方法

        /// <summary>
        /// 生成公钥私钥对
        /// </summary>
        public static KeyPair CreateKeyPair()
        {
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                return CreateKeyPair(cryptoServiceProvider);
            }
        }
        
        /// <summary>
        /// 生成公钥私钥对
        /// </summary>
        /// <param name="cryptoServiceProvider">加密服务</param>
        public static KeyPair CreateKeyPair(RSACryptoServiceProvider cryptoServiceProvider)
        {
            if (cryptoServiceProvider == null)
                throw new ArgumentNullException(nameof(cryptoServiceProvider));

            return new KeyPair(cryptoServiceProvider.ToXmlString(false), cryptoServiceProvider.ToXmlString(true));
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="sourceText">原文</param>
        /// <param name="fOAEP">如果为 true，则使用OAEP填充（仅在运行Microsoft Windows XP或更高版本的计算机上可用）执行直接的RSA加密；否则，如果为false，则使用PKCS#1 1.5版填充</param>
        /// <returns>密文(Base64字符串)</returns>
        public static string Encrypt(string publicKey, string sourceText, bool fOAEP = true)
        {
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                return Encrypt(cryptoServiceProvider, publicKey, sourceText, fOAEP);
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="cryptoServiceProvider">加密服务</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="sourceText">原文</param>
        /// <param name="fOAEP">如果为 true，则使用OAEP填充（仅在运行Microsoft Windows XP或更高版本的计算机上可用）执行直接的RSA加密；否则，如果为false，则使用PKCS#1 1.5版填充</param>
        /// <returns>密文(Base64字符串)</returns>
        public static string Encrypt(RSACryptoServiceProvider cryptoServiceProvider, string publicKey, string sourceText, bool fOAEP = true)
        {
            if (cryptoServiceProvider == null)
                throw new ArgumentNullException(nameof(cryptoServiceProvider));
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
            if (sourceText == null)
                throw new ArgumentNullException(nameof(sourceText));

            cryptoServiceProvider.FromXmlString(publicKey);
            byte[] result = cryptoServiceProvider.Encrypt(Encoding.UTF8.GetBytes(sourceText), fOAEP);
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="cipherText">密文(Base64字符串)</param>
        /// <param name="fOAEP">如果为 true，则使用OAEP填充（仅在运行Microsoft Windows XP或更高版本的计算机上可用）执行直接的RSA加密；否则，如果为false，则使用PKCS#1 1.5版填充</param>
        /// <returns>原文</returns>
        public static string Decrypt(string privateKey, string cipherText, bool fOAEP = true)
        {
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                return Decrypt(cryptoServiceProvider, privateKey, cipherText, fOAEP);
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="cryptoServiceProvider">加密服务</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="cipherText">密文(Base64字符串)</param>
        /// <param name="fOAEP">如果为 true，则使用OAEP填充（仅在运行Microsoft Windows XP或更高版本的计算机上可用）执行直接的RSA加密；否则，如果为false，则使用PKCS#1 1.5版填充</param>
        /// <returns>原文</returns>
        public static string Decrypt(RSACryptoServiceProvider cryptoServiceProvider, string privateKey, string cipherText, bool fOAEP = true)
        {
            if (cryptoServiceProvider == null)
                throw new ArgumentNullException(nameof(cryptoServiceProvider));
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));
            if (cipherText == null)
                throw new ArgumentNullException(nameof(cipherText));

            cryptoServiceProvider.FromXmlString(privateKey);
            byte[] result = cryptoServiceProvider.Decrypt(Convert.FromBase64String(cipherText), fOAEP);
            return Encoding.UTF8.GetString(result);
        }

        #endregion
    }
}
