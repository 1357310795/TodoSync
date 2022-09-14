using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.CLI
{
    public class AesHelper
    {
        #region 方法

        /// <summary>
        /// 加密
        /// IV等于Key且Key和IV将被转换为MD5值
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="sourceText">原文</param>
        /// <returns>密文(Base64字符串)</returns>
        public static string Encrypt(string key, string sourceText)
        {
            return Encrypt(key, key, sourceText);
        }

        /// <summary>
        /// 加密
        /// Key和IV将被转换为MD5值
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="IV">初始化向量</param>
        /// <param name="sourceText">原文</param>
        /// <returns>密文(Base64字符串)</returns>
        public static string Encrypt(string key, string IV, string sourceText)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (IV == null)
                throw new ArgumentNullException(nameof(IV));
            if (sourceText == null)
                throw new ArgumentNullException(nameof(sourceText));

            return Convert.ToBase64String(Encrypt(EncryptMD5(key), EncryptMD5(IV), sourceText));
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="rgbKey">密钥</param>
        /// <param name="rgbIV">初始化向量</param>
        /// <param name="sourceText">原文</param>
        /// <returns>密文</returns>
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
                using (Aes aes = Aes.Create())
                using (ICryptoTransform transform = aes.CreateEncryptor(rgbKey, rgbIV))
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
        /// 解密
        /// IV等于Key且Key和IV将被转换为MD5值
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="cipherText">密文(Base64字符串)</param>
        /// <returns>原文</returns>
        public static string Decrypt(string key, string cipherText)
        {
            return Decrypt(key, key, cipherText);
        }

        /// <summary>
        /// 解密
        /// Key和IV将被转换为MD5值
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="IV">初始化向量</param>
        /// <param name="cipherText">密文(Base64字符串)</param>
        /// <returns>原文</returns>
        public static string Decrypt(string key, string IV, string cipherText)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (IV == null)
                throw new ArgumentNullException(nameof(IV));
            if (cipherText == null)
                throw new ArgumentNullException(nameof(cipherText));

            return Decrypt(EncryptMD5(key), EncryptMD5(IV), Convert.FromBase64String(cipherText));
        }

        /// <summary>
        /// 解密
        /// IV等于Key
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="cipherBuffer">密文</param>
        /// <returns>原文</returns>
        public static string Decrypt(string key, byte[] cipherBuffer)
        {
            return Decrypt(key, key, cipherBuffer);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="IV">初始化向量</param>
        /// <param name="cipherBuffer">密文</param>
        /// <returns>原文</returns>
        public static string Decrypt(string key, string IV, byte[] cipherBuffer)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (IV == null)
                throw new ArgumentNullException(nameof(IV));
            if (cipherBuffer == null)
                throw new ArgumentNullException(nameof(cipherBuffer));

            return Decrypt(EncryptMD5(key), EncryptMD5(IV), cipherBuffer);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="rgbKey">密钥</param>
        /// <param name="rgbIV">初始化向量</param>
        /// <param name="cipherBuffer">密文</param>
        /// <returns>原文</returns>
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
        /// 解密
        /// IV等于Key
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="cipherStream">密文</param>
        /// <returns>原文</returns>
        public static string Decrypt(string key, Stream cipherStream)
        {
            return Decrypt(key, key, cipherStream);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="IV">初始化向量</param>
        /// <param name="cipherStream">密文</param>
        /// <returns>原文</returns>
        public static string Decrypt(string key, string IV, Stream cipherStream)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (IV == null)
                throw new ArgumentNullException(nameof(IV));
            if (cipherStream == null)
                throw new ArgumentNullException(nameof(cipherStream));

            return Decrypt(EncryptMD5(key), EncryptMD5(IV), cipherStream);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="rgbKey">密钥</param>
        /// <param name="rgbIV">初始化向量</param>
        /// <param name="cipherStream">密文</param>
        /// <returns>原文</returns>
        public static string Decrypt(byte[] rgbKey, byte[] rgbIV, Stream cipherStream)
        {
            if (rgbKey == null)
                throw new ArgumentNullException(nameof(rgbKey));
            if (rgbIV == null)
                throw new ArgumentNullException(nameof(rgbIV));
            if (cipherStream == null)
                throw new ArgumentNullException(nameof(cipherStream));

            using (Aes aes = Aes.Create())
            using (ICryptoTransform transform = aes.CreateDecryptor(rgbKey, rgbIV))
            using (CryptoStream cryptoStream = new CryptoStream(cipherStream, transform, CryptoStreamMode.Read))
            using (StreamReader streamReader = new StreamReader(cryptoStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        #endregion

        public static byte[] EncryptMD5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            return s;
        }
    }
}
