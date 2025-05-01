using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;
using OverCloud.Services.FileManager.DriveManager;

namespace OverCloud.Services.FileManager
{
    public class FileDownloadManager
    {
      //  private readonly Dictionary<string, ICloudFileService> serviceMap;
         private readonly IAccountRepository acountRepository;
        private readonly List<ICloudFileService> cloudServices;

//        new GoogleDriveService(new GoogleTokenProvider() , new StorageRepository(DbConfig.ConnectionString) )
        public FileDownloadManager(
            IAccountRepository acountRepository,
            List<ICloudFileService> cloudServices)

        {
            this.acountRepository = acountRepository;
            this.cloudServices = cloudServices;
        }

        public async Task <bool> DownloadFile(string userId, string cloudFileId, int CloudStorageNum, string savePath)
        {
            Console.WriteLine(userId);
            Console.WriteLine("DownloadFile");

            var clouds = acountRepository.GetAllAccounts(userId);
            var cloudInfo = clouds.FirstOrDefault(c => c.CloudStorageNum == CloudStorageNum);
            if (cloudInfo == null)
            {
                Console.WriteLine("❌ 클라우드 정보 없음");
                return false;
            }

            string cloudType = cloudInfo.CloudType;
            var service = cloudServices.FirstOrDefault(s => s.GetType().Name.Contains(cloudType));
            if (service == null)
            {
                Console.WriteLine($"❌ 지원되지 않는 클라우드");
                return false;
            }


            bool result = await service.DownloadFileAsync(cloudInfo.AccountId, cloudFileId, savePath);
            return result;
        }


    }
}