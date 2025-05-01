using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Identity.Client; // Microsoft Authentication Library (MSAL) 필요
using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using overcloud.Views;
using System.Net;
using System.Text;


using System.Windows; // Application 객체를 사용하려면 필요
using System.Windows.Threading;       // Dispatcher를 사용하려면 필요



namespace OverCloud.Services.FileManager.DriveManager
{
    public static class OneDriveAuthHelper
    {
        private const string CredentialFile = "C:\\key\\onedrive_credential.json";

        private const string Authority = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";// 개인 Microsoft 계정은 "consumers" 

        //private static readonly string[] Scopes = {
        //"offline_access", "Files.ReadWrite.All", "User.Read"
        //};

        public static async Task<(string email, string refreshToken, string clientId, string clientSecret)> AuthorizeAsync(string dummyId)
        {

            var config = OneDriveCredentialConfig.Load(CredentialFile);
            
            string ClientId = config.client_id; // 너가 Azure 등록하면서 받은 Client ID
            string RedirectUri = config.redirect_uri;
            string scopeString = string.Join(" ", config.scopes);

            // 1. 브라우저로 사용자 인증
            string authUrl = $"{Authority}?client_id={ClientId}&response_type=code&redirect_uri={RedirectUri}&response_mode=query&scope={scopeString}&state=12345";
            Console.WriteLine("브라우저 열기: " + authUrl);

            // 1.5  브라우저 열기. 
            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

  


            // 2. 사용자가 URL에서 code 복사해서 콘솔에 입력
            Console.Write("🔐 인증 후 받은 code를 입력하세요: ");
            Console.Write("✏️ code 입력: ");

            string code = null;

            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(RedirectUri);
                listener.Start();

                // 2. 브라우저 열기
                //Process.Start(new ProcessStartInfo
                //{
                //    FileName = authUrl,
                //    UseShellExecute = true
                //});

                // 3. 리디렉션 대기 후 code 추출
                var context = await listener.GetContextAsync();
                var req = context.Request;
                var resp = context.Response;

                code = req.QueryString["code"];
                string state = req.QueryString["state"];

                const string responseString = "<html><body><h2>\uC778\uC99D \uC644\uB8CC! \uCC3D\uC744 \uB2EB\uC73C\uC138\uC694.</h2></body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                resp.ContentLength64 = buffer.Length;
                await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                resp.OutputStream.Close();

                listener.Stop();
                Console.WriteLine("code: " + code);
            }

            if (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("❌ code가 null 또는 빈 문자열입니다");
                throw new Exception("Code가 입력되지 않았습니다.");
            }

            // 4. 토큰 요청
            using var client = new HttpClient();
            var parameters = new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "scope", scopeString },
                { "code", code },
                { "redirect_uri", RedirectUri },
                { "grant_type", "authorization_code" }
            };

            Console.WriteLine(" 요청 파라미터:");
            foreach (var kv in parameters)
            {
                Console.WriteLine($"{kv.Key} = {kv.Value}");
            }

            HttpResponseMessage response;

            try
            {
                response = await client.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", new FormUrlEncodedContent(parameters));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HTTP 요청 예외 발생: {ex.Message}");
                return (null, null, null, null);
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($" 응답 코드: {response.StatusCode}");
            Console.WriteLine($" 응답 본문: {content}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ 토큰 요청 실패 - DB 저장 불가");
                return (null, null, null, null);
            }

            string accessToken, refreshToken;
            try
            {
                var json = JsonDocument.Parse(content);
                accessToken = json.RootElement.GetProperty("access_token").GetString();
                refreshToken = json.RootElement.GetProperty("refresh_token").GetString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ 토큰 파싱 실패: " + ex.Message);
                return (null, null, null, null);
            }

            string email = await GetUserEmailAsync(accessToken);
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("❌ 사용자 이메일 조회 실패");
                return (null, null, null, null);
            }

            Console.WriteLine("✅ OneDrive 인증 성공: " + email);
            return (email, refreshToken, ClientId, null);
        }


        private static async Task<string> GetUserEmailAsync(string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("📡 OneDrive 사용자 정보 응답: " + content);
                var doc = JsonDocument.Parse(content);

                return doc.RootElement.GetProperty("userPrincipalName").GetString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(" 이메일 조회 중 예외: " + ex.Message);
                return null;
            }
        }

    }
}
