using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Service;
using overcloud.Models;

namespace OverCloud.Services
{
    public class AccountManager //병합 전체 흐름 정리할 중간 관리자 클래스
    {
        private readonly IAccountRepository repo;
        private readonly GoogleDriveService googleDriveService; 

        public AccountManager(IAccountRepository repo, GoogleDriveService googleDriveService)
        {
            this.repo = repo;
            this.googleDriveService = googleDriveService;
        }

        public async Task<CloudAccountMerger> GetMergedAccountsAsync()
        {
            var accounts = repo.GetAllAccounts();

            foreach (var acc in accounts)
            {
                if (acc.CloudType == "GoogleDrive")
                {
                    var (total, used) = await googleDriveService.GetDriveQuotaAsync(acc.ID);
                    acc.TotalSize = total;
                    acc.UsedSize = used;
                }

                // dropbox, onedrive 등은 나중에 여기에 추가
            }

            return new CloudAccountMerger(accounts);
        }

       
    }

}
