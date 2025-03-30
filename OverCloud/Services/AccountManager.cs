using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverCloud.Services
{
    public class AccountManager
    {
        private readonly IAccountRepository repo;
        private readonly GoogleDriveService googleDriveService;

        AccountManager(IAccountRepository repo, GoogleDriveService driveService)
        {
            this.repo = repo;
            this.googleDriveService = driveService;
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
