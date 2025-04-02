using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Service;
using DB.overcloud.Models;

namespace OverCloud.Services
{
    public class AccountService //프론트 <-> 프로그램 함수 호출
    {
        private readonly IAccountRepository repo;
        private readonly IStorageService repo_2;

        public AccountService(IAccountRepository repo, IStorageService repo_2)
        {
            this.repo = repo;
            this.repo_2 = repo_2;
        }

        // 계정 추가 (UI에서 호출)
        public bool AddAccount(CloudStorageInfo account)
        {
            return repo_2.AddCloudStorage(account);
        }

        // 계정 삭제 (UI에서 호출)
        public bool RemoveAccount(int userNum)
        {
            return repo.DeleteAccountByUserNum(userNum);
        }

        // 모든 계정 정보 조회 (UI에서 호출)
        public List<CloudStorageInfo> GetAllAccounts()
        {
            return repo.GetAllCloudAccounts();
        }
    }
}
