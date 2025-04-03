using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;

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

        public ulong TotalCapacity => (ulong)accounts.Sum(a => a.TotalSize);
        public ulong UsedCapacity => (ulong)accounts.Sum(a => a.UsedSize);
        public ulong Remaining => TotalCapacity - UsedCapacity;
        public double UsagePercent => TotalCapacity == 0 ? 0 : (double)UsedCapacity / TotalCapacity * 100;
    }

}
