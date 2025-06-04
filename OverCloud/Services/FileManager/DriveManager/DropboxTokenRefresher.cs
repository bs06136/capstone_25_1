using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;

namespace OverCloud.Services.FileManager.DriveManager
{
    public class DropboxTokenRefresher
    {
        public async Task<string> RefreshAccessTokenAsync(CloudStorageInfo cloud)
        {
            if (cloud == null)
                throw new ArgumentNullException(nameof(cloud));

            using var client = new HttpClient();

            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", cloud.RefreshToken }
            };

            var authHeader = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{cloud.ClientId}:{cloud.ClientSecret}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

            Console.WriteLine("🔑 Dropbox AccessToken 재발급 요청 파라미터:");
            foreach (var param in parameters)
            {
                Console.WriteLine($"{param.Key}: {param.Value}");
            }

            var response = await client.PostAsync("https://api.dropbox.com/oauth2/token", new FormUrlEncodedContent(parameters));
            Console.WriteLine($"🔑 Dropbox AccessToken 재발급 요청 상태: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Dropbox AccessToken 재발급 실패: {errorContent}");
                return null;
            }

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var token = json.RootElement.GetProperty("access_token").GetString();
            Console.WriteLine($"🔑 AccessToken 재발급 성공: {token}");
            return token;
        }
    }
}
