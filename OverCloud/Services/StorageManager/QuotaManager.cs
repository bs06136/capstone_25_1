using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;
using OverCloud.Services.FileManager.DriveManager;

namespace OverCloud.Services.StorageManager
{
    public class QuotaManager
    {

        private readonly IEnumerable<ICloudFileService> cloudServices;
        // private readonly DropboxService dropboxService;
        //private readonly OneDriveService oneDriveService;
        private readonly IAccountRepository accountRepository;
        private readonly IStorageRepository storageRepository;
        private AccountService accountService;

        public QuotaManager(IEnumerable<ICloudFileService> cloudServices, IStorageRepository storageRepo, IAccountRepository accountRepo)
        {
            storageRepository = storageRepo;
            accountRepository = accountRepo;

            this.cloudServices = cloudServices;
           //googleDriveService = new GoogleDriveService(new GoogleTokenProvider(), storageRepository);
            // dropboxService = new DropboxService();
            // oneDriveService = new OneDriveService();
        }

        //계정에 있는 모든 스토리지의 용량 업데이트

        public bool UpdateAggregatedStorageForUser(string userId) //여기서 넘기는 userId는 overcloud계정의 id임요.
        {
            // 1. 해당 계정이 가진 모든 클라우드 가져오기
            var cloudList = accountRepository.GetAllAccounts(userId);
            if (cloudList == null || cloudList.Count == 0)
                return false;

//            int userNum = cloudList.First().UserNum;

            // 2. 합산
            ulong totalSize = cloudList.Aggregate(0UL, (acc, c) => acc + c.TotalCapacity);
            ulong usedSize = cloudList.Aggregate(0UL, (acc, c) => acc + c.UsedCapacity);


            // 3. DB 업데이트
            return accountRepository.UpdateAccountUsage(userId, totalSize, usedSize);
        }

      



        // 계정에 있는 특정 클라우드 하나만 용량 업데이트 (일단은 구글 드라이브 한정 DB에 업데이트)
        public async Task<bool> SaveDriveQuotaToDB(string userId, int CloudStorageNum) //오버클라우드 userid를 넘겨줌.
        {
           
            // 1. userEmail에 맞는 클라우드 타입 찾기
            var cloudInfo = storageRepository.GetCloud(CloudStorageNum);
            if (cloudInfo == null)
            {
                Console.WriteLine("❌ 해당 이메일에 맞는 클라우드 정보를 찾을 수 없습니다.");
                return false;
            }

            string cloudType = cloudInfo.CloudType;


                 // 2. 클라우드 타입에 맞는 서비스 찾기
            var service = cloudServices.FirstOrDefault(s =>
                s.GetType().Name.StartsWith(cloudType)); // "GoogleDriveService", "DropboxService" 같은 이름 비교

            if (service == null)
            {
                Console.WriteLine("❌ 해당 클라우드에 맞는 서비스 구현체를 찾을 수 없습니다.");
                return false;
            }

            // 3. 해당 클라우드에 API 호출
            var (total, used) = await service.GetDriveQuotaAsync(CloudStorageNum);


            // 5. TotalCapacity, UsedCapacity만 업데이트 (KB단위)
            cloudInfo.TotalCapacity = (total / 1024);
            cloudInfo.UsedCapacity = (used / 1024);

            // 💡 메모리 세션도 갱신
            StorageSessionManager.SetQuota(
                cloudStorageNum: cloudInfo.CloudStorageNum,
                accountId : cloudInfo.AccountId,
                cloudType: cloudInfo.CloudType,
                totalKB: cloudInfo.TotalCapacity,
                usedKB: cloudInfo.UsedCapacity
            );

            // 3. 저장
            return storageRepository.account_save(cloudInfo);
        }



        //업로드 or 삭제 시 스토리지 용량 최신화.
        public void UpdateQuotaAfterUploadOrDelete(int cloudStorageNum, ulong fileSizeKB, bool isUpload)
        {
            var quota = StorageSessionManager.Quotas.FirstOrDefault(q => q.CloudStorageNum == cloudStorageNum);
            Console.WriteLine($" 업로드 or 삭제 반영 전: quota.Total = {quota.TotalCapacityKB}");
            Console.WriteLine($" 업로드 or 삭제 반영 전: quota.Used = {quota.UsedCapacityKB}");
            if (quota == null)
            {
                Console.WriteLine($"❌ quota not found for CloudStorageNum: {cloudStorageNum}");
                return;
            }

            if (isUpload)
                quota.UsedCapacityKB += fileSizeKB;
            else
                quota.UsedCapacityKB -= fileSizeKB;


            Console.WriteLine($" 업로드 or 삭제 반영 후: quota.Used = {quota.UsedCapacityKB}");

            var cloudInfo = new CloudStorageInfo
            {
                CloudStorageNum = quota.CloudStorageNum,
                TotalCapacity = quota.TotalCapacityKB,
                UsedCapacity = quota.UsedCapacityKB
            };

            bool dbResult = storageRepository.account_save(cloudInfo);
            Console.WriteLine(dbResult ? "✅ DB 저장 성공" : "❌ DB 저장 실패");

        }



        //만약 UI에서 새로고침버튼 누를때 전체 새로 업데이트하고 싶으면 사용
        //로그인/ 계정추가/ 삭제 후 강제로 메모리 초기화하고 새로 불러올 때 사용 가능.
        public void RefreshAllQuotas(string userId) //오버클라우드 아이디
        {
            StorageSessionManager.Quotas.Clear();

            var cloudList = accountRepository.GetAllAccounts(userId);
            foreach (var cloud in cloudList)
            {
                StorageSessionManager.Quotas.Add(new CloudQuotaInfo
                {
                    CloudStorageNum = cloud.CloudStorageNum,
                    CloudType = cloud.CloudType,
                    TotalCapacityKB = cloud.TotalCapacity,
                    UsedCapacityKB = cloud.UsedCapacity
                });
            }
        }

    }

}