using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OverCloud.Models;
using OverCloud.Services;

namespace OverCloud.Services
{
    public class FileDownloadManager
    {
        private readonly Dictionary<string, ICloudFileService> serviceMap;

        public FileDownloadManager()
        {
            serviceMap = new Dictionary<string, ICloudFileService>
        {
            { "GoogleDrive", new GoogleDriveService() }
            // 앞으로 Dropbox, OneDrive 추가 가능
        };
        }

        public async Task<bool> DownloadFile(string cloudType, string userId, string fileId, string savePath)
        {
            if (!serviceMap.ContainsKey(cloudType))
            {
                Console.WriteLine($"❌ 지원되지 않는 클라우드 타입: {cloudType}");
                return false;
            }

            return await serviceMap[cloudType].DownloadFileAsync(userId, fileId, savePath);
        }
    }


}