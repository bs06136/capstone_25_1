using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Service;

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

        public async Task<bool> SaveDriveQuotaToDB(string userEmail, int CloudStorageNum)
        {
            // 1. 구글 API로부터 용량 정보 받아오기
            var (total, used) = await googleDriveService.GetDriveQuotaAsync(userEmail);

            // 2. 객체 생성
            CloudStorageInfo cloud = new CloudStorageInfo
            {
                CloudStorageNum = CloudStorageNum,
                TotalSize = (int)(total/1048576),
                UsedSize = (int)(used/ 1048576)
            };

            // 3. 저장
            return storageService.account_save(cloud);
        }
    }
}