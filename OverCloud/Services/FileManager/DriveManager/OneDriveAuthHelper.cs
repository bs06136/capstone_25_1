using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Identity.Client; // Microsoft Authentication Library (MSAL) 필요
using DB.overcloud.Models;
using MySql.Data.MySqlClient;

namespace OverCloud.Services.FileManager.DriveManager
{
    public static class OneDriveAuthHelper
    {
        private const string clientId = "9be3b88a-60b4-404b-93e7-ace80ff849f2"; // 너가 Azure 등록하면서 받은 Client ID
        private const string tenant = "consumers"; // 개인 Microsoft 계정은 "consumers" 
        private const string redirectUri = "http://localhost:5000/"; // 등록한 리다이렉션 URL

        public static async Task<(string email, string refreshToken, string clientId, string clientSecret)> AuthorizeAsync(string dummyId)
        {
            var app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenant)
                .WithRedirectUri(redirectUri)
                .Build();

            string[] scopes = { "Files.ReadWrite.All", "offline_access", "User.Read" };

            try
            {
                var result = await app.AcquireTokenInteractive(scopes)
                    .WithPrompt(Prompt.SelectAccount) // 사용자에게 계정선택 요구
                    .ExecuteAsync();

                // RefreshToken은 PublicClientApplication에서는 바로 접근이 어려움
                // accessToken만 제공됨 (refresh는 내부적으로 갱신됨)
                // workaround: graph API로 추가로 가져와야 할 수 있음

                // 여기선 AccessToken 기반으로 User 정보 가져오는 코드 작성
                var email = await GetUserEmailAsync(result.AccessToken);

                return (email, result.AccessToken, clientId, "");
                // OneDrive는 ClientSecret 없이 Public Client로 동작함 (빈값 반환)
            }
            catch (MsalException ex)
            {
                Console.WriteLine($"❌ MSAL 서비스 에러: {ex.Message}");
                Console.WriteLine($"❌ 코드: {ex.ErrorCode}");
                Console.WriteLine($"❌ 상세: {ex.InnerException?.Message}");
                throw;
            }
        }

        private static async Task<string> GetUserEmailAsync(string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me/");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            return doc.RootElement.GetProperty("userPrincipalName").GetString();
        }
    }
}
