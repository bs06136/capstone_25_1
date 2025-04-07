using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;

namespace OverCloud.Services
{
    public class FileDownloadManager
    {
        private readonly Dictionary<string, ICloudFileService> serviceMap;
        private readonly FileService fileService;
        private readonly FileOptimizerService optimizer;
        private readonly IFileRepository fileRepo;

        public FileDownloadManager()
        {
            fileRepo = new FileRepository(DbConfig.ConnectionString);
            fileService = new FileService(fileRepo);
            optimizer = new FileOptimizerService();

            serviceMap = new Dictionary<string, ICloudFileService>
            {
                { "GoogleDrive", new GoogleDriveService() }
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

            if (result)
            {
                var file = fileService.GetFile(fileId);
                optimizer.OptimizeFileAfterDownload(file, cloudFileId);
                fileService.SaveFile(file);
            }

            return result;
        }


    }
}