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
       // private const string ClientId = "9be3b88a-60b4-404b-93e7-ace80ff849f2"; // 너가 Azure 등록하면서 받은 Client ID
        //private const string ClientSecret = "63820dd0-0d33-4d59-abb5-7d639f75fa62";

        public async Task<string> RefreshAccessTokenAsync(CloudStorageInfo cloud)
        {
            if (cloud == null) throw new ArgumentNullException(nameof(cloud));

            using var client = new HttpClient();
            var parameters = new Dictionary<string, string>
            {
                { "client_id", cloud.ClientId },
                { "refresh_token", cloud.RefreshToken },
                { "redirect_uri", "http://localhost:5000/" },
                { "grant_type", "refresh_token" },
                { "scope", "offline_access Files.ReadWrite.ALL User.Read" }
            };

            Console.WriteLine("🔑 OneDrive AccessToken 재발급 요청 파라미터:");
            foreach (var param in parameters)
            {
                Console.WriteLine($"{param.Key}: {param.Value}");
            }

            var response = await client.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", new FormUrlEncodedContent(parameters));
            Console.WriteLine($"🔑 AccessToken 재발급 요청 상태: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ OneDrive AccessToken 재발급 실패: {errorContent}");
                return null;
            }

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var token =json.RootElement.GetProperty("access_token").GetString();
            Console.WriteLine($"🔑 AccessToken 재발급 성공: {token}");
            return token;
        }

    }
}
