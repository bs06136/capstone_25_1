using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using DB.overcloud.Models;
using overcloud;

namespace OverCloud.Services
{
    public class FileDeleteManager
    {
        private readonly Dictionary<string, ICloudFileService> serviceMap;
        private readonly FileService fileService;
        private readonly IFileRepository fileRepository;
        private readonly IStorageRepository storageRepository;

        public FileDeleteManager()
        {
            fileRepository = new FileRepository(DbConfig.ConnectionString);
            fileService = new FileService(fileRepository);

            // 현재는 GoogleDrive만 연동
            serviceMap = new Dictionary<string, ICloudFileService>
            {
                { "GoogleDrive", new GoogleDriveService(new GoogleTokenProvider(), storageRepository) }
                // 추후 OneDrive, Dropbox 추가 가능
            };


        }

        // 파일 ID를 기반으로 삭제
        public async Task<bool> DeleteFile(string userId, int fileId)
        {
            var file = fileService.GetFile(fileId);
            if (file == null)
            {
                Console.WriteLine("❌ 삭제할 파일이 존재하지 않습니다.");
                return false;
            }

            string cloudType = storageRepository
               .GetCloudsForUser(userId)
               .FirstOrDefault(c => c.CloudStorageNum == file.CloudStorageNum)?.CloudType;

            if (string.IsNullOrEmpty(cloudType) || !serviceMap.ContainsKey(cloudType))
            {
                Console.WriteLine("❌ 지원되지 않는 클라우드 타입입니다.");
                return false;
            }

            var service = serviceMap[cloudType];
            bool apiDeleted = await service.DeleteFileAsync(userId, file.GoogleFileId);

            if (apiDeleted)
            {
                return fileRepository.DeleteFile(fileId);
            }

            return false;

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