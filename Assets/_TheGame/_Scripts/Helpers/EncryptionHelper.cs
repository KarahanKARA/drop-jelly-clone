using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace _TheGame._Scripts.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly string Key = "c3a4ab7f70f45fe2ba2c806e01bbaf3d46f21ddabeeca692abc620a527a78292";

        private static byte[] GenerateRandomIv()
        {
            using var rng = new RNGCryptoServiceProvider();
            var iv = new byte[16];
            rng.GetBytes(iv);
            return iv;
        }

        public static string Encrypt(string plainText)
        {
            using var aesAlg = Aes.Create();
            using (var sha256 = SHA256.Create())
            {
                aesAlg.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(Key));
            }

            aesAlg.IV = GenerateRandomIv(); 
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }

        public static string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aesAlg = Aes.Create();
            using (var sha256 = SHA256.Create())
            {
                aesAlg.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(Key));
            }

            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aesAlg.IV = iv;

            var cipherBytes = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            var decrypt = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new MemoryStream(cipherBytes))
            using (var csDecrypt = new CryptoStream(msDecrypt, decrypt, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}