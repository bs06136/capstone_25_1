using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverCloud.Models;

namespace OverCloud.Services
{   //계정 CRUD , 병합 로직
    public class AccountService
    {
        private readonly AccountRepository repository = new AccountRepository();
        
        //계정 추가 로직
        public bool AddAccount(CloudAccountInfo account)
        {
            if (IsDuplicate(account.ID))
                return false;

            return repository.InsertAccount(account);
        }
        
        //계정 삭제
        public bool RemoveAccount(int userNum)
        {
            return repository.DeleteAccountByUserNum(userNum);
        }

        //DB에 저장된 모든 클라우드 계정을 보여주는 함수
        public List<CloudAccountInfo> GetAllAccounts()
        {
            return repository.GetAllAccounts();
        }

        //// 특정 클라우드 유형(GoogleDrive, Dropbox 등)에 해당하는 계정만 필터링
        //public List<CloudAccountInfo> GetAccountsByCloud(string cloudType)
        //{
        //    return repository.GetAllAccounts()
        //                     .FindAll(a => a.CloudType == cloudType);
        //}

        //중복 검사 로직
        private bool IsDuplicate(string id)
        {
            return repository.GetAllAccounts().Any(a => a.ID == id);
        }
    }
}
