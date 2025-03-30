using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class GoogleDriveService
{
    private const string TokenRootPath = "Tokens";
    private const string CredentialFile = "credential.json";

    public async Task<UserCredential> AuthenticateAsync(string userId)
    {
        using var stream = new FileStream(CredentialFile, FileMode.Open, FileAccess.Read);
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            new[] { DriveService.Scope.DriveReadonly },
            userId,  // 사용자 ID로 토큰 구분
            CancellationToken.None,
            new FileDataStore(TokenRootPath, true));

        return credential;
    }

    public async Task ListFilesAsync(string userId)
    {
        var credential = await AuthenticateAsync(userId);

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "OverCloud",
        });

        var request = service.Files.List();
        request.PageSize = 10;
        request.Fields = "files(id, name, size)";

        var result = await request.ExecuteAsync();

        Console.WriteLine($"\n📂 {userId}의 Google Drive 파일 목록:");
        foreach (var file in result.Files)
        {
            Console.WriteLine($"{file.Name} | ID: {file.Id} | Size: {file.Size ?? 0} bytes");
        }
    }

    public async Task DeleteTokenAsync(string userId)
    {
        var store = new FileDataStore(TokenRootPath, true);
        //await store.DeleteAsync<StoredCredential>(userId);
    }


    public async Task<(long total, long used)> GetDriveQuotaAsync(string userId)
    {
        var credential = await AuthenticateAsync(userId);

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "OverCloud"
        });

        var aboutRequest = service.About.Get();
        aboutRequest.Fields = "storageQuota";

        var about = await aboutRequest.ExecuteAsync();

        long total = Convert.ToInt64(about.StorageQuota.Limit);
        long used = Convert.ToInt64(about.StorageQuota.Usage);

        return (total, used);
    }
}