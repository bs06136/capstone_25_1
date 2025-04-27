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
using OverCloud.Services.FileManager.DriveManager;
using OverCloud.Services.StorageManager;


namespace OverCloud.Services.FileManager
{
    public class FileUploadManager
    {
        private readonly AccountService accountService;
        //private readonly GoogleDriveService googleDriveService;
        private readonly IFileRepository repo_file;
        private readonly TokenProviderFactory tokenFactory;
        private readonly IAccountRepository accountRepo;
        private readonly CloudTierManager cloudTierManager;
        private readonly QuotaManager quotaManager;
        private readonly IStorageRepository storageRepository;
        private readonly ICloudFileService cloudService;

        public FileUploadManager(
            AccountService accountService,
            QuotaManager quotaManager,
            IStorageRepository storageRepository,
            IFileRepository repo_file,
            ICloudFileService cloudService,
            CloudTierManager cloudTierManager)

        {
            this.accountService = accountService;
            this.quotaManager = quotaManager; 
            this.storageRepository = storageRepository;
            this.repo_file = repo_file;
            this.cloudService = cloudService;
            this.cloudTierManager = cloudTierManager;
          
        }

        public async Task<bool> file_upload(string file_name)
        {
            var cloud = cloudTierManager.SelectBestStorage((ulong)new FileInfo(file_name).Length);
            if (cloud == null) return false;


            var cloudFileId = await cloudService.UploadFileAsync(cloud.AccountId, file_name);
            // 1. 업로드 수행
            // 1. 파일 업로드 후 Google Drive 내부 파일 ID 반환
            if (string.IsNullOrEmpty(cloudFileId)) return false;

            // 2. 파일 정보 추출
            var fileInfo = new FileInfo(file_name);

            quotaManager.UpdateQuotaAfterUploadOrDelete(cloud.CloudStorageNum, (int)(fileInfo.Length / 1048576), true);

            CloudFileInfo file = new CloudFileInfo
            {
                FileName = fileInfo.Name,
                FileSize = (ulong)(fileInfo.Length/1048576),
                UploadedAt = DateTime.Now,
                CloudStorageNum = cloud.CloudStorageNum,
                ParentFolderId = null, // 최상위 업로드라면 null ,일단은 파일만처리, 나중에는 폴더까지 
                IsFolder = false, 
                Count = 0,
                GoogleFileId = cloudFileId
            };

            // 3. DB 저장
            repo_file.addfile(file);

            return true;
        }

    }
}