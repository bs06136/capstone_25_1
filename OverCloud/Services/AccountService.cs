using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using DB.overcloud.Models;
using overcloud;
using MySqlX.XDevAPI;
using System.Security.AccessControl;
using OverCloud.Services.FileManager.DriveManager;
using OverCloud.Services.StorageManager;

namespace OverCloud.Services
{
    public class AccountService //프론트 <-> 프로그램 함수 호출
    {
        private readonly IAccountRepository accountRepository; //OverClour의 계정 정보 
        private readonly IStorageRepository storageRepository; // OverCloud계정 하나 안에 또 다른 계정들의 정보
        private readonly QuotaManager quotaManager;
        
        public AccountService(IAccountRepository accountRepo, IStorageRepository storageRepo ,QuotaManager quotaMgr)
        {
            accountRepository = new AccountRepository(DbConfig.ConnectionString);
            storageRepository = new StorageRepository(DbConfig.ConnectionString);
            quotaManager = quotaMgr;
        }

        // 오버클라우드 계정에 새로운 계정 추가 (UI에서 호출)
        public async Task<bool> Add_Cloud_Storage(CloudStorageInfo storage)
        {
            
            if (storage.CloudType == "GoogleDrive")
            {
                var (email, refreshToken, clientId, clientSecret) = await GoogleAuthHelper.AuthorizeAsync(storage.AccountId);
                storage.AccountId = email;
                storage.RefreshToken = refreshToken;
                storage.ClientId = clientId;
                storage.ClientSecret = clientSecret;
                storage.UserNum = 1;                //자동으로 넣는 로직 필요함
                Console.WriteLine("구글 계정 추가중...");

            }
            else if (storage.CloudType == "OneDrive")
            {
                var (email, refreshToken, clientId, clientSecret) = await OneDriveAuthHelper.AuthorizeAsync(storage.AccountId);
                storage.AccountId = email;
                storage.RefreshToken = refreshToken;
                storage.ClientId = clientId;
                storage.ClientSecret = clientSecret;
                storage.UserNum = 1;
                Console.WriteLine("원드라이브 계정 추가중...");
            }



            bool result = storageRepository.AddCloudStorage(storage);

            if (result)
            {
                //계정 추가 성공시 바로 용량 업데이트 호출
                await quotaManager.SaveDriveQuotaToDB(storage.AccountId, storage.CloudStorageNum);

                // ⭐ StorageSessionManager에도 반영 (옵션)
                StorageSessionManager.Quotas.Add(new CloudQuotaInfo
                {
                    CloudStorageNum = storage.CloudStorageNum,
                    CloudType = storage.CloudType,
                    TotalCapacityMB = storage.TotalCapacity,
                    UsedCapacityMB = storage.UsedCapacity
                });

                         // 전체 합산 용량 업데이트도 할 수 있음
                quotaManager.UpdateAggregatedStorageForUser(storage.AccountId);
            }



            return result;

        }

        // 오버클라우드 계정에 있던 클라우드 하나 삭제 (UI에서 호출)
        public bool Delete_Cloud_Storage(int cloudStorageNum)
        {

            var clouds = accountRepository.GetAllAccounts("admin");
            var target = clouds.FirstOrDefault(c => c.CloudStorageNum == cloudStorageNum);
            
            if (target == null)
            {
                Console.WriteLine($" 삭제 실패 : cloudStorageNum {cloudStorageNum}에 해당하는 클라우드 계정이 없습니다.");
                return false;
            }

            bool result = storageRepository.DeleteCloudStorage(cloudStorageNum);
            if (result)
            {
                Console.WriteLine($" 클라우드 계정 삭제 성공 : cloudStorageNum {cloudStorageNum}");
            }
            else
            {
                Console.WriteLine($" 클라우드 계정 삭제 실패 : cloudStorageNum {cloudStorageNum}");
            }

            return result;
        }

        // 오버클라우드 계정 안의 모든 계정 정보 조회 (UI에서 호출)
        public List<CloudStorageInfo> GetCloudsForUser(string userId)
        {
            return accountRepository.GetAllAccounts(userId);
        }
   

    }
}
