using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.MessageBox;


namespace OverCloud.Services
{
    public class FileUploadManager
    {
        private readonly AccountService accountService;
        private readonly GoogleDriveService googleDriveService;

        public FileUploadManager(AccountService accountService, GoogleDriveService googleDriveService)
        {
            this.accountService = accountService;
            this.googleDriveService = googleDriveService;
        }

        public async Task<bool> file_upload(string file_name)
        {
            return await googleDriveService.UploadFileAsync("ojw73366@gamil.com", file_name);

            var accounts = accountService.GetAllAccounts();
            var googleAccount = accounts.FirstOrDefault(a => a.CloudType == "GoogleDrive");

            if (googleAccount == null)
            {
              //  Sysyem.MessageBox.Show("Google Drive 계정이 없습니다.");
                return false;
            }

            return await googleDriveService.UploadFileAsync(googleAccount.AccountId, file_name);
        }
    }
}
