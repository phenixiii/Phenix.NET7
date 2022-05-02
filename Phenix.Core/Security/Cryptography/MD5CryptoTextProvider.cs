using System;
using System.Security.Cryptography;
using System.Text;

namespace Phenix.Core.Security.Cryptography
{
    /// <summary>
    /// MD5加密字符串
    /// </summary>
    [Obsolete]
    public static class MD5CryptoTextProvider
    {
        /// <summary>
        /// 取Hash字符串
        /// </summary>
        /// <param name="sourceText">原文</param>
        /// <param name="toUpper">返回大写字符串</param>
        /// <returns>Hash字符串</returns>
        public static string ComputeHash(string sourceText, bool toUpper = true)
        {
            if (sourceText == null)
                return null;

            StringBuilder result = new StringBuilder();
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(sourceText));
                if (toUpper)
                    for (int i = 0; i < data.Length; i++)
                        result.Append(data[i].ToString("X2"));
                else
                    for (int i = 0; i < data.Length; i++)
                        result.Append(data[i].ToString("x2"));
            }

            return result.ToString();
        }
    }
}