using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Service;

namespace OverCloud.Services
{
    public class FileDownloadManager
    {
        private readonly Dictionary<string, ICloudFileService> serviceMap;
        private readonly FileService fileService;
        private readonly FileOptimizerService optimizer;

        public FileDownloadManager(FileService fileService, FileOptimizerService optimizer)
        {
            this.fileService = fileService;
            this.optimizer = optimizer;

            serviceMap = new Dictionary<string, ICloudFileService>
            {
                { "GoogleDrive", new GoogleDriveService() }
                // Dropbox, OneDrive도 추가 가능
            };
        }

        public async Task<bool> DownloadFile(string userId, string cloudType, string cloudFileId, int fileId, string savePath)
        {
            if (!serviceMap.ContainsKey(cloudType))
            {
                Console.WriteLine($"❌ 지원되지 않는 클라우드: {cloudType}");
                return false;
            }

            bool result = await serviceMap[cloudType].DownloadFileAsync(userId, cloudFileId, savePath);

            if (result)
            {
                var file = fileService.GetFile(fileId);
                optimizer.OptimizeFileAfterDownload(file);
            }

            return result;
        }


    }
}