// GoogleDriveService.cs (1~3단계 리팩토링: TokenProvider 기반 + 전체 호출 흐름 점검)
using DB.overcloud.Models;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using OverCloud.Services;
using Google.Apis.Http;
using System.IO;
using System.Net.Http;
using DB.overcloud.Repository;
using Google.Apis.Upload;

namespace OverCloud.Services.FileManager.DriveManager
{
    public class GoogleDriveService : ICloudFileService
    {
        private readonly GoogleTokenProvider tokenProvider;
        private readonly IStorageRepository storageRepo;
        private readonly AccountService accountService;
        private readonly IAccountRepository accountRepository;

        public GoogleDriveService(GoogleTokenProvider tokenProvider, IStorageRepository storageRepo, IAccountRepository accountRepository)
        {
            this.tokenProvider = tokenProvider;
            this.storageRepo = storageRepo;
            this.accountRepository = accountRepository;
        }

        private DriveService CreateDriveService(string accessToken)
        {
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = null,
                ApplicationName = "OverCloud",
                HttpClientFactory = new AccessTokenHttpClientFactory(accessToken)
            });
        }

        public async Task<string> UploadFileAsync(string userId, string file_name)
        {
            System.Diagnostics.Debug.WriteLine("UploadFileAsync 1");
            var clouds = accountRepository.GetAllAccounts("admin");
            var googleCloud = clouds.FirstOrDefault(c => c.CloudType == "GoogleDrive");
            if (googleCloud == null) return null;

            System.Diagnostics.Debug.WriteLine("UploadFileAsync 2");
            var accessToken = await tokenProvider.GetAccessTokenAsync(googleCloud);
            var service = CreateDriveService(accessToken);

            System.Diagnostics.Debug.WriteLine("UploadFileAsync 3");
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(file_name)
            };

            System.Diagnostics.Debug.WriteLine("UploadFileAsync 4");
            using var stream = new FileStream(file_name, FileMode.Open);
            var request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
            var result = await request.UploadAsync();

            System.Diagnostics.Debug.WriteLine("UploadFileAsync 5");
            if (result.Status == Google.Apis.Upload.UploadStatus.Completed)
            {
                return request.ResponseBody.Id; // Google Drive 파일 ID 반환
            }

            System.Diagnostics.Debug.WriteLine("UploadFileAsync 6");
            return null;
        }

        public async Task<bool> DownloadFileAsync(string userId, string cloudFileId, string savePath)
        {
            var clouds = accountRepository.GetAllAccounts("admin");
            var googleCloud = clouds.FirstOrDefault(c => c.CloudType == "GoogleDrive");
            if (googleCloud == null) return false;

            var accessToken = await tokenProvider.GetAccessTokenAsync(googleCloud);
            var service = CreateDriveService(accessToken);

            var request = service.Files.Get(cloudFileId);
            using var stream = new FileStream(savePath, FileMode.Create, FileAccess.Write);
            await request.DownloadAsync(stream);

            return true;
        }

        public async Task<bool> DeleteFileAsync(string userId, string fileId)
        {
            var clouds = accountRepository.GetAllAccounts("admin");
            var googleCloud = clouds.FirstOrDefault(c => c.CloudType == "GoogleDrive");
            if (googleCloud == null) return false;

            var accessToken = await tokenProvider.GetAccessTokenAsync(googleCloud);
            var service = CreateDriveService(accessToken);

            try
            {
                await service.Files.Delete(fileId).ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 파일 삭제 실패: {ex.Message}");
                return false;
            }
        }


        public async Task<(ulong, ulong)> GetDriveQuotaAsync(string userId)
        {
            var clouds = accountRepository.GetAllAccounts("admin");
            var googleCloud = clouds.FirstOrDefault(c => c.AccountId == userId);
            if (googleCloud == null) return (0, 0);

            var accessToken = await tokenProvider.GetAccessTokenAsync(googleCloud);
            var service = CreateDriveService(accessToken);

            var about = service.About.Get();
            about.Fields = "storageQuota";
            var result = await about.ExecuteAsync();

            ulong total = ulong.Parse(result.StorageQuota.Limit?.ToString() ?? "0");
            ulong used = ulong.Parse(result.StorageQuota.Usage?.ToString() ?? "0");
            return (total, used);
        }

        // 토큰 기반으로 DriveService에 인증 설정하는 HttpClientFactory
        // AccessToken 기반 HttpClientFactory
        private class AccessTokenHttpClientFactory : IHttpClientFactory
        {
            private readonly string accessToken;

            public AccessTokenHttpClientFactory(string accessToken)
            {
                this.accessToken = accessToken;
            }

            public ConfigurableHttpClient CreateHttpClient(CreateHttpClientArgs args)
            {
                var handler = new ConfigurableMessageHandler(new HttpClientHandler());
                var client = new ConfigurableHttpClient(handler)
                {
                    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                };
                return client;
            }
        }
    }
}