using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;
using OverCloud.Services.FileManager.DriveManager;

namespace OverCloud.Services.StorageManager
{
    public class QuotaManager
    {

        private readonly IEnumerable<ICloudFileService> cloudServices;
        // private readonly DropboxService dropboxService;
        //private readonly OneDriveService oneDriveService;
        private readonly IAccountRepository accountRepository;
        private readonly IStorageRepository storageRepository;
        private readonly IFileRepository fileRepository;
        private readonly CloudTierManager cloudTierManager;

        public QuotaManager(IEnumerable<ICloudFileService> cloudServices, IStorageRepository storageRepo, IAccountRepository accountRepo,IFileRepository fileRepository, CloudTierManager cloudTierManager)
        {
            storageRepository = storageRepo;
            accountRepository = accountRepo;
            this.fileRepository = fileRepository;
            this.cloudServices = cloudServices;
            this.cloudTierManager = cloudTierManager;
        }

        //계정에 있는 모든 스토리지의 용량 업데이트

        public bool UpdateAggregatedStorageForUser(string userId) //여기서 넘기는 userId는 overcloud계정의 id임요.
        {
            // 1. 해당 계정이 가진 모든 클라우드 가져오기
            var cloudList = accountRepository.GetAllAccounts(userId);
            if (cloudList == null || cloudList.Count == 0)
                return false;

//            int userNum = cloudList.First().UserNum;

            // 2. 합산
            ulong totalSize = cloudList.Aggregate(0UL, (acc, c) => acc + c.TotalCapacity);
            ulong usedSize = cloudList.Aggregate(0UL, (acc, c) => acc + c.UsedCapacity);


            // 3. DB 업데이트
            return accountRepository.UpdateAccountUsage(userId, totalSize, usedSize);
        }

      



        // 계정에 있는 특정 클라우드 하나만 용량 업데이트 (일단은 구글 드라이브 한정 DB에 업데이트)
        public async Task<bool> SaveDriveQuotaToDB(string userId, int CloudStorageNum) //오버클라우드 userid를 넘겨줌.
        {
           
            // 1. userEmail에 맞는 클라우드 타입 찾기
            var cloudInfo = storageRepository.GetCloud(CloudStorageNum, userId);
            if (cloudInfo == null)
            {
                Console.WriteLine("❌ 해당 이메일에 맞는 클라우드 정보를 찾을 수 없습니다.");
                return false;
            }

            string cloudType = cloudInfo.CloudType;


                 // 2. 클라우드 타입에 맞는 서비스 찾기
            var service = cloudServices.FirstOrDefault(s =>
                s.GetType().Name.StartsWith(cloudType)); // "GoogleDriveService", "DropboxService" 같은 이름 비교

            if (service == null)
            {
                Console.WriteLine("❌ 해당 클라우드에 맞는 서비스 구현체를 찾을 수 없습니다.");
                return false;
            }

            // 3. 해당 클라우드에 API 호출
            var (total, used) = await service.GetDriveQuotaAsync(CloudStorageNum, userId);


            // 5. TotalCapacity, UsedCapacity만 업데이트 (KB단위)
            cloudInfo.TotalCapacity = (total / 1024);
            cloudInfo.UsedCapacity = (used / 1024);

            // 💡 메모리 세션도 갱신
            StorageSessionManager.SetQuota(
                cloudStorageNum: cloudInfo.CloudStorageNum,
                accountId : cloudInfo.AccountId,
                cloudType: cloudInfo.CloudType,
                totalKB: cloudInfo.TotalCapacity,
                usedKB: cloudInfo.UsedCapacity
            );

            // 3. 저장
            return storageRepository.account_save(cloudInfo);
        }



        //업로드 or 삭제 시 스토리지 용량 최신화.
        public void UpdateQuotaAfterUploadOrDelete(int cloudStorageNum, ulong fileSizeKB, bool isUpload)
        {
            var quota = StorageSessionManager.Quotas.FirstOrDefault(q => q.CloudStorageNum == cloudStorageNum);
            Console.WriteLine($" 업로드 or 삭제 반영 전: quota.Total = {quota.TotalCapacityKB}");
            Console.WriteLine($" 업로드 or 삭제 반영 전: quota.Used = {quota.UsedCapacityKB}");
            if (quota == null)
            {
                Console.WriteLine($"❌ quota not found for CloudStorageNum: {cloudStorageNum}");
                return;
            }

            if (isUpload)
                quota.UsedCapacityKB += fileSizeKB;
            else
                quota.UsedCapacityKB -= fileSizeKB;


            Console.WriteLine($" 업로드 or 삭제 반영 후: quota.Used = {quota.UsedCapacityKB}");

            var cloudInfo = new CloudStorageInfo
            {
                CloudStorageNum = quota.CloudStorageNum,
                TotalCapacity = quota.TotalCapacityKB,
                UsedCapacity = quota.UsedCapacityKB
            };

            bool dbResult = storageRepository.account_save(cloudInfo);
            Console.WriteLine(dbResult ? "✅ DB 저장 성공" : "❌ DB 저장 실패");

        }


