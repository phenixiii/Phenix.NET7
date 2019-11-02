using System;
using Phenix.Core.Security.Cryptography;

namespace Phenix.Tools.CreateKeyPair
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** RSA公钥密钥对生成工具 ****");
            Console.WriteLine();

            KeyPair keyPair = RSACryptoTextProvider.CreateKeyPair();
            Console.WriteLine("公钥：{0}", keyPair.PublicKey);
            Console.WriteLine();
            Console.WriteLine("私钥：{0}", keyPair.PrivateKey);
            Console.WriteLine();

            Console.Write("请输入测试用字符串：");
            string sourceText = Console.ReadLine();
            string cipherText = RSACryptoTextProvider.Encrypt(keyPair.PublicKey, sourceText);
            Console.WriteLine("密文：{0}", cipherText);
            string decryptText = RSACryptoTextProvider.Decrypt(keyPair.PrivateKey, cipherText);
            Console.WriteLine("解密：{0} {1}", decryptText, decryptText == sourceText ? "ok" : "error");
            Console.WriteLine();

            Console.Write("请按回车键结束程序");
            Console.ReadLine();
            Console.WriteLine();
        }
    }
}