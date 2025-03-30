﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverCloud.Services
{
    public class AccountService //프론트 <-> 프로그램 함수 호출
    {
        private readonly IAccountRepository accountRepo;

        AccountService(IAccountRepository repo)
        {
            accountRepo = repo;
        }

        // 계정 추가 (UI에서 호출)
        public bool AddAccount(CloudAccountInfo account)
        {
            return accountRepo.InsertAccount(account);
        }

        // 계정 삭제 (UI에서 호출)
        public bool RemoveAccount(int userNum)
        {
            return accountRepo.DeleteAccountByUserNum(userNum);
        }

        // 모든 계정 정보 조회 (UI에서 호출)
        public List<CloudAccountInfo> GetAllAccounts()
        {
            return accountRepo.GetAllAccounts();
        }
    }
}
