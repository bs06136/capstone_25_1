using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

namespace OverCloud.Services.FileManager.DriveManager
{
    public static class GoogleAuthHelper
    {
        private const string CredentialFile = "C:\\key\\credential.json";

        public static async Task<(string email, string RefreshToken, string ClientId, string ClientSecret)> AuthorizeAsync(string id)
        {
            // ✅ 이 부분 추가: 기존 토큰 삭제
            string tokenPath = Path.Combine("Tokens", $"{id}.TokenResponse-user");
            if (File.Exists(tokenPath))
                File.Delete(tokenPath);

            using var stream = new FileStream(CredentialFile, FileMode.Open, FileAccess.Read);
            var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

            // ✅ Flow 초기화
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = secrets,
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new FileDataStore("Tokens", true)
            });

            // ✅ 인증 요청 시 수동 설정
            var codeReceiver = new LocalServerCodeReceiver();

            var app = new AuthorizationCodeInstalledApp(flow, codeReceiver)
            {
                // 여기에 추가 설정 불가능. 대신 URL 수정 필요
            };

            // ✅ OAuth URL 생성 시 직접 access_type, prompt 설정
            var authUrl = new GoogleAuthorizationCodeRequestUrl(new Uri(flow.AuthorizationServerUrl))
            {
                ClientId = secrets.ClientId,
                Scope = string.Join(" ", new[] { DriveService.Scope.Drive }),
                RedirectUri = codeReceiver.RedirectUri,
                AccessType = "offline",   // ✅ 필수
                Prompt = "consent"        // ✅ 새로 로그인 강제
            }.Build().ToString();

            Console.WriteLine($"🔗 인증 URL: {authUrl}");

            // 이 단계에서 웹브라우저 수동 호출 or 자동 실행
            var result = await app.AuthorizeAsync(id ?? "user", CancellationToken.None);

            string refreshToken = result.Token.RefreshToken;
            string email = result.UserId;

            return (email, refreshToken, secrets.ClientId, secrets.ClientSecret);
        }

    }

}