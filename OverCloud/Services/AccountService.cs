using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using DB.overcloud.Models;
using overcloud;

namespace OverCloud.Services
{
    public class AccountService //프론트 <-> 프로그램 함수 호출
    {
        private readonly IAccountRepository repo_account; //OverClour의 계정 정보 
        private readonly IStorageRepository repo_storage; // OverCloud계정 하나 안에 또 다른 계정들의 정보

        public AccountService()
        {
            repo_account = new AccountRepository(DbConfig.ConnectionString);
            repo_storage = new StorageRepository(DbConfig.ConnectionString);
        }

        // 오버클라우드 계정에 새로운 계정 추가 (UI에서 호출)
        public bool AddCloudStorage(CloudStorageInfo account)
        {

            return repo_storage.AddCloudStorage(account);
        }

        // 오버클라우드 계정에 있던 클라우드 하나 삭제 (UI에서 호출)
        public bool DeleteCloudStorage(int userNum)
        {
            return repo_account.DeleteAccountByUserNum(userNum);
        }

        // 오버클라우드 계정 안의 모든 계정 정보 조회 (UI에서 호출)
        public List<CloudStorageInfo> GetCloudsForUser()
        {
            return repo_account.GetAllAccounts();
        }
    }
}
