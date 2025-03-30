using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Service;

namespace OverCloud.Services
{
    public class AccountManager //병합 전체 흐름 정리할 중간 관리자 클래스
    {
        private readonly IAccountRepository repo;
        // private readonly GoogleDriveService googleDriveService; //아직 구글드라이브 API연동 안해서 나중에 사용

        public AccountManager(IAccountRepository repo)
        {
            this.repo = repo;
            //           this.googleDriveService = driveService;
        }

        //public async Task<CloudAccountMerger> GetMergedAccountsAsync()
        //{
        //    var accounts = repo.GetAllAccounts();

        //    foreach (var acc in accounts)
        //    {
        //        if (acc.CloudType == "GoogleDrive")
        //        {
        //            var (total, used) = await googleDriveService.GetDriveQuotaAsync(acc.ID);
        //            acc.TotalSize = total;
        //            acc.UsedSize = used;
        //        }

        //        // dropbox, onedrive 등은 나중에 여기에 추가
        //    }

        //    return new CloudAccountMerger(accounts);

        public CloudAccountMerger GetMergedAccounts()
        {
            var accounts = repo.GetAllAccounts();

            // 지금은 API 대신 하드코딩된 용량 (가상)
            foreach (var acc in accounts)
            {
                acc.TotalSize = 15_000_000_000; // 15GB
                acc.UsedSize = 3_000_000_000;   // 3GB
            }

            return new CloudAccountMerger(accounts);
        }

    }

}
