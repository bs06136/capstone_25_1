using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverCloud.Models;

namespace OverCloud.Services
{
    // CloudStorageMerger: 통합 용량 계산
    public class CloudStorageMerger
    {
        private readonly List<CloudStorageInfo> accounts;

        public CloudStorageMerger(List<CloudStorageInfo> accounts)
        {
            this.accounts = accounts;
        }

        public long TotalCapacity => accounts.Sum(a => a.TotalCapacity);
        public long UsedCapacity => accounts.Sum(a => a.UsedCapacity);
        public long Remaining => TotalCapacity - UsedCapacity;
        public double UsagePercent => TotalCapacity == 0 ? 0 : (double)UsedCapacity / TotalCapacity * 100;
    }

}
