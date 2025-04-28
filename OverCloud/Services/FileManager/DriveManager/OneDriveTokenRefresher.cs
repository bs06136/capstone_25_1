using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using DB.overcloud.Models;

namespace OverCloud.Services.FileManager.DriveManager
{
    public class OneDriveTokenRefresher
    {
        public async Task<string> RefreshAccessTokenAsync(CloudStorageInfo cloud)
        {
            if (cloud == null) throw new ArgumentNullException(nameof(cloud));

            using var client = new HttpClient();

            var parameters = new Dictionary<string, string>
            {
                { "client_id", cloud.ClientId },
                { "scope", "offline_access Files.ReadWrite User.Read" },
                { "refresh_token", cloud.RefreshToken },
                { "grant_type", "refresh_token" },
                { "redirect_uri", "http://localhost:5000/" },
                { "client_secret", cloud.ClientSecret }
            };

            var content = new FormUrlEncodedContent(parameters);

            var response = await client.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ OneDrive AccessToken 재발급 실패: {errorContent}");
                return null;
            }

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("access_token").GetString();
        }
    }
}
