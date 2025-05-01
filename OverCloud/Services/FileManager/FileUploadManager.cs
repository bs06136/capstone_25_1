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
        private readonly CloudTierManager cloudTierManager;
        private readonly QuotaManager quotaManager;
        private readonly IStorageRepository storageRepository;
        private readonly List<ICloudFileService> cloudServices;

        public FileUploadManager(
            AccountService accountService,
            QuotaManager quotaManager,
            IStorageRepository storageRepository,
            IFileRepository repo_file,
            List<ICloudFileService> cloudServices,
            CloudTierManager cloudTierManager)
        {
            this.accountService = accountService;
            this.quotaManager = quotaManager; 
            this.storageRepository = storageRepository;
            this.repo_file = repo_file;
            this.cloudServices = cloudServices;
            this.cloudTierManager = cloudTierManager;
        }

        public async Task<bool> file_upload(string file_name, int target_parent_file_id)
        {
            ulong fileSize = (ulong)new FileInfo(file_name).Length;

                // 1. 업로드 가능한 스토리지 선택
            var cloud = cloudTierManager.SelectBestStorage(fileSize);
            if (cloud == null) return false;


            string cloudType = cloud.CloudType;

                // 2. 클라우드 타입에 맞는 서비스 찾기
            var service = cloudServices.FirstOrDefault(s => s.GetType().Name.Contains(cloudType));
            if (service == null)
            {
                Console.WriteLine($"❌ 지원되지 않는 클라우드: {cloudType}");
                return false;
            }

            // 3. 파일 업로드
            var cloudFileId = await service.UploadFileAsync(cloud.AccountId, file_name);
            if (string.IsNullOrEmpty(cloudFileId)) return false;

            // 4. 파일 정보 저장
            var fileInfo = new FileInfo(file_name);
            CloudFileInfo file = new CloudFileInfo
            {
                FileName = fileInfo.Name,
                FileSize = (ulong)((fileInfo.Length)/1024),
                UploadedAt = DateTime.Now,
                CloudStorageNum = cloud.CloudStorageNum,
                ParentFolderId = target_parent_file_id, // 최상위 업로드라면 -1 ,일단은 파일만처리, 나중에는 폴더까지 
                IsFolder = false,
                CloudFileId = cloudFileId,
            };

            // 5. DB 저장
            repo_file.addfile(file);


            // 6. 업로드 후 용량 갱신
            quotaManager.UpdateQuotaAfterUploadOrDelete(cloud.CloudStorageNum, (ulong)((fileInfo.Length)/1024), true);

            return true;
        }

    }
}