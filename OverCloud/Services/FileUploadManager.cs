using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DB.overcloud.Models;
using System.IO;
using DB.overcloud.Repository;
using overcloud;


namespace OverCloud.Services
{
    public class FileUploadManager
    {
        private readonly AccountService accountService;
        private readonly GoogleDriveService googleDriveService;
        private readonly FileService fileService;
        private readonly IFileRepository repo_file;
        private readonly TokenProviderFactory tokenFactory;
        private readonly IStorageRepository storageRepository;

        public FileUploadManager()
        {
            accountService = new AccountService();
            googleDriveService = new GoogleDriveService(new GoogleTokenProvider(), new StorageRepository(DbConfig.ConnectionString));
            tokenFactory = new TokenProviderFactory();

            googleDriveService = new GoogleDriveService();
          
            repo_file = new FileRepository(DbConfig.ConnectionString);
            fileService = new FileService(repo_file);

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

            // 1. 업로드 수행
            // 1. 파일 업로드 후 Google Drive 내부 파일 ID 반환
            var googleFileId = await googleDriveService.UploadFileAsync(googleAccount.AccountId, file_name);
            if (string.IsNullOrEmpty(googleFileId)) return false;


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
                Count = 0,
                GoogleFileId = googleFileId
            };

            // 3. DB 저장
            fileService.SaveFile(file);
            return true;
        }

    }
}