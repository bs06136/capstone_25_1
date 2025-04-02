using System.Threading.Tasks;
using OverCloud.Models;

namespace OverCloud.Services
{
    public class StorageUpdater
    {
        private readonly GoogleDriveService googleDriveService;
        private readonly IStorageService storageService;

        public StorageUpdater(GoogleDriveService googleDriveService, IStorageService storageService)
        {
            this.googleDriveService = googleDriveService;
            this.storageService = storageService;
        }

        public async Task<bool> SaveDriveQuotaToDB(string userEmail, int userNum)
        {
            // 1. 구글 API로부터 용량 정보 받아오기
            var (total, used) = await googleDriveService.GetDriveQuotaAsync(userEmail);

            // 2. 객체 생성
            CloudStorageInfo cloud = new CloudStorageInfo
            {
                AccountUserNum = userNum,
                TotalCapacity = total,
                UsedCapacity = used
            };

            // 3. 저장
            return storageService.account_save(cloud);
        }
    }
}