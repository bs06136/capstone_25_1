using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace OverCloud.Services
{
    public class FileUploadManager
    {
        private readonly AccountService accountService;
        private readonly GoogleDriveService googleDriveService;
        private readonly FileService fileService;

        public FileUploadManager()
        {
            accountService = new AccountService();
            googleDriveService = new GoogleDriveService();
        }

        
        public async Task<bool> file_upload(string file_name)
        {
            //return await googleDriveService.UploadFileAsync("ojw73366@gamil.com", file_name);

            var accounts = accountService.GetCloudsForUser();
            var googleAccount = accounts.FirstOrDefault(a => a.CloudType == "Google Drive");

            if (googleAccount == null)
            {

                System.Windows.MessageBox.Show("Google Drive 계정이 없습니다.");
                return false;
            }

            var file = fileService.GetFile(fileId);
            fileService.SaveFile(file);
            return await googleDriveService.UploadFileAsync(googleAccount.AccountId + "@gmail.com", file_name);
        }
    }
}
