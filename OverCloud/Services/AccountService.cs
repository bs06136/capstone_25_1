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

namespace OverCloud.Services
{
    public class AccountService //프론트 <-> 프로그램 함수 호출
    {
        private readonly IAccountRepository accountRepository; //OverClour의 계정 정보 
        private readonly IStorageRepository storageRepository; // OverCloud계정 하나 안에 또 다른 계정들의 정보
        private readonly StorageAggregator aggregator;

        public AccountService()
        {
            accountRepository = new AccountRepository(DbConfig.ConnectionString);
            storageRepository = new StorageRepository(DbConfig.ConnectionString);
            aggregator = new StorageAggregator();
        }

        // 오버클라우드 계정에 새로운 계정 추가 (UI에서 호출)
        public async Task<bool> AddCloudStorage(CloudStorageInfo account)
        {
           // var account = await GoogleAuthHelper.AuthorizeAsync();
           if (account.CloudType =="GoogleDrive")
            {
                var (email, refreshToken, clientId, clientSecret) = await GoogleAuthHelper.AuthorizeAsync(account.AccountId);

                account.AccountId = email;
                account.RefreshToken = refreshToken;
                account.ClientId = clientId;
                account.ClientSecret = clientSecret;
                account.UserNum = 1;                //자동으로 넣는 로직 필요함
                Console.WriteLine("구글 계정 추가중...");

            }

            bool result = storageRepository.AddCloudStorage(account);

            if(result)
            {
                aggregator.UpdateAggregatedStorageForUser("admin");
            }


           //계정 추가할때 용량정보 업데이트 함수 호출
            aggregator.UpdateAggregatedStorageForUser("admin");

          
            return storageRepository.AddCloudStorage(account);
        }

        // 오버클라우드 계정에 있던 클라우드 하나 삭제 (UI에서 호출)
        public bool DeleteCloudStorage(int userNum)
        {

            var clouds = accountRepository.GetAllAccounts();
            var target = clouds.FirstOrDefault(c => c.UserNum == userNum);

            if (target != null)
            {
                Console.WriteLine($" 삭제 실패 : userNum {userNum}에 해당하는 클라우드 계정이 없습닏.");
            }

            bool result = accountRepository.DeleteAccountByUserNum(userNum);
            
            if (result) {
                //계정 삭제할 때 용량정보 업데이트 함수 호출
                aggregator.UpdateAggregatedStorageForUser("admin");
                   
            }

            return accountRepository.DeleteAccountByUserNum(userNum);
        }

        // 오버클라우드 계정 안의 모든 계정 정보 조회 (UI에서 호출)
        public List<CloudStorageInfo> GetCloudsForUser()
        {
            return accountRepository.GetAllAccounts();
        }
    }
}
