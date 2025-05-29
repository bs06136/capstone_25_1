using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;

namespace OverCloud.Services.StorageManager
{
    public class CloudQuotaInfo
    {
     
        public string AccountId { get; set; }
        public int CloudStorageNum { get; set; }
        public string CloudType { get; set; }
        public ulong TotalCapacityKB { get; set; }
        public ulong UsedCapacityKB { get; set; }
    }

    public static class StorageSessionManager
    {
        public static List<CloudQuotaInfo> Quotas { get; set; } = new List<CloudQuotaInfo>();

        public static void SetQuota(int cloudStorageNum, string accountId, string cloudType, ulong totalKB, ulong usedKB)
        {
            var existing = Quotas.FirstOrDefault(q =>
                q.CloudStorageNum == cloudStorageNum ||
                (q.AccountId == accountId && q.CloudType == cloudType)
            );
            
            if (existing != null)
            {
                existing.TotalCapacityKB = totalKB;
                existing.UsedCapacityKB = usedKB;
                existing.CloudStorageNum = cloudStorageNum; // 중요!
            }
            else
            {
                Quotas.Add(new CloudQuotaInfo
                {
                    CloudStorageNum = cloudStorageNum,
                    AccountId = accountId,
                    CloudType = cloudType,
                    TotalCapacityKB = totalKB,
                    UsedCapacityKB = usedKB
                });
            }
        }
       
        /// <summary>
        /// 특정 클라우드 id와 타입 기준으로 삭제 (옵션)
        /// </summary>
        public static void RemoveQuota(string accountId, string cloudType)
        {
            Quotas.RemoveAll(q => q.AccountId == accountId && q.CloudType == cloudType);
        }

        /// <summary>
        /// 전체 사용 가능 용량 계산
        /// </summary>
        public static ulong GetTotalAvailableCapacityKB()
        {
            ulong total = 0;

            foreach (var q in Quotas)
            {
                if (q.TotalCapacityKB > q.UsedCapacityKB)
                    total += (q.TotalCapacityKB - q.UsedCapacityKB);
            }

            return total;
        }


        /// <summary>
        /// 현재 전체 총 용량 및 사용량 반환
        /// </summary>
        public static (ulong totalKB, ulong usedKB) GetAggregatedUsage()
        {
            ulong total = 0, used = 0;
            foreach (var q in Quotas)
            {
                total += q.TotalCapacityKB;
                used += q.UsedCapacityKB;
            }
            return (total, used);
        }

        /// <summary>
        /// 모든 세션 초기화
        /// </summary>
        public static void Clear()
        {
            Quotas.Clear();
        }

        public static void InitializeFromDatabase(List<CloudStorageInfo> storages)
        {
            Quotas.Clear(); // 기존 세션 초기화
            foreach (var s in storages)
            {
                SetQuota(
                    s.CloudStorageNum,
                    s.AccountId,
                    s.CloudType,
                    s.TotalCapacity,
                    s.UsedCapacity
                );
            }

            Console.WriteLine($"✅ StorageSessionManager 초기화 완료: {Quotas.Count}개 로드됨");
        }


    }



}