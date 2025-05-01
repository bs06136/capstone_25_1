using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;

namespace OverCloud.Services
{
    public class CloudTierManager
    {

        private readonly AccountRepository accountRepository;

        public CloudTierManager(AccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        public CloudStorageInfo SelectBestStorage(ulong fileSize)
        {

            var clouds = accountRepository.GetAllAccounts("admin");
            if (clouds == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ 클라우드 계정 없음");
                return null;
            }
                
                  // 사전 정의된 티어 순서
            var ordered = clouds
                .OrderBy(c => GetTierValue(c.CloudType))  //1순위: 티어 순서 
                .ThenByDescending(c => c.TotalCapacity - c.UsedCapacity); // 같은 티어일 땐 여유 공간 많은 순

            foreach (var cloud in ordered)
            {
                var remaining = (ulong)(cloud.TotalCapacity - cloud.UsedCapacity);
                Console.WriteLine($"🧪 클라우드: {cloud.CloudType}, 잔여용량: {remaining}KB");

                if (remaining >= fileSize / 1024) // KB 단위 맞추기
                    return cloud;
            }

            Console.WriteLine("❌ 저장 가능한 클라우드가 없습니다.");
            return null; // 저장 가능한 클라우드 없음
        }

        private int GetTierValue(string cloudType)
        {
            return cloudType switch
            {
                "OneDrive" => 1,
                "GoogleDrive" => 2,
                "Dropbox" => 3,
                _ => 99 // 기타 클라우드
            };
        }
    }


}