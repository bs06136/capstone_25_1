using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using DB.overcloud.Models;
using OverCloud.Services.FileManager.DriveManager;
using overcloud;
using OverCloud.Services.StorageManager;

namespace OverCloud.Services.FileManager
{
    public class FileDeleteManager
    {
        private readonly IAccountRepository accountRepository;
        private readonly QuotaManager quotaManager;
        private readonly IStorageRepository storageRepository;
        private readonly IFileRepository fileRepository;
        private readonly List<ICloudFileService> cloudServices;

       // private readonly Dictionary<string, ICloudFileService> serviceMap;


        public FileDeleteManager(
            IAccountRepository accountRepository,
            QuotaManager quotaManager,
            IStorageRepository storageRepository,
            IFileRepository fileRepository,
            List<ICloudFileService> cloudServices)
        {
            this.accountRepository = accountRepository;
            this.quotaManager = quotaManager;
            this.storageRepository = storageRepository;
            this.fileRepository = fileRepository;
            this.cloudServices = cloudServices;
        }

        // 파일 ID를 기반으로 삭제
        public async Task<bool> Delete_File(string userId, int fileId)
        {
            var file = fileRepository.GetFileById(fileId);
            if (file == null)
            {
                Console.WriteLine("❌ 삭제할 파일이 존재하지 않습니다.");
                return false;
            }

            var cloudInfo = accountRepository
               .GetAllAccounts(userId)
               .FirstOrDefault(c => c.CloudStorageNum == file.CloudStorageNum);

            string cloudType = cloudInfo.CloudType;
            var service = cloudServices.FirstOrDefault(s => s.GetType().Name.Contains(cloudType));
            if (service == null)
            {
                Console.WriteLine($"❌ 지원되지 않는 클라우드: {cloudType}");
                return false;
            }


            bool apiDeleted = await service.DeleteFileAsync(userId, file.cloud_file_id);
            if (!apiDeleted)
            {
                Console.WriteLine("❌ 클라우드 API에서 파일 삭제 실패");
                return false;
            }
                //파일 DB에서 삭제
            bool dbDeleted = fileRepository.DeleteFile(fileId);


            if (dbDeleted)
            {
                quotaManager.UpdateQuotaAfterUploadOrDelete(cloudInfo.CloudStorageNum, (ulong)((file.FileSize)/1024), false);
            }

            return dbDeleted;
        }

        //// (선택) 폴더 전체 삭제 등 확장 가능
        //public bool DeleteFolderRecursively(int folderId)
        //{
        //    // 하위 파일, 폴더 탐색 및 재귀적 삭제 로직 추가 가능
        //    // (이건 추후 구현)
        //    return false;
        //}
    }


}