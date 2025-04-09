using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FenjiuCapstone.Tools
{
    public class Password
    {
        /// <summary>
        /// 加密password
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string plainText, string base64Key)
        {
            using (var aes = Aes.Create())
            {
                // 解码 Base64 得到 32 字节 key
                var keyBytes = Convert.FromBase64String(base64Key);

                if (keyBytes.Length != 32)
                    throw new ArgumentException("Key must be 256 bits (32 bytes)");

                aes.Key = keyBytes;
                aes.IV = new byte[16]; // 默认IV，全0（不推荐用生产，建议换成随机IV+明文附加）

                var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
        }


        /// <summary>
        /// 解密 password
        /// </summary>
        /// <param name="encryptedText">Base64 编码的密文</param>
        /// <param name="base64Key">Base64 编码的 256 位密钥</param>
        /// <returns></returns>
        public static string Decrypt(string encryptedText, string base64Key)
        {
            using (var aes = Aes.Create())
            {
                // 解码 Base64 密钥
                var keyBytes = Convert.FromBase64String(base64Key);

                if (keyBytes.Length != 32)
                    throw new ArgumentException("Key must be 256 bits (32 bytes)");

                aes.Key = keyBytes;
                aes.IV = new byte[16]; // 和加密时保持一致（默认全 0）

                var decryptor = aes.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        /// <summary>
        /// 生成一个 AES-256 加密所需的 32 字节密钥，并返回 Base64 编码格式
        /// </summary>
        /// <returns></returns>
        public static string GenerateAes256Key()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[32]; // 256-bit
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes); // 返回 Base64 编码字符串
            }
        }

    }
}