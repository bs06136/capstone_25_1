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

public class GoogleDriveService : ICloudFileService
{
    private readonly GoogleTokenProvider tokenProvider;
    private readonly IStorageRepository storageRepo;

    public GoogleDriveService(GoogleTokenProvider tokenProvider, IStorageRepository storageRepo)
    {
        this.tokenProvider = tokenProvider;
        this.storageRepo = storageRepo;
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

    public async Task <string> UploadFileAsync(string userId, string filePath)
    {
        var clouds = storageRepo.GetCloudsForUser(userId);
        var googleCloud = clouds.FirstOrDefault(c => c.CloudType == "GoogleDrive");
        if (googleCloud == null) return null;

        var accessToken = await tokenProvider.GetAccessTokenAsync(googleCloud);
        var service = CreateDriveService(accessToken);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(filePath)
        };

        using var stream = new FileStream(filePath, FileMode.Open);
        var request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
        var result = await request.UploadAsync();

        if (result.Status == Google.Apis.Upload.UploadStatus.Completed)
        {
            return request.ResponseBody.Id; // Google Drive 파일 ID 반환
        }

        return null;
    }

    public async Task<bool> DownloadFileAsync(string userId, string cloudFileId, string savePath)
    {
        var clouds = storageRepo.GetCloudsForUser(userId);
        var googleCloud = clouds.FirstOrDefault(c => c.CloudType == "GoogleDrive");
        if (googleCloud == null) return false;

        var accessToken = await tokenProvider.GetAccessTokenAsync(googleCloud);
        var service = CreateDriveService(accessToken);

        var request = service.Files.Get(cloudFileId);
        using var stream = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        await request.DownloadAsync(stream);

        return true;
    }

    public async Task<(long, long)> GetDriveQuotaAsync(string userId)
    {
        var clouds = storageRepo.GetCloudsForUser(userId);
        var googleCloud = clouds.FirstOrDefault(c => c.CloudType == "GoogleDrive");
        if (googleCloud == null) return (0, 0);

        var accessToken = await tokenProvider.GetAccessTokenAsync(googleCloud);
        var service = CreateDriveService(accessToken);

        var about = service.About.Get();
        about.Fields = "storageQuota";
        var result = await about.ExecuteAsync();

        long total = long.Parse(result.StorageQuota.Limit?.ToString() ?? "0");
        long used = long.Parse(result.StorageQuota.Usage?.ToString() ?? "0");
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
