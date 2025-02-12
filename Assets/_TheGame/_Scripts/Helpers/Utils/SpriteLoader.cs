using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace _TheGame._Scripts.Helpers.Utils
{
    public static class SpriteLoader
    {
        public static async Task<Sprite> LoadSpriteFromUrl(string url)
        {
            var cachedImage = await LoadCachedImage(url);
            if (cachedImage != null)
            {
                return ByteToSprite(cachedImage);
            }


            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                byte[] imageData = await response.Content.ReadAsByteArrayAsync();
                CacheImage(url, imageData);
                return ByteToSprite(imageData);
            }
            else
            {
                Debug.LogError($"Failed to load image from URL: {url}. Error: {response.StatusCode}");
                return null;
            }
        }

        private static async void CacheImage(string url, byte[] imageData)
        {
            string filePath = Path.Combine(Application.persistentDataPath, MD5Hash(url));
            await File.WriteAllBytesAsync(filePath, imageData);
        }


        private static Sprite ByteToSprite(byte[] imageData)
        {
            var texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }


        private static async Task<byte[]> LoadCachedImage(string url)
        {
            string filePath = Path.Combine(Application.persistentDataPath, MD5Hash(url));
            if (File.Exists(filePath))
            {
                return await File.ReadAllBytesAsync(filePath);
            }

            return null;
        }

        private static string MD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}