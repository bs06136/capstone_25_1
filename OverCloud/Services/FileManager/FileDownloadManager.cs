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
        private readonly Dictionary<string, ICloudFileService> serviceMap;
        private readonly IFileRepository fileRepo;
        private readonly GoogleTokenProvider googleTokenProvider;
        private readonly IStorageRepository storageRepository;

//        new GoogleDriveService(new GoogleTokenProvider() , new StorageRepository(DbConfig.ConnectionString) )
        public FileDownloadManager(IStorageRepository storageRepository, ICloudFileService cloudService)
        {
            this.storageRepository = storageRepository;
           // fileRepo = FileRepository;
            

           
            serviceMap = new Dictionary<string, ICloudFileService>
            {
                { "GoogleDrive", cloudService }
                // Dropbox, OneDrive도 추가 가능
            };
        }

        public async Task <bool> DownloadFile(string userId, string cloudFileId, int fileId, string savePath)
        {
            if (!serviceMap.ContainsKey("GoogleDrive"))
            {
                Console.WriteLine($"❌ 지원되지 않는 클라우드: {"GoogleDrive"}");
                return false;
            }



            bool result = await serviceMap["GoogleDrive"].DownloadFileAsync(userId, cloudFileId, savePath);

            //if (result)
            //{
            //    var file = fileService.GetFile(fileId);
            //    if(file != null)
            //    {
            //        fileRepo.addfile(file);

            //    }
            //}

            return result;
        }


    }
}