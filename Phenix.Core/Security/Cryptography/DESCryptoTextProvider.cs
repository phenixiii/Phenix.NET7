using System;
using System.IO;
using System.Security.Cryptography;

namespace Phenix.Core.Security.Cryptography
{
    /// <summary>
    /// DES�ӽ����ַ���
    /// </summary>
    public static class DESCryptoTextProvider
    {
        #region ����

        /// <summary>
        /// ����
        /// IV����Key��Key��IV����ת��ΪMD5ֵ
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="sourceText">ԭ��</param>
        /// <returns>����(Base64�ַ���)</returns>
        public static string Encrypt(string key, string sourceText)
        {
            return Encrypt(key, key, sourceText);
        }

        /// <summary>
        /// ����
        /// Key��IV����ת��ΪMD5ֵ
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="IV">��ʼ������</param>
        /// <param name="sourceText">ԭ��</param>
        /// <returns>����(Base64�ַ���)</returns>
        public static string Encrypt(string key, string IV, string sourceText)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (IV == null)
                throw new ArgumentNullException(nameof(IV));
            if (sourceText == null)
                throw new ArgumentNullException(nameof(sourceText));

            using (SHA512 sha512 = SHA512.Create())
            {
                return Convert.ToBase64String(Encrypt(sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key)), sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(IV)), sourceText));
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="rgbKey">��Կ</param>
        /// <param name="rgbIV">��ʼ������</param>
        /// <param name="sourceText">ԭ��</param>
        /// <returns>����</returns>
        public static byte[] Encrypt(byte[] rgbKey, byte[] rgbIV, string sourceText)
        {
            if (rgbKey == null)
                throw new ArgumentNullException(nameof(rgbKey));
            if (rgbIV == null)
                throw new ArgumentNullException(nameof(rgbIV));
            if (sourceText == null)
                throw new ArgumentNullException(nameof(sourceText));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DES des = DES.Create())
                using (ICryptoTransform transform = des.CreateEncryptor(rgbKey, rgbIV))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(sourceText);
                    streamWriter.Flush();
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// ����
        /// IV����Key��Key��IV����ת��ΪMD5ֵ
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="cipherText">����(Base64�ַ���)</param>
        /// <returns>ԭ��</returns>
        public static string Decrypt(string key, string cipherText)
        {
            return Decrypt(key, key, cipherText);
        }

        /// <summary>
        /// ����
        /// Key��IV����ת��ΪMD5ֵ
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="IV">��ʼ������</param>
        /// <param name="cipherText">����(Base64�ַ���)</param>
        /// <returns>ԭ��</returns>
        public static string Decrypt(string key, string IV, string cipherText)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (IV == null)
                throw new ArgumentNullException(nameof(IV));
            if (cipherText == null)
                throw new ArgumentNullException(nameof(cipherText));

            using (SHA512 sha512 = SHA512.Create())
            {
                return Decrypt(sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key)), sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(IV)), Convert.FromBase64String(cipherText));
            }
        }

        /// <summary>
        /// ����
        /// IV����Key
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="cipherBuffer">����</param>
        /// <returns>ԭ��</returns>
        public static string Decrypt(string key, byte[] cipherBuffer)
        {
            return Decrypt(key, key, cipherBuffer);
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="IV">��ʼ������</param>
        /// <param name="cipherBuffer">����</param>
        /// <returns>ԭ��</returns>
        public static string Decrypt(string key, string IV, byte[] cipherBuffer)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (IV == null)
                throw new ArgumentNullException(nameof(IV));
            if (cipherBuffer == null)
                throw new ArgumentNullException(nameof(cipherBuffer));

            using (SHA512 sha512 = SHA512.Create())
            {
                return Decrypt(sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key)), sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(IV)), cipherBuffer);
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="rgbKey">��Կ</param>
        /// <param name="rgbIV">��ʼ������</param>
        /// <param name="cipherBuffer">����</param>
        /// <returns>ԭ��</returns>
        public static string Decrypt(byte[] rgbKey, byte[] rgbIV, byte[] cipherBuffer)
        {
            if (rgbKey == null)
                throw new ArgumentNullException(nameof(rgbKey));
            if (rgbIV == null)
                throw new ArgumentNullException(nameof(rgbIV));
            if (cipherBuffer == null)
                throw new ArgumentNullException(nameof(cipherBuffer));

            using (MemoryStream stream = new MemoryStream(cipherBuffer))
            {
                return Decrypt(rgbKey, rgbIV, stream);
            }
        }

        /// <summary>
        /// ����
        /// IV����Key
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="cipherStream">����</param>
        /// <returns>ԭ��</returns>
        public static string Decrypt(string key, Stream cipherStream)
        {
            return Decrypt(key, key, cipherStream);
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="IV">��ʼ������</param>
        /// <param name="cipherStream">����</param>
        /// <returns>ԭ��</returns>
        public static string Decrypt(string key, string IV, Stream cipherStream)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (IV == null)
                throw new ArgumentNullException(nameof(IV));
            if (cipherStream == null)
                throw new ArgumentNullException(nameof(cipherStream));

            using (SHA512 sha512 = SHA512.Create())
            {
                return Decrypt(sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key)), sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(IV)), cipherStream);
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="rgbKey">��Կ</param>
        /// <param name="rgbIV">��ʼ������</param>
        /// <param name="cipherStream">����</param>
        /// <returns>ԭ��</returns>
        public static string Decrypt(byte[] rgbKey, byte[] rgbIV, Stream cipherStream)
        {
            if (rgbKey == null)
                throw new ArgumentNullException(nameof(rgbKey));
            if (rgbIV == null)
                throw new ArgumentNullException(nameof(rgbIV));
            if (cipherStream == null)
                throw new ArgumentNullException(nameof(cipherStream));

            using (DES des = DES.Create())
            using (ICryptoTransform transform = des.CreateDecryptor(rgbKey, rgbIV))
            using (CryptoStream cryptoStream = new CryptoStream(cipherStream, transform, CryptoStreamMode.Read))
            using (StreamReader streamReader = new StreamReader(cryptoStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        #endregion
    }
}