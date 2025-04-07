using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;

namespace OverCloud.Services
{
    //StorageUpdater: API에서 받아온 실제 클라우드의 사용량 데이터를 DB에 갱신하는 역할
    public class StorageUpdater
    {
        private readonly GoogleDriveService googleDriveService;
        private readonly IStorageRepository storageService;
        private readonly GoogleTokenProvider googleTokenProvider;


        public StorageUpdater()
        {
            googleDriveService = new GoogleDriveService(new GoogleTokenProvider(), new StorageRepository(DbConfig.ConnectionString));
            storageService = new StorageRepository(DbConfig.ConnectionString);
        }

        //일단은 구글 드라이브 한정 DB에 업데이트.
        public async Task<bool> SaveDriveQuotaToDB(string userEmail, int CloudStorageNum)
        {
            // 1. 구글 API로부터 용량 정보 받아오기
            var (total, used) = await googleDriveService.GetDriveQuotaAsync(userEmail);

            // 2. 객체 생성
            CloudStorageInfo cloud = new CloudStorageInfo
            {
                CloudStorageNum = CloudStorageNum,
                TotalCapacity = (int)(total / 1048576),
                UsedCapacity = (int)(used / 1048576)
            };

            // 3. 저장
            return storageService.account_save(cloud);
        }
    }
}