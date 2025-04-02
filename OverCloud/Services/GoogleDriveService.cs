using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.IO;

public class GoogleDriveService
{
    private const string TokenRootPath = "Tokens";
    private const string CredentialFile = "C:\\Users\\bszxc\\source\\repos\\bs06136\\capstone_25_1\\UI\\overcloud\\overcloud\\bae.json";

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
}
