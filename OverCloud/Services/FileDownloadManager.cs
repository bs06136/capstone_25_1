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
        private AccountService _accountService;
        private GoogleDriveService _GoogleDriveService;

        public FileDownloadManager(GoogleDriveService GoogleDriveService)
        {
            _GoogleDriveService = GoogleDriveService;
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

            //return await serviceMap[cloudType].DownloadFileAsync(userId, fileId, savePath);
            return await _GoogleDriveService.DownloadFileAsync(userId, fileId, savePath);
        }
    }


}