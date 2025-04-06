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
            var googleAccount = accounts.FirstOrDefault(a => a.CloudType == "GoogleDrive");

            if (googleAccount == null)
            {

                //   System.MessageBox.Show("Google Drive 계정이 없습니다.");
                return false;
            }

            // 1. 업로드 수행
            bool result = await googleDriveService.UploadFileAsync(googleAccount.AccountId, file_name);
            if (!result) return false;

            // 2. 파일 정보 추출
            var fileInfo = new FileInfo(file_name);

            CloudFileInfo file = new CloudFileInfo
            {
                FileName = fileInfo.Name,
                FileSize = (ulong)fileInfo.Length,
                UploadedAt = DateTime.Now,
                CloudStorageNum = googleAccount.CloudStorageNum,
                ParentFolderId = null, // 최상위 업로드라면 null
                IsFolder = false,
                Count = 0
            };

            // 3. DB 저장
            return fileService.SaveFile(file);
        }
    }
}
