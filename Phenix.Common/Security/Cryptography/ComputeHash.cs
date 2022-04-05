using System.Security.Cryptography;
using System.Text;

namespace Phenix.Common.Security.Cryptography
{
    /// <summary>
    /// 计算哈希(SHA512)
    /// </summary>
    public static class ComputeHash
    {
        /// <summary>
        /// 取Hash字符串
        /// </summary>
        /// <param name="sourceText">原文</param>
        /// <param name="toUpper">返回大写字符串</param>
        /// <returns>Hash字符串</returns>
        public static string Do(string sourceText, bool toUpper = true)
        {
            if (sourceText == null)
                return null;

            StringBuilder result = new StringBuilder();
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] data = sha512.ComputeHash(Encoding.UTF8.GetBytes(sourceText));
                if (toUpper)
                    foreach (byte b in data)
                        result.Append(b.ToString("X2"));
                else
                    foreach (byte b in data)
                        result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }
    }
}