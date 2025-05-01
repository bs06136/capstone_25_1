using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;

namespace OverCloud.Services.FileManager.DriveManager
{
    public class OneDriveService : ICloudFileService
    {
        private readonly OneDriveTokenRefresher oneDriveTokenRefresher;
        private readonly IStorageRepository storageRepository;
        private readonly IAccountRepository accountRepository;
        private string accessToken; // 동적으로 갱신

        public OneDriveService(OneDriveTokenRefresher oneDriveTokenRefresher, IStorageRepository storageRepo, IAccountRepository accountRepository)
        {
            this.oneDriveTokenRefresher = oneDriveTokenRefresher;
            this.storageRepository = storageRepo;
            this.accountRepository = accountRepository;
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

        private async Task<bool> EnsureAccessTokenAsync(CloudStorageInfo cloud)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = await oneDriveTokenRefresher.RefreshAccessTokenAsync(cloud);
                Console.WriteLine($"[OneDrive AccessToken] {accessToken}");
                
            }
            return !string.IsNullOrEmpty(accessToken); ;
        }

        public async Task<string> UploadFileAsync(string userId, string filePath)
        {
            var cloud = accountRepository.GetAllAccounts("admin")
                .FirstOrDefault(c => c.CloudType == "OneDrive");

            if (cloud == null) return null;
            if (!await EnsureAccessTokenAsync(cloud)) return null;

            var client = CreateClient();
            
            var fileName = Path.GetFileName(filePath);
            var uploadUrl = $"https://graph.microsoft.com/v1.0/me/drive/root:/{fileName}:/content";

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var response = await client.PutAsync(uploadUrl, new StreamContent(fileStream));

            if (!response.IsSuccessStatusCode) return null;

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("id").GetString();
        }

        public async Task<bool> DownloadFileAsync(string userId, string cloudFileId, string savePath)
        {
            var cloud = accountRepository.GetAllAccounts("admin")
                .FirstOrDefault(c => c.CloudType == "OneDrive");

            if (cloud == null) return false;
            if (!await EnsureAccessTokenAsync(cloud)) return false;

            var client = CreateClient();
            var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/drive/items/{cloudFileId}/content");

            if (!response.IsSuccessStatusCode) return false;

            using var stream = new FileStream(savePath, FileMode.Create, FileAccess.Write);
            await response.Content.CopyToAsync(stream);

            return true;
        }

        public async Task<bool> DeleteFileAsync(string userId, string cloudFileId)
        {
            var cloud = accountRepository.GetAllAccounts("admin")
                .FirstOrDefault(c => c.CloudType == "OneDrive");

            if (cloud == null) return false;
            if (!await EnsureAccessTokenAsync(cloud)) return false;

            var client = CreateClient();
            var response = await client.DeleteAsync($"https://graph.microsoft.com/v1.0/me/drive/items/{cloudFileId}");

            return response.IsSuccessStatusCode;
        }

        public async Task<(ulong, ulong)> GetDriveQuotaAsync(string overcloud_id)
        {
            var cloud = accountRepository.GetAllAccounts("admin")
                .FirstOrDefault(c => c.CloudType == "OneDrive");

            if (cloud == null) return (0, 0);
            if (!await EnsureAccessTokenAsync(cloud)) return (0, 0);

            var client = CreateClient();
            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me/drive");

            var rawJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[OneDrive Drive API Raw Response] {rawJson}");

            if (!response.IsSuccessStatusCode) return (0, 0);

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            ulong total = json.RootElement.GetProperty("quota").GetProperty("total").GetUInt64();
            ulong used = json.RootElement.GetProperty("quota").GetProperty("used").GetUInt64();

            return (total, used);
        }
    }
}
