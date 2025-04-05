using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.IO;

namespace OverCloud.Services
{

    public class GoogleDriveService : ICloudFileService
    {
        private const string TokenRootPath = "Tokens";
        private const string CredentialFile = "C:\\key\\bae.json";

        public async Task<UserCredential> AuthenticateAsync(string userId)
        {
            using var stream = new FileStream(CredentialFile, FileMode.Open, FileAccess.Read);
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { DriveService.Scope.Drive },
                userId,
                CancellationToken.None,
                new FileDataStore(TokenRootPath, true)
            );
        }

        //파일 업로드
        public async Task<bool> UploadFileAsync(string userId, string filePath)
        {
            var credential = await AuthenticateAsync(userId);
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "OverCloud"
            });

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath)
            };

            using var stream = new FileStream(filePath, FileMode.Open);
            var request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
            var result = await request.UploadAsync();

            return result.Status == Google.Apis.Upload.UploadStatus.Completed;
        }


        //용량정보를 받아오기 위해 호출하는 함수
        public async Task<(long, long)> GetDriveQuotaAsync(string userId)
        {
            var credential = await AuthenticateAsync(userId);
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "OverCloud"
            });

            var about = service.About.Get();
            about.Fields = "storageQuota";
            var result = await about.ExecuteAsync();

            return (long.Parse(result.StorageQuota.Limit.ToString()), long.Parse(result.StorageQuota.Usage.ToString()));
        }


        //파일 다운로드 함수 : fileId는 구글 드라이브 내부의 실제 ID값이다.
        public async Task<bool> DownloadFileAsync(string userId, string cloudFileId, string savePath)
        {
            var credential = await AuthenticateAsync(userId); // 실제 로그인된 계정 Id 사용
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "OverCloud"
            });

            var request = service.Files.Get(cloudFileId);

            using var stream = new FileStream(savePath, FileMode.Create, FileAccess.Write);
            await request.DownloadAsync(stream);

            return true;
        }



    }
}