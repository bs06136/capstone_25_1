using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;

namespace OverCloud.Services
{
    public class QuotaManager
    {

        private readonly GoogleDriveService googleDriveService;
        private readonly DropboxService dropboxService;
        private readonly OneDriveService oneDriveService;
        private readonly IAccountRepository accountRepository;
        private readonly IStorageRepository storageRepository;
        private AccountService accountService;

        public QuotaManager()
        {
            storageRepository = new StorageRepository(DbConfig.ConnectionString);
            accountRepository = new AccountRepository(DbConfig.ConnectionString);

            ICloudFileService service = accountService.GetOneCloud("admin").CloudType switch
            {
                "GoogleDrive" => googleDriveService,
                //"Dropbox" => dropboxService,
                //"OneDrive" => oneDriveService,
                _ => throw new NotSupportedException()
            };
        }

        //계정에 있는 모든 스토리지의 용량 업데이트

        public bool UpdateAggregatedStorageForUser(string userId) //여기서 넘기는 userId는 overcloud계정의 id임요
        {
            // 1. 해당 계정이 가진 모든 클라우드 가져오기
            var cloudList = storageRepository.GetCloudsForUser(userId);
            if (cloudList == null || cloudList.Count == 0)
                return false;

            int userNum = cloudList.First().UserNum;

            // 2. 합산
            ulong totalSize = (ulong)cloudList.Sum(c => c.TotalCapacity);
            ulong usedSize = (ulong)cloudList.Sum(c => c.UsedCapacity);

            // 3. DB 업데이트
            return accountRepository.UpdateAccountUsage(userNum, totalSize, usedSize);
        }

        // 계정에 있는 특정 클라우드 하나만 용량 업데이트 (일단은 구글 드라이브 한정 DB에 업데이트)
        

        public async Task<bool> SaveDriveQuotaToDB(string userEmail, int CloudStorageNum)
        {
            // 1. 구글 API로부터 용량 정보 받아오기
            var (total, used) = await googleDriveService.GetDriveQuotaAsync(userEmail);

            // 2. 객체 생성
            CloudStorageInfo cloud = new CloudStorageInfo
            {
                CloudStorageNum = CloudStorageNum,
                TotalCapacity = (int)(total / 1048576),
                UsedCapacity = (int)(used / 1048576)
            };

            // 3. 저장
            return storageRepository.account_save(cloud);
        }


        //용량 합치기 AggregateQuota 메소드 사용. -> ui보여주기용.



    }

}