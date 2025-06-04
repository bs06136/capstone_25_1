using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OverCloud.Services.FileManager.DriveManager
{
    public static class DropboxAuthHelper
    {
        private const string CredentialFile = "C:\\key\\dropbox.json";

        public static async Task<(string AppKey, string AppSecret, string RefreshToken)> LoadDropboxCredentialsAsync()
        {
            if (!File.Exists(CredentialFile))
            {
                Console.WriteLine($"❌ Dropbox Credential 파일이 존재하지 않음: {CredentialFile}");
                return (null, null, null);
            }

            var json = await File.ReadAllTextAsync(CredentialFile);
            var doc = JsonDocument.Parse(json);

            string appKey = doc.RootElement.GetProperty("AppKey").GetString();
            string appSecret = doc.RootElement.GetProperty("AppSecret").GetString();
            string refreshToken = doc.RootElement.GetProperty("RefreshToken").GetString();

            Console.WriteLine("🔑 Dropbox 인증 정보 로드 완료");
            return (appKey, appSecret, refreshToken);
        }
    }
}
