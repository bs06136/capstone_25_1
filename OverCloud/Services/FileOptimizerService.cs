using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;

namespace OverCloud.Services
{
    public class FileOptimizerService
    {
        private readonly IFileRepository fileRepo;
        private readonly IAccountRepository accountRepo;

        public FileOptimizerService()
        {
            fileRepo = new FileRepository(DbConfig.ConnectionString);
            accountRepo = new AccountRepository(DbConfig.ConnectionString);
        }

        public void OptimizeFileAfterDownload(CloudFileInfo file, string cloudFileId)
        {
            fileRepo.IncrementDownloadCount(file.FileId);

            if (file.Count + 1 >= 2) //일단은 다운로드 횟수가 두번 이상인경우 => 최적화
            {
                var storages = accountRepo.GetAllAccounts();
                var currentStorage = storages.FirstOrDefault(s => s.CloudStorageNum == file.CloudStorageNum);

                // 가장 여유 있는 클라우드 찾기
                var best = storages
                    .Where(s => s.CloudStorageNum != file.CloudStorageNum)
                    .OrderByDescending(s => s.TotalCapacity - s.UsedCapacity)
                    .FirstOrDefault();

                if (best != null)
                {
                    file.CloudStorageNum = best.CloudStorageNum; // 메모리 상 변경
                    fileRepo.change_file(file, cloudFileId); // DB에 반영
                    Console.WriteLine($"📦 파일 {file.FileName} → 클라우드 {best.CloudStorageNum}로 이전됨");
                }

            }
        }
    }

}