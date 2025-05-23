using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using static Google.Apis.Requests.BatchRequest;

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
            accessToken = null;
            Console.WriteLine($"accessToken:{accessToken}");
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = await oneDriveTokenRefresher.RefreshAccessTokenAsync(cloud);
                Console.WriteLine($"[OneDrive AccessToken] {accessToken}");
                
            }
            return !string.IsNullOrEmpty(accessToken); ;
        }

        public async Task<string> UploadFileAsync(CloudStorageInfo bestStorage, string filePath)
        {
            //var cloud = accountRepository.GetAllAccounts(userId)
            //    .FirstOrDefault(c => c.AccountId == userId);
            var oneCloud = storageRepository.GetCloud(bestStorage.CloudStorageNum);

            if (oneCloud == null)
                return null;

            string accessToken = await oneDriveTokenRefresher.RefreshAccessTokenAsync(oneCloud);
            if (string.IsNullOrEmpty(accessToken)) return null;


            var client = CreateClient();
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Length <= 100* 1024*1024) //100mb 이하-> 단일 업로드
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var uploadUrl = $"https://graph.microsoft.com/v1.0/me/drive/root:/{fileName}:/content";
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var response = await client.PutAsync(uploadUrl, new StreamContent(fileStream));

                if (!response.IsSuccessStatusCode) return null;

                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return json.RootElement.GetProperty("id").GetString();
            }
            else
            {
                return await UploadLargeFileViaSession(filePath, fileName, accessToken);
            }
        }

        private async Task<string> UploadLargeFileViaSession(string filePath, string fileName, string accessToken)
        {

            // 1. Upload Session 생성 (여기에만 토큰 필요)
            var sessionHandler = new HttpClientHandler();
            var sessionClient = new HttpClient(sessionHandler);

            sessionClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var sessionPayload = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["item"] = new Dictionary<string, object>
                {
                    ["@microsoft.graph.conflictBehavior"] = "rename",
                    ["name"] = fileName
                }
            });

            var sessionRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.microsoft.com/v1.0/me/drive/root:/{fileName}:/createUploadSession")
            {
                Content = new StringContent(sessionPayload, Encoding.UTF8, "application/json")
            };

            var sessionResponse = await sessionClient.SendAsync(sessionRequest);
            if (!sessionResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($" 세션 생성 실패: {sessionResponse.StatusCode}");
                return null;
            }

            var sessionJson = JsonDocument.Parse(await sessionResponse.Content.ReadAsStringAsync());
            string uploadUrl = sessionJson.RootElement.GetProperty("uploadUrl").GetString();

          //  Console.WriteLine($"Upload URL: {uploadUrl}");

            // 2. 조각 업로드( Authorization절대 붙이지않음)
            const int chunkSize = 100 * 1024 * 1024; // 320KB (microsoft 권장 크기)
            var fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;
            long uploaded = 0;

            var uploadHandler = new HttpClientHandler { AllowAutoRedirect = false };
            var uploadClient = new HttpClient(uploadHandler)
            {
                Timeout =TimeSpan.FromMinutes(10) //필요 시 더 늘려서 사용 가능.
            };

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            HttpResponseMessage finalResponse = null;

            while (uploaded < fileSize)
            {
                int thisChunkSize = (int)Math.Min(chunkSize, fileSize - uploaded);
                byte[] buffer = new byte[thisChunkSize];
                int read = await stream.ReadAsync(buffer, 0, thisChunkSize);
                if (read == 0) break;

                var byteArrayContent = new ByteArrayContent(buffer.Take(read).ToArray()); //read만큼 자르기
                byteArrayContent.Headers.Add("Content-Range", $"bytes {uploaded}-{uploaded + read - 1}/{fileSize}");

                //  요청 직접 구성 (인증 직접 포함)
                var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                {
                    Content = byteArrayContent
                };

           
                var chunkResponse = await uploadClient.SendAsync(request);
                Console.WriteLine($" 조각 업로드: {uploaded}-{uploaded + read - 1}/{fileSize} → {chunkResponse.StatusCode}");
                finalResponse = chunkResponse;

                if (!chunkResponse.IsSuccessStatusCode &&
                    chunkResponse.StatusCode != HttpStatusCode.Accepted &&
                    chunkResponse.StatusCode != HttpStatusCode.Created &&
                    chunkResponse.StatusCode != HttpStatusCode.OK)
                {
                    var error = await chunkResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($" 조각 업로드 실패 at byte {uploaded}");
                    Console.WriteLine($" 응답: {error}");
                    return null;
                }

                uploaded += read;
            }


            // 3. 완료 후 응답 처리
            if (finalResponse != null && finalResponse.IsSuccessStatusCode)
            {
                var resultJson = JsonDocument.Parse(await finalResponse.Content.ReadAsStringAsync());
                if (resultJson.RootElement.TryGetProperty("id", out var idProperty))
                {
                    Console.WriteLine("모든 조각 업로드 완료");
                    return idProperty.GetString();
                }
            }

            Console.WriteLine("모든 조각 업로드 완료했지만 ID를 찾을 수 없음");
            return null; 
        }


        public async Task<bool> DownloadFileAsync(int CloudStorageNum, string cloudFileId, string savePath)
        {   
                //  Console.WriteLine(userId); //여기서 userID는 구글게정, 원드 계정, 드롭계정 id
            Console.WriteLine("one DownloadFileAsync");

            var cloud = storageRepository.GetCloud(CloudStorageNum);

            if (cloud == null) return false;
            if (!await EnsureAccessTokenAsync(cloud)) return false;

            var client = CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);

            var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/drive/items/{cloudFileId}/content");
            if (!response.IsSuccessStatusCode) return false;

            using (var httpStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true)) //기본 버퍼 크기 ->80kb
            {
                await httpStream.CopyToAsync(fileStream);
            }

            return true;
        }

        public async Task<bool> DeleteFileAsync(int cloudStorageNum, string cloudFileId, string userId)
        {
            var cloud = accountRepository.GetAllAccounts(userId)
                .FirstOrDefault(c => c.CloudStorageNum == cloudStorageNum);
            if (cloud == null) return false;
            if (!await EnsureAccessTokenAsync(cloud)) return false;

            var client = CreateClient();
            var response = await client.DeleteAsync($"https://graph.microsoft.com/v1.0/me/drive/items/{cloudFileId}");

            return response.IsSuccessStatusCode;
        }

        public async Task<(ulong, ulong)> GetDriveQuotaAsync(int CloudStorageNum)
        {
            var oneCloud = storageRepository.GetCloud(CloudStorageNum);
           // var oneCloud = cloud.FirstOrDefault(c => c.ID == userId);

            if (oneCloud == null) return (0, 0);
            if (!await EnsureAccessTokenAsync(oneCloud)) return (0, 0);

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