        public ulong GetTotalRemainingQuotaInBytes_Delete_Account(string userId, int cloudStroageNum)
        {
            var clouds = accountRepository.GetAllAccounts(userId);
            if (clouds == null || clouds.Count == 0)
                return 0;

            ulong totalAvailableBytes = 0;

            foreach (var cloud in clouds)
            {
                ulong remainingKB = cloud.TotalCapacity - cloud.UsedCapacity;
                totalAvailableBytes += remainingKB * 1024; // KB → byte
            }

            var delete_cloud = storageRepository.GetCloud(cloudStroageNum, userId);
            ulong remainingKB_Delete_Cloud = delete_cloud.TotalCapacity - delete_cloud.UsedCapacity;
            totalAvailableBytes -= remainingKB_Delete_Cloud * 1024;

            return totalAvailableBytes;
        }

        public ulong AllFilelistSize(int CloudStorageNum)
        {
            var files = fileRepository.GetFilesByStorageNum(CloudStorageNum);

            ulong totalSize = 0;

            foreach (var file in files)
            {
                totalSize += (ulong)file.FileSize;  // file.Size를 ulong으로 캐스팅
            }

            return totalSize;
        }



        public async Task<bool> AccountFile_Redistribution(int cloudStorageNum, string userId)
        {
            // 1. 삭제될 계정의 파일 목록 조회
            var filesToRedistribute = fileRepository.GetFilesByStorageNum(cloudStorageNum);
            if (filesToRedistribute == null || filesToRedistribute.Count == 0)
            {
                Console.WriteLine("재분배할 파일이 없습니다.");
                return true; // 아무 파일도 없으므로 성공 처리
            }

            foreach (var file in filesToRedistribute)
            {
                try
                {
                    var cloud = storageRepository.GetCloud(cloudStorageNum, userId);
                    var sourceService = cloudServices.FirstOrDefault(s => s.GetType().Name.Contains(cloud.CloudType));
                    if (sourceService == null)
                    {
                        Console.WriteLine($"❌ 클라우드 서비스 없음 (cloudType: {cloud.CloudType})");
                        continue;
                    }

                    string tempPath = Path.GetTempFileName();
                    bool downloaded = await sourceService.DownloadFileAsync(file.CloudStorageNum, file.CloudFileId, tempPath,userId);
                    if (!downloaded)
                    {
                        Console.WriteLine($"❌ 파일 다운로드 실패: {file.FileName}");
                        continue;
                    }

                    var candidateStorages = cloudTierManager.GetCandidateStorages(file.FileSize / 1024, userId);
                    if (candidateStorages == null)
                    {
                        Console.WriteLine($"❌ 적절한 스토리지가 없음. 파일: {file.FileName}");
                        File.Delete(tempPath);
                        continue;
                    }

                    bool uploadSuccess = false;

                    foreach (var bestStorage in candidateStorages)
                    {
                        var targetService = cloudServices.FirstOrDefault(s => s.GetType().Name.Contains(bestStorage.CloudType));
                        if (targetService == null)
                        {
                            Console.WriteLine($"❌ 대상 클라우드 서비스 없음: {bestStorage.CloudType}");
                            continue;
                        }

                        string newCloudFileId = await targetService.UploadFileAsync(bestStorage, tempPath, userId);
                        if (!string.IsNullOrEmpty(newCloudFileId))
                        {
                            // 업로드 성공 처리
                            file.CloudStorageNum = bestStorage.CloudStorageNum;
                            file.CloudFileId = newCloudFileId;
                            fileRepository.updateFile(file);
                            UpdateQuotaAfterUploadOrDelete(bestStorage.CloudStorageNum, file.FileSize / 1024, true);
                            uploadSuccess = true;
                            Console.WriteLine($"✅ 파일 재분배 성공: {file.FileName} -> {bestStorage.CloudType}");
                            break; // 반복문 종료 (업로드 성공)
                        }

                        Console.WriteLine($"❌ 업로드 실패: {file.FileName} 대상: {bestStorage.CloudType}");
                    }

                    File.Delete(tempPath); // 임시 파일 삭제

                    if (!uploadSuccess)
                    {
                        Console.WriteLine($"❌ 모든 스토리지 업로드 실패: {file.FileName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ 예외 발생: {ex.Message}");
                }
            }

            // 7. 모든 파일 재분배 후 집계 갱신
            UpdateAggregatedStorageForUser(userId);
            return true;
        }






    }

}