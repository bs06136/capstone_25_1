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
        private readonly IAccountRepository accountRepo;
        private readonly CloudTierManager cloudTierManager;

        public FileUploadManager()
        {
            accountService = new AccountService();
            googleDriveService = new GoogleDriveService(new GoogleTokenProvider(), new StorageRepository(DbConfig.ConnectionString));
            tokenFactory = new TokenProviderFactory();
            cloudTierManager = new CloudTierManager(accountService);
          
            repo_file = new FileRepository(DbConfig.ConnectionString);
            fileService = new FileService(repo_file);

        }
        public async Task<bool> file_upload(string file_name)
        {
            var cloud = cloudTierManager.SelectBestStorage((ulong)new FileInfo(file_name).Length);
            if (cloud == null) return false;


            var cloudFileId = await googleDriveService.UploadFileAsync(cloud.AccountId, file_name);
            // 1. 업로드 수행
            // 1. 파일 업로드 후 Google Drive 내부 파일 ID 반환
            if (string.IsNullOrEmpty(cloudFileId)) return false;


            // 2. 파일 정보 추출
            var fileInfo = new FileInfo(file_name);

            CloudFileInfo file = new CloudFileInfo
            {
                FileName = fileInfo.Name,
                FileSize = (ulong)fileInfo.Length,
                UploadedAt = DateTime.Now,
                CloudStorageNum = cloud.CloudStorageNum,
                ParentFolderId = null, // 최상위 업로드라면 null ,일단은 파일만처리, 나중에는 폴더까지 
                IsFolder = false, 
                Count = 0,
                GoogleFileId = cloudFileId
            };

            // 3. DB 저장
            fileService.SaveFile(file);
            return true;
        }

    }
}